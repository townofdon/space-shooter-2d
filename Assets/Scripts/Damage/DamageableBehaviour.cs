using System.Collections;
using UnityEngine;

using Core;
using Game;

namespace Damage {

    public enum DamageableType {
        Default,
        Ship,
        Shield,
        Station,
        Rock,
        ExplodeOnCollision,
    }

    public class DamageableBehaviour : MonoBehaviour
    {
        [Header("General Settings")][Space]
        [SerializeField] DamageableType _damageableType = DamageableType.Default;
        [SerializeField] bool debug = false;
        [SerializeField] bool _invulnerable = false;
        [SerializeField] float _timeInvincibleAfterSpawn = 0f;

        [Space]
        [Tooltip("Probability that actor cheats death")]
        [SerializeField][Range(0f, 1f)] float savingThrow = 0f;

        [Header("Health Settings")][Space]
        [SerializeField] float _maxHealth = 50f;
        [SerializeField] float _hitRecoveryTime = 0.2f;
        [SerializeField] float _nukeRecoveryTime = 0.4f;
        [SerializeField] bool _destroyOnDeath = false;

        [Header("Shield Settings")][Space]
        [SerializeField] float _maxShield = 0f;
        [SerializeField] float _shieldWaitTime = 3f;
        [SerializeField] float _shieldRechargeTime = 1f;

        [Header("Difficulty Settings")]
        [Space]
        [SerializeField][Range(0f, 5f)] float healthModEasy = 1f;
        [SerializeField][Range(0f, 5f)] float healthModMedium = 1f;
        [SerializeField][Range(0f, 5f)] float healthModHard = 1f;
        [SerializeField][Range(0f, 5f)] float healthModInsane = 1f;
        [Space]
        [SerializeField][Range(0f, 5f)] float shieldModEasy = 1f;
        [SerializeField][Range(0f, 5f)] float shieldModMedium = 1f;
        [SerializeField][Range(0f, 5f)] float shieldModHard = 1f;
        [SerializeField][Range(0f, 5f)] float shieldModInsane = 1f;

        [Header("Damage FX")][Space]
        [SerializeField] Material defaultMaterial;
        [SerializeField] Material damageFlashMaterial;
        [SerializeField] float damageFlashDuration = 0.1f;
        [SerializeField] SpriteRenderer damageSpriteTarget;
        [SerializeField] SpriteRenderer[] secondaryDamageSpriteTargets;

        [Header("Cleanup on Aisle 5")]
        [Space]
        [SerializeField] GameObject[] perishableObjects;

        [Header("Events / Callbacks")][Space]
        // Action callbacks can be set via the Editor OR via RegisterCallbacks()
        [SerializeField] System.Action<DamageType, bool> _onDeath;
        [SerializeField] System.Action<float> _onHealthTaken;
        [SerializeField] System.Action<float, DamageType, bool> _onHealthDamage;
        [SerializeField] System.Action _onShieldDepleted;
        [SerializeField] System.Action<float> _onShieldDamage;
        [SerializeField] System.Action<float> _onShieldDrain;
        [SerializeField] System.Action _onShieldRechargeStart;
        [SerializeField] System.Action _onShieldRechargeComplete;

        public delegate void DamageAction();
        public event DamageAction OnDeathEvent;
        public event DamageAction OnShieldHitEvent;
        public event DamageAction OnShieldDepletedEvent;

        // cached
        System.Guid _uuid = System.Guid.NewGuid();
        protected Collider2D[] colliders;
        DamageClass damageClass;

        // state
        bool _isAlive = true;
        float _health = 50f;
        float _shield = 0f;
        float _timeHit = 0f;
        Timer _nukeHit = new Timer(TimerDirection.Decrement, TimerStep.DeltaTime, 0.4f);
        float _healthDamageThisFrame = 0f;
        float _shieldDamageThisFrame = 0f;
        Timer _spawnInvulnerable = new Timer();

        float _prevShield = 0f;
        float _timeShieldHit = 0f;
        bool _isRechargingShield = false;

        // getters
        public System.Guid uuid => _uuid;
        public bool isAlive => _isAlive;
        public float health => _health;
        public float healthPct => _health / (_maxHealth * GetHealthMod());
        public float shield => _shield;
        public float shieldPct => _shield / (_maxShield * GetShieldMod());
        public float timeHit => _timeHit;
        public bool isRechargingShield => _isRechargingShield;
        public float hitRecoveryTime => _hitRecoveryTime;
        public bool hasShieldCapability => _maxShield * GetShieldMod() > 0f;
        public DamageableType damageableType => (hasShieldCapability && _shield > 0f) ? DamageableType.Shield : _damageableType;

