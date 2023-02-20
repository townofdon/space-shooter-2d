using UnityEngine;

using Enemies;
using Damage;
using Physics;

namespace Game {

    public class AccelOnWarp : MonoBehaviour {

        Rigidbody2D rb;
        DamageableBehaviour actor;
        EnemyShooter enemyShooter;
        EnemyMovement enemyMovement;
        ScrollDownScreen scrollDownScreen;
        OffscreenDestroyer offscreenDestroyer;

        bool isWarping = false;
        float speed = 0f;
        Vector3 heading = Vector3.down;

        float maxSpeed = 20f;
        float accelTime = 1f;

        public void BeginWarp(float _maxSpeed = 20f, float _accelTime = 1f) {
            isWarping = true;
            maxSpeed = _maxSpeed;
            accelTime = _accelTime;
            if (rb != null) rb.isKinematic = true;
            if (enemyMovement != null) enemyMovement.enabled = false;
            if (enemyShooter != null) enemyShooter.enabled = false;
            if (scrollDownScreen != null) scrollDownScreen.enabled = false;
            if (offscreenDestroyer != null) offscreenDestroyer.enabled = false;
            if (actor != null) actor.PrepareForWarp();
        }

        void Start() {
            speed = 0f;
            isWarping = false;
            rb = GetComponent<Rigidbody2D>();
            actor = GetComponent<DamageableBehaviour>();
            enemyShooter = GetComponent<EnemyShooter>();
            enemyMovement = GetComponent<EnemyMovement>();
            scrollDownScreen = GetComponent<ScrollDownScreen>();
            offscreenDestroyer = GetComponent<OffscreenDestroyer>();
        }

        void Update() {
            if (!isWarping) return;

            transform.position = transform.position + heading * speed;

            if (accelTime <= 0) {
                speed = maxSpeed;
            } else {
                speed = Mathf.Min(speed + (GetZMod() * Time.deltaTime) / accelTime, maxSpeed * GetZMod());
            }
        }

        float GetZMod() {
            // if z === 10 => return 0
            // if z === 0 => return 1
            // if z === -10 => return 2
            return Mathf.Clamp((10f - transform.position.z) * 0.1f, 0f, 2f);
        }
    }
}