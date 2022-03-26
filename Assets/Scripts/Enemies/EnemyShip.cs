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

        // state
        bool everDamagedByPlayer = false;

        void Start() {
            AppIntegrity.AssertPresent<GameObject>(ship);
            AppIntegrity.AssertPresent<GameObject>(explosion);

            rb = GetComponent<Rigidbody2D>();
            if (rb != null) {
                originalDrag = rb.drag;
                rb.drag = 0f; // we will handle physics calcs manually MWA HA HA!!
            }

            ResetHealth();
            SetColliders();
            RegisterHealthCallbacks(OnDeath, OnHealthDamaged, Utils.__NOOP__);
            ship.SetActive(true);
            damageSound.Init(this);
            deathSound.Init(this);
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

        void OnDeath(bool isDamageByPlayer) {
            RemoveMarker();
            // OnEnemyDeath.Raise(); // old event
            eventChannel.OnEnemyDeath.Invoke(Utils.GetRootInstanceId(gameObject), GetDeathPoints(isDamageByPlayer));
            rb.drag = originalDrag; // to make it seem like it was there all along
            deathSound.Play();
            pickups.Spawn(transform.position, rb);
            StartCoroutine(DeathAnimation());
        }

        public void OnDeathByGuardians() {
            TakeDamage(1000f, DamageType.Instakill, false);
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

        IEnumerator DeathAnimation() {
            Instantiate(explosion, transform);
            ship.SetActive(false);
            yield return new WaitForSeconds(3f);
            Destroy(gameObject);

            yield return null;
        }
    }
}
