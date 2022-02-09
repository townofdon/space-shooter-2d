using System.Collections;

using UnityEngine;

using Core;
using Damage;

namespace Enemies {

    public class EnemyShip : DamageableBehaviour
    {
        [Header("Components")][Space]
        [SerializeField] GameObject ship;
        [SerializeField] GameObject explosion;

        [Header("Movement")][Space]
        [SerializeField] float _turnSpeed = 2f;

        // getters
        public float turnSpeed => _turnSpeed;

        void Start() {
            AppIntegrity.AssertPresent<GameObject>(ship);
            AppIntegrity.AssertPresent<GameObject>(explosion);

            ResetHealth();
            SetColliders();
            RegisterDamageCallbacks(OnDeath, OnDamageTaken);
            ship.SetActive(true);
        }

        void Update() {
            TickHealth();
        }

        void OnDamageTaken(float amount) {
            // Debug.Log("enemy_damage=" + amount + " health=" + health);
            // TODO: FLASH ENEMY SPRITE
            // TODO: PLAY DAMAGE SOUND
        }

        void OnDeath() {
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