        public void SetInvulnerable(bool value) {
            _invulnerable = value;
        }

        public void OnDeathByGuardians(bool isQuiet = false) {
            StartCoroutine(IDeathByGuardians(isQuiet));
        }

        void OnDestroy() {
            foreach (var perishable in perishableObjects) {
                GameObject.Destroy(perishable);
            }
        }

        protected void TickHealth() {
            if (!_isAlive) return;
            _timeHit = Mathf.Clamp(_timeHit - Time.deltaTime, 0f, _hitRecoveryTime);
            _timeShieldHit = Mathf.Clamp(_timeShieldHit - Time.deltaTime / _shieldWaitTime, 0f, _shieldWaitTime);
            _nukeHit.Tick();
            _spawnInvulnerable.Tick();

            if (!hasShieldCapability) return;

            // recharge shield
            if (_timeShieldHit <= 0f && _shield < _maxShield * GetShieldMod()) {
                // NOTE - may also find it useful to add an `_onShieldRechargeStep` callback method at some point if needed for UI updates, etc.
                if (!_isRechargingShield) InvokeCallback(_onShieldRechargeStart);
                _isRechargingShield = true;
                _shield = Mathf.Min(_shield + _maxShield * GetShieldMod() * (Time.deltaTime / _shieldRechargeTime), _maxShield * GetShieldMod());
            } else {
                if (_isRechargingShield && _shield == _maxShield * GetShieldMod()) InvokeCallback(_onShieldRechargeComplete);
                _isRechargingShield = false;
            }
        }

        protected void RegisterHealthCallbacks(
            System.Action<DamageType, bool> onDeath,
            System.Action<float, DamageType, bool> onHealthDamage,
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
            _health = _maxHealth * GetHealthMod();
            _shield = _maxShield * GetShieldMod();
            _timeHit = 0f;
            _spawnInvulnerable.SetDuration(_timeInvincibleAfterSpawn);
            _spawnInvulnerable.Start();
        }

        protected void SetColliders() {
            colliders = GetColliders();
            _EnableColliders();
        }

        public Collider2D[] GetColliders() {
            if (colliders != null && colliders.Length > 0) return colliders;
            return transform.GetComponentsInChildren<Collider2D>(true);
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
                InvokeCallback(_onShieldDrain, amount);
            }
            _shield = Mathf.Max(_shield - amount, 0f);
            return true;
        }

        public bool TakeHealth(float amount) {
            if (!_isAlive) return false;

            _health = Mathf.Min(_health + amount, _maxHealth * GetHealthMod());
            InvokeCallback(_onHealthTaken, amount);

            return true;
        }

        public bool TakeDamage(float amount, DamageType damageType = DamageType.Default, bool isDamageByPlayer = false) {
            if (!_isAlive) return false;

            if (damageType == DamageType.Instakill || damageType == DamageType.InstakillQuiet) {
                _health = Mathf.Min(0f, _health - amount);
                _shield = 0f;
                _Die(damageType, isDamageByPlayer);
                return true;
            }

            if (_timeHit > Mathf.Epsilon) return false;
            if (_invulnerable) return false;
            if (_spawnInvulnerable.active) return false;
            if (!Utils.IsObjectOnScreen(gameObject, Utils.GetCamera(), 1f)) return false;

            if (_damageableType == DamageableType.ExplodeOnCollision && damageType == DamageType.Collision) {
                _health = Mathf.Min(0f, _health - amount);
                _shield = 0f;
                _Die(damageType, isDamageByPlayer);
                return true;
            }

            if (damageType == DamageType.Nuke && amount > Mathf.Epsilon) {
                if (_nukeHit.active) return false;
                _nukeHit.SetDuration(_nukeRecoveryTime);
                _nukeHit.Start();
            }

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
            if (gameObject.tag == UTag.EnemyShip || gameObject.tag == UTag.EnemyTurret) {
                _healthDamageThisFrame *= damageClass.enemyEffectiveness;
                _shieldDamageThisFrame *= damageClass.enemyEffectiveness;
            }
            if (gameObject.tag == UTag.Asteroid) {
                _healthDamageThisFrame *= damageClass.environmentEffectiveness;
                _shieldDamageThisFrame *= damageClass.environmentEffectiveness;
            }

            if (_healthDamageThisFrame > 0f) {
                InvokeCallback(_onHealthDamage, _healthDamageThisFrame, damageType, isDamageByPlayer);
                DamageFlash();
            }
            if (_shieldDamageThisFrame > 0f && _shield > 0f && hasShieldCapability) {
                InvokeCallback(_onShieldDamage, _shieldDamageThisFrame);
                if (OnShieldHitEvent != null) OnShieldHitEvent();
            }

            _health -= _healthDamageThisFrame;
            _shield = Mathf.Max(_shield - _shieldDamageThisFrame, 0f);
            if (amount > Mathf.Epsilon) {
                _timeHit = _hitRecoveryTime;
                _timeShieldHit = 1f;
            }

            if (_shield == 0f && _prevShield > _shield && hasShieldCapability) {
                InvokeCallback(_onShieldDepleted);
                if (OnShieldDepletedEvent != null) OnShieldDepletedEvent();
            }

            _prevShield = _shield;

            if (_health <= 0f) {
                if (savingThrow > Mathf.Epsilon && UnityEngine.Random.Range(0f, 1f) <= savingThrow) {
                    _health = _maxHealth * 0.05f;
                    return true;
                }

                // if about to die from collision - give one last save
                if (savingThrow > Mathf.Epsilon && damageType == DamageType.Collision && UnityEngine.Random.Range(0f, 1f) <= savingThrow) {
                    _health = _maxHealth * 0.05f;
                    return true;
                }

                if (_isAlive) _Die(damageType, isDamageByPlayer);
            }

            return true;
        }

