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
        [SerializeField] System.Action<float> _onShieldDrained;
        [SerializeField] System.Action<float> _onHealthTaken;
        [SerializeField] System.Action<float> _onDamageTaken;
        [SerializeField] System.Action _onDeath;

        // cached
        DamageClass damageClass;

        // state
        bool _isAlive = true;
        float _health = 50f;
        float _shield = 100f;
        float _timeHit = 0f;
        float _healthDamageThisFrame = 0f;
        float _shieldDamageThisFrame = 0f;

        // TODO: REMOVE
        // float _shieldedHealthDamage = 0f;
        // float _shieldDamage = 1f;
        // float _unshieldedHealthDamage = 1f;

        float _timeShieldHit = 0f;
        bool _isRechargingShield = false;

        // getters
        public bool isAlive => _isAlive;
        public float health => _health;
        public float shield => _shield;
        public float timeHit => _timeHit;
        public bool isRechardingShield => _isRechargingShield;
        public float hitRecoveryTime => _hitRecoveryTime;

        protected void TickHealth() {
            _timeHit = Mathf.Clamp(_timeHit - Time.deltaTime, 0f, _hitRecoveryTime);
            _timeShieldHit = Mathf.Clamp(_timeShieldHit - Time.deltaTime / _shieldWaitTime, 0f, _shieldWaitTime);

            // recharge shield
            if (_timeShieldHit <= 0f && _shield < _maxShield) {
                _isRechargingShield = true;
                _shield = Mathf.Min(_shield + _maxShield * (Time.deltaTime / _shieldRechargeTime), _maxShield);
                Debug.Log("recharge_shield >> " + _shield);
            } else {
                _isRechargingShield = false;
            }
        }

        protected void RegisterDamageCallbacks(System.Action onDeath, System.Action<float> onDamageTaken, System.Action<float> onHealthTaken, System.Action<float> onShieldDrained) {
            _onDeath = onDeath;
            _onDamageTaken = onDamageTaken;
            _onHealthTaken = onHealthTaken;
            _onShieldDrained = onShieldDrained;
        }
        protected void RegisterDamageCallbacks(System.Action onDeath, System.Action<float> onDamageTaken, System.Action<float> onHealthTaken) {
            _onDeath = onDeath;
            _onDamageTaken = onDamageTaken;
            _onHealthTaken = onHealthTaken;
        }
        protected void RegisterDamageCallbacks(System.Action onDeath, System.Action<float> onDamageTaken) {
            _onDeath = onDeath;
            _onDamageTaken = onDamageTaken;
        }

        protected void ResetHealth() {
            _isAlive = true;
            _health = _maxHealth;
            _shield = _maxShield;
            _timeHit = 0f;
        }

        public bool DrainShield(float amount) {
            if (!_isAlive) return false;

            if (_shield > 0f && amount > 0f) {
                _timeShieldHit = 1f;
                InvokeCallback(_onShieldDrained, amount, true);
            }
            _shield = Mathf.Max(_shield - amount, 0f);

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

            _health -= _healthDamageThisFrame;
            _shield = Mathf.Max(_shield - _shieldDamageThisFrame, 0f);
            _timeHit = _hitRecoveryTime;
            _timeShieldHit = 1f;

            if (amount > 0f) {
                InvokeCallback(_onDamageTaken, _healthDamageThisFrame);
            }

            if (_health <= 0f) {
                if (_isAlive) _Die();
            }

            return true;
        }

        private void _Die() {
            _isAlive = false;
            InvokeCallback(_onDeath);
            if (_destroyOnDeath) Destroy(gameObject);
        }

        // CALLBACK STUFF

        private void InvokeCallback(System.Action action) {
            if (action != null) {
                action.Invoke();
            } else {
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
    }
}


