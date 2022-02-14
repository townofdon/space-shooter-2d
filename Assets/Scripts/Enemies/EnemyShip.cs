using System.Collections;

using UnityEngine;

using Core;
using Damage;
using Audio;

namespace Enemies {

    public class EnemyShip : DamageableBehaviour
    {
        [Header("Components")][Space]
        [SerializeField] GameObject ship;
        [SerializeField] GameObject explosion;

        [Header("Movement")][Space]
        [SerializeField] float _turnSpeed = 2f;

        [Header("Audio")][Space]
        [SerializeField] Sound damageSound;
        [SerializeField] Sound deathSound;

        // getters
        public float turnSpeed => _turnSpeed;

        void Start() {
            AppIntegrity.AssertPresent<GameObject>(ship);
            AppIntegrity.AssertPresent<GameObject>(explosion);

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
            deathSound.Play();
            StartCoroutine(DeathAnimation());
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

