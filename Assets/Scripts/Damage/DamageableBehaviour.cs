using Core;
using Game;
using UnityEngine;

namespace Damage {

    public class DamageableBehaviour : MonoBehaviour
    {
        [Header("Health Settings")][Space]
        [SerializeField] float _maxHealth = 50f;
        [SerializeField] float _hitRecoveryTime = 0.2f;
        [SerializeField] bool _destroyOnDeath = false;

        [Header("Shield Settings")][Space]
        [SerializeField] float _maxShield = 0f;
        [SerializeField] float _shieldWaitTime = 3f;
        [SerializeField] float _shieldRechargeTime = 1f;

        // Action callbacks can be set via the Editor OR via RegisterCallbacks()
        [SerializeField] System.Action _onDeath;
        [SerializeField] System.Action<float> _onHealthTaken;
        [SerializeField] System.Action<float, DamageType> _onHealthDamage;
        [SerializeField] System.Action _onShieldDepleted;
        [SerializeField] System.Action<float> _onShieldDamage;
        [SerializeField] System.Action<float> _onShieldDrain;
        [SerializeField] System.Action _onShieldRechargeStart;
        [SerializeField] System.Action _onShieldRechargeComplete;

        // cached
        System.Guid _uuid = System.Guid.NewGuid();
        protected Collider2D[] colliders;
        DamageClass damageClass;

        // state
        bool _isAlive = true;
        float _health = 50f;
        float _shield = 100f;
        float _timeHit = 0f;
        float _healthDamageThisFrame = 0f;
        float _shieldDamageThisFrame = 0f;

        float _prevShield = 0f;
        float _timeShieldHit = 0f;
        bool _isRechargingShield = false;

        // getters
        public System.Guid uuid => _uuid;
        public bool isAlive => _isAlive;
        public float health => _health;
        public float shield => _shield;
        public float timeHit => _timeHit;
        public bool isRechargingShield => _isRechargingShield;
        public float hitRecoveryTime => _hitRecoveryTime;

        protected void TickHealth() {
            _timeHit = Mathf.Clamp(_timeHit - Time.deltaTime, 0f, _hitRecoveryTime);
            _timeShieldHit = Mathf.Clamp(_timeShieldHit - Time.deltaTime / _shieldWaitTime, 0f, _shieldWaitTime);

            // recharge shield
            if (_timeShieldHit <= 0f && _shield < _maxShield) {
                // NOTE - may also find it useful to add an `_onShieldRechargeStep` callback method at some point if needed for UI updates, etc.
                if (!_isRechargingShield) InvokeCallback(_onShieldRechargeStart, true);
                _isRechargingShield = true;
                _shield = Mathf.Min(_shield + _maxShield * (Time.deltaTime / _shieldRechargeTime), _maxShield);
                Debug.Log("recharge_shield >> " + _shield);
            } else {
                if (_isRechargingShield && _shield == _maxShield) InvokeCallback(_onShieldRechargeComplete, true);
                _isRechargingShield = false;
            }
        }

        protected void RegisterHealthCallbacks(
            System.Action onDeath,
            System.Action<float, DamageType> onHealthDamage,
            System.Action<float> onHealthTaken
        ) {
            _onDeath = onDeath;
            _onHealthDamage = onHealthDamage;
            _onHealthTaken = onHealthTaken;
        }
        protected void RegisterShieldCallbacks(
            System.Action onShieldDepleted,
            System.Action<float> onShieldDamage,
            System.Action<float> onShieldDrain,
            System.Action onShieldRechargeStart,
            System.Action onShieldRechargeComplete
        ) {
            _onShieldDepleted = onShieldDepleted;
            _onShieldDamage = onShieldDamage;
            _onShieldDrain = onShieldDrain;
            _onShieldRechargeStart = onShieldRechargeStart;
            _onShieldRechargeComplete = onShieldRechargeComplete;
        }

        protected void ResetHealth() {
            _isAlive = true;
            _health = _maxHealth;
            _shield = _maxShield;
            _timeHit = 0f;
        }

