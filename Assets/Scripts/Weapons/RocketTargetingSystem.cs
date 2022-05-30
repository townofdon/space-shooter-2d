using UnityEngine;

using Core;
using Audio;

namespace Weapons {

    public class RocketTargetingSystem : MonoBehaviour {
        [SerializeField][Range(0f, 5f)] float targetLockTime = 0.2f;

        float t = 0;
        bool locked = false;
        Transform target;
        Rocket rocket;

        public void SetRocket(Rocket value) {
            rocket = value;
        }

        void Start() {
            t = 0f;
        }

        void Update() {
            if (locked) return;

            HandleLock();
            t += Time.deltaTime;
        }

        void HandleLock() {
            if (locked) return;
            if (target == null) return;
            if (t < targetLockTime) return;

            locked = true;
            if (rocket != null) rocket.SetTarget(target);
            gameObject.SetActive(false);
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (locked) return;

            if (target != null) {
                // define priority of target tags
                if (target.tag == UTag.Boss && other.tag != UTag.Boss) return;
                if (target.tag == UTag.EnemyShip && other.tag != UTag.EnemyShip) return;
                if (target.tag == UTag.EnemyTurret && other.tag != UTag.EnemyTurret) return;
                if (target.tag == UTag.Asteroid && other.tag != UTag.Asteroid) return;
                if (target.tag == UTag.Ordnance && other.tag != UTag.Ordnance) return;
                // keep target if closer
                if (Vector2.Distance(target.position, transform.position) <= Vector2.Distance(other.transform.position, transform.position)) return;
            }

            if (
                other.tag != UTag.Boss &&
                other.tag != UTag.EnemyShip &&
                other.tag != UTag.EnemyTurret &&
                other.tag != UTag.Asteroid &&
                other.tag != UTag.Ordnance
            ) {
                return;
            }

            target = other.transform;
        }
    }
}