        private void DamageFlash() {
            if (damageFlashDuration <= 0 || defaultMaterial == null || damageFlashMaterial == null || (damageSpriteTarget == null && secondaryDamageSpriteTargets.Length == 0)) return;
            StartCoroutine(IDamageFlash());
        }

        private IEnumerator IDamageFlash() {
            if (damageSpriteTarget != null) damageSpriteTarget.material = damageFlashMaterial;
            foreach (var sprite in secondaryDamageSpriteTargets) sprite.material = damageFlashMaterial;
            yield return new WaitForSeconds(damageFlashDuration);
            if (damageSpriteTarget != null) damageSpriteTarget.material = defaultMaterial;
            foreach (var sprite in secondaryDamageSpriteTargets) sprite.material = defaultMaterial;
        }

        private void _Die(DamageType damageType, bool isDamageByPlayer) {
            _isAlive = false;
            _DisableColliders();
            InvokeCallback(_onDeath, damageType, isDamageByPlayer);
            if (_destroyOnDeath) Destroy(gameObject);
            if (OnDeathEvent != null) OnDeathEvent();
        }

        private void _EnableColliders() {
            foreach (var collider in colliders) {
                collider.enabled = true;
            }
        }

        private void _DisableColliders() {
            if (colliders == null) return;
            foreach (var collider in colliders) {
                if (collider != null) collider.enabled = false;
            }
        }

        private float GetHealthMod() {
            switch (GameManager.current.difficulty) {
                case (GameDifficulty.Easy):
                    return healthModEasy;
                case (GameDifficulty.Medium):
                    return healthModMedium;
                case (GameDifficulty.Hard):
                    return healthModHard;
                case (GameDifficulty.Insane):
                    return healthModInsane;
                default:
                    return healthModMedium;
            }
        }

        private float GetShieldMod() {
            switch (GameManager.current.difficulty) {
                case (GameDifficulty.Easy):
                    return shieldModEasy;
                case (GameDifficulty.Medium):
                    return shieldModMedium;
                case (GameDifficulty.Hard):
                    return shieldModHard;
                case (GameDifficulty.Insane):
                    return shieldModInsane;
                default:
                    return shieldModMedium;
            }
        }

        private IEnumerator IDeathByGuardians(bool isQuiet = false) {
            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 0.2f));
            TakeDamage(1000f, isQuiet ? DamageType.InstakillQuiet : DamageType.Instakill, false);
        }

        // CALLBACK STUFF

        private void InvokeCallback(System.Action action) {
            if (action != null) {
                action.Invoke();
            }
        }
        private void InvokeCallback(System.Action<bool> action, bool boolValue) {
            if (action != null) {
                action.Invoke(boolValue);
            }
        }
        private void InvokeCallback(System.Action<DamageType, bool> action, DamageType damageType, bool boolValue) {
            if (action != null) {
                action.Invoke(damageType, boolValue);
            }
        }
        private void InvokeCallback(System.Action<float> action, float amount) {
            if (action != null) {
                action.Invoke(amount);
            }
        }
        private void InvokeCallback(System.Action<float, DamageType, bool> action, float amount, DamageType damageType, bool isDamageByPlayer) {
            if (action != null) {
                action.Invoke(amount, damageType, isDamageByPlayer);
            }
        }

        private void OnGUI() {
            if (!debug) return;
            GUILayout.TextField("health=" + health.ToString());
            GUILayout.TextField("shield=" + shield.ToString());
        }
    }
}