        protected void SetColliders() {
            colliders = transform.GetComponentsInChildren<Collider2D>(true);
            _EnableColliders();
        }

        public void IgnoreCollider(Collider2D other) {
            foreach (var collider in colliders) {
                Physics2D.IgnoreCollision(other, collider);
            }
        }

        public bool DrainShield(float amount) {
            if (!_isAlive) return false;
            if (_shield > 0f && amount > 0f) {
                _timeShieldHit = 1f;
                InvokeCallback(_onShieldDrain, amount, true);
            }
            _shield = Mathf.Max(_shield - amount, 0f);

            // TODO: REMOVE
            Debug.Log("shield >> " + _shield);

            return true;
        }

        public bool TakeHealth(float amount) {
            if (!_isAlive) return false;

            _health += amount;
            InvokeCallback(_onHealthTaken, amount, true);

            return true;
        }

        public bool TakeDamage(float amount, DamageType damageType = DamageType.Default) {
            if (!_isAlive || _timeHit > 0f) return false;

            damageClass = GameManager.current.GetDamageClass(damageType);

            // determine damage for health, shield
            _healthDamageThisFrame = _shield > 0f
                ? amount * damageClass.shieldedHealthEffectiveness
                : amount * damageClass.unshieldedHealthEffectiveness;
            _shieldDamageThisFrame = amount * damageClass.shieldEffectiveness;

            // determine damage by actor type
            if (gameObject.tag == UTag.Player) {
                _healthDamageThisFrame *= damageClass.playerEffectiveness;
                _shieldDamageThisFrame *= damageClass.playerEffectiveness;
            }
            if (gameObject.tag == UTag.EnemyShip) {
                _healthDamageThisFrame *= damageClass.enemyEffectiveness;
                _shieldDamageThisFrame *= damageClass.enemyEffectiveness;
            }

            if (_healthDamageThisFrame > 0f) {
                InvokeCallback(_onHealthDamage, _healthDamageThisFrame, damageType);
            }
            if (_shieldDamageThisFrame > 0f && _shield > 0f) {
                InvokeCallback(_onShieldDamage, _shieldDamageThisFrame, true);
            }

            _health -= _healthDamageThisFrame;
            _shield = Mathf.Max(_shield - _shieldDamageThisFrame, 0f);
            _timeHit = _hitRecoveryTime;
            _timeShieldHit = 1f;

            if (_shield == 0f && _prevShield > _shield) {
                InvokeCallback(_onShieldDepleted, true);
            }

            _prevShield = _shield;

            if (_health <= 0f) {
                if (_isAlive) _Die();
            }

            return true;
        }

        private void _Die() {
            _isAlive = false;
            _DisableColliders();
            InvokeCallback(_onDeath);
            if (_destroyOnDeath) Destroy(gameObject);
        }

        private void _EnableColliders() {
            foreach (var collider in colliders) {
                collider.enabled = true;
            }
        }

        private void _DisableColliders() {
            foreach (var collider in colliders) {
                collider.enabled = false;
            }
        }

        // CALLBACK STUFF

        private void InvokeCallback(System.Action action, bool ignoreMissingCallback = false) {
            if (action != null) {
                action.Invoke();
            } else if (!ignoreMissingCallback) {
                Debug.LogError("WARN: a callback was null in DamageableBehaviour - something's not hooked up correctly.");
            }
        }
        private void InvokeCallback(System.Action<float> action, float amount, bool ignoreMissingCallback = false) {
            if (action != null) {
                action.Invoke(amount);
            } else if (!ignoreMissingCallback) {
                Debug.LogError("WARN: a callback was null in DamageableBehaviour - something's not hooked up correctly.");
            }
        }
        private void InvokeCallback(System.Action<float, DamageType> action, float amount, DamageType damageType, bool ignoreMissingCallback = false) {
            if (action != null) {
                action.Invoke(amount, damageType);
            } else if (!ignoreMissingCallback) {
                Debug.LogError("WARN: a callback was null in DamageableBehaviour - something's not hooked up correctly.");
            }
        }
    }
}


