using UnityEngine;

using Core;
using Damage;

namespace Weapons {

    public class RocketTargetingSystem : MonoBehaviour {
        [SerializeField][Range(0f, 5f)] float targetLockTime = 0.2f;

        float t = 0;
        bool preLocked = false;
        bool locked = false;
        Rocket rocket;
        Transform target;
        DamageReceiver actor;

        public void SetRocket(Rocket value) {
            rocket = value;
        }

        void Start() {
            t = 0f;
        }

        void Update() {
            if (locked) return;

            HandleLock();
            HandleLoseLock();
            t += Time.deltaTime;
        }

        void HandleLock() {
            if (locked) return;
            if (target == null) return;
            if (t < targetLockTime) return;

            locked = true;
            if (rocket != null) rocket.SetTarget(target);
        }

        void HandleLoseLock() {
            if (!locked) return;
            if (target == null
                || actor == null
                || !actor.isAlive
                || !Utils.IsObjectOnScreen(target.gameObject)
            ) {
                locked = false;
                target = null;
                actor = null;
                if (rocket != null) rocket.SetTarget(null);
            }
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (locked) return;

            if (target != null) {
                if (target == other.gameObject) return;
                if (!Utils.IsObjectOnScreen(other.gameObject)) return;
                // define priority of target tags
                if (target.tag == UTag.Boss && other.tag != UTag.Boss) return;
                if (target.tag == UTag.EnemyShip && other.tag != UTag.EnemyShip) return;
                if (target.tag == UTag.EnemyTurret && other.tag != UTag.EnemyTurret) return;
                if (target.tag == UTag.Ordnance && other.tag != UTag.Ordnance) return;
                // keep if target is more directly ahead of player
                float aheadnessThis = Vector2.Dot(transform.up, (transform.position - target.position).normalized);
                float aheadnessOther = Vector2.Dot(transform.up, (transform.position - other.transform.position).normalized);
                if (aheadnessThis > aheadnessOther) return;
                // keep target if closer
                if (Vector2.Distance(target.position, transform.position) <= Vector2.Distance(other.transform.position, transform.position)) return;
            }

            if (
                other.tag != UTag.Boss &&
                other.tag != UTag.EnemyShip &&
                other.tag != UTag.EnemyTurret &&
                other.tag != UTag.Ordnance
            ) {
                return;
            }

            actor = other.GetComponent<DamageReceiver>();
            if (actor == null) return;
            target = actor.root.transform;
        }
    }
}
