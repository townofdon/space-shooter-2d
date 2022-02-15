using System.Collections;

using UnityEngine;

using Core;
using Damage;
using Audio;
using Game;
using UI;

namespace Enemies {

    public class EnemyShip : DamageableBehaviour
    {
        [Header("Components")][Space]
        [SerializeField] GameObject ship;
        [SerializeField] GameObject explosion;

        [Header("Movement")][Space]
        [SerializeField] float _turnSpeed = 4f;
        [SerializeField] float _moveSpeed = 5f;
        [SerializeField] float _accel = 10f;

        [Header("Audio")][Space]
        [SerializeField] Sound damageSound;
        [SerializeField] Sound deathSound;

        [Header("Events")][Space]
        [SerializeField] GameEvent OnEnemyDeath;

        // getters
        public float turnSpeed => _turnSpeed;
        public float moveSpeed => _moveSpeed;
        public float accel => _accel;

        // components
        Rigidbody2D rb;

        // cached
        float originalDrag;

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

        void OnHealthDamaged(float amount, DamageType damageType) {
            // Debug.Log("enemy_damage=" + amount + " health=" + health);
            // TODO: FLASH ENEMY SPRITE
            // TODO: PLAY DAMAGE SOUND
            damageSound.Play();
        }

        void OnDeath() {
            RemoveMarker();
            OnEnemyDeath.Raise();
            rb.drag = originalDrag; // to make it seem like it was there all along
            deathSound.Play();
            StartCoroutine(DeathAnimation());
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
