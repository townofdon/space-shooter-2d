using System.Collections;

using UnityEngine;

using Core;
using Damage;
using Audio;
using UI;
using Pickups;
using Event;

namespace Enemies {

    public class EnemyShip : DamageableBehaviour
    {
        [Header("Enemy Type")]
        [Space]
        [SerializeField] bool isBoss = false;

        [Header("Components")][Space]
        [SerializeField] GameObject ship;
        [SerializeField] GameObject explosion;

        [Header("Movement")][Space]
        [Header("Audio")][Space]
        [SerializeField] Sound damageSound;
        [SerializeField] Sound deathSound;

        [Header("Pickups")]
        [Space]
        [SerializeField] PickupsSpawnConfig pickups;

        [Header("Points")]
        [Space]
        [SerializeField] int pointsWhenKilledByPlayer = 50;
        [SerializeField] int pointsWhenWoundedByPlayer = 10;

        [Header("Events")][Space]
        // [SerializeField] GameEvent OnEnemyDeath;
        [SerializeField] EventChannelSO eventChannel;

        // components
        Rigidbody2D rb;

        // cached
        float originalDrag;

        // props
        int instanceId;

        // state
        bool everDamagedByPlayer = false;

        void OnEnable() {
            if (isBoss) eventChannel.OnBossSpawn.Invoke(instanceId);
        }

        void Awake() {
            instanceId = Utils.GetRootInstanceId(gameObject);
            SetColliders();
        }

        void Start() {
            AppIntegrity.AssertPresent<GameObject>(ship);
            AppIntegrity.AssertPresent<GameObject>(explosion);
            AppIntegrity.AssertPresent<EventChannelSO>(eventChannel);

            rb = GetComponent<Rigidbody2D>();
            if (rb != null) {
                originalDrag = rb.drag;
                rb.drag = 0f; // we will handle physics calcs manually MWA HA HA!!
            }

            ResetHealth();
            RegisterHealthCallbacks(OnDeath, OnHealthDamaged, Utils.__NOOP__);
            ship.SetActive(true);
            damageSound.Init(this);
            deathSound.Init(this);

            if (tag == UTag.Boss) eventChannel.OnBossSpawn.Invoke(instanceId);
        }

        void Update() {
            TickHealth();
        }

        void OnHealthDamaged(float amount, DamageType damageType, bool isDamageByPlayer) {
            if (isDamageByPlayer) everDamagedByPlayer = true;
            // Debug.Log("enemy_damage=" + amount + " health=" + health);
            // TODO: FLASH ENEMY SPRITE
            // TODO: PLAY DAMAGE SOUND
            damageSound.Play();
        }

        void OnDeath(DamageType damageType, bool isDamageByPlayer) {
            RemoveMarker();
            eventChannel.OnEnemyDeath.Invoke(instanceId, GetDeathPoints(isDamageByPlayer));
            if (rb != null) rb.drag = originalDrag; // to make it seem like it was there all along
            if (damageType != DamageType.InstakillQuiet) {
                deathSound.Play();
                pickups.Spawn(transform.position, rb);
            }
            StartCoroutine(DeathAnimation());
        }

        public void OnDeathByGuardians(bool isQuiet = false) {
            StartCoroutine(IDeathByGuardians());
        }

        int GetDeathPoints(bool isDamageByPlayer) {
            if (isDamageByPlayer) return pointsWhenKilledByPlayer;
            if (everDamagedByPlayer) return pointsWhenWoundedByPlayer;
            return 0;
        }

        void RemoveMarker() {
            OffscreenMarker marker = GetComponentInChildren<OffscreenMarker>();
            if (marker != null) marker.Disable();
        }

        IEnumerator IDeathByGuardians(bool isQuiet = false) {
            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 0.2f));
            TakeDamage(1000f, isQuiet ? DamageType.InstakillQuiet : DamageType.Instakill, false);
        }

        IEnumerator DeathAnimation() {
            Instantiate(explosion, transform);
            if (ship != null) ship.SetActive(false);
            yield return new WaitForSeconds(3f);
            while (deathSound.isPlaying) yield return null;
            Destroy(gameObject);

            yield return null;
        }
    }
}
