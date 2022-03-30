using UnityEngine;

using Core;
using Player;

namespace Physics {

    public class AttractTowardsPlayer : MonoBehaviour {
        [SerializeField][Range(0f, 20f)] float attractRadius = 1f;
        [SerializeField][Range(0.1f, 50f)] float accelMod = 1f;
        [SerializeField][Range(1f, 100f)] float topSpeed = 20f;

        // components
        GameObject player;
        PlayerGeneral playerGeneral;
        Rigidbody2D rb;

        // cached
        float initialDrag = 1f;
        float distanceToPlayer = 100f;

        // state
        Timer findPlayerInterval = new Timer(TimerDirection.Decrement, TimerStep.FixedDeltaTime);
        Vector2 attraction;

        void Start() {
            rb = GetComponent<Rigidbody2D>();
            initialDrag = rb.drag;
            player = GameObject.FindGameObjectWithTag(UTag.Player);
            if (player != null) playerGeneral = player.GetComponentInParent<PlayerGeneral>();
            findPlayerInterval.SetDuration(0.1f);
        }

        void FixedUpdate() {
            if (player == null || playerGeneral == null) {
                if (findPlayerInterval.active) return;
                findPlayerInterval.Start();
                player = GameObject.FindGameObjectWithTag(UTag.Player);
                if (player != null) playerGeneral = player.GetComponentInParent<PlayerGeneral>();
            }
            Attract();
            findPlayerInterval.Tick();
        }

        void Attract() {
            if (player != null && playerGeneral != null && playerGeneral.isAlive) {
                distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
            } else {
                distanceToPlayer = float.MaxValue;
            }
            attraction = getAttractionVector();
            if (attraction == Vector2.zero) {
                rb.drag = initialDrag;
            } else {
                rb.drag = 0f;
                rb.AddForce(attraction);
                rb.velocity = Vector2.ClampMagnitude(rb.velocity, topSpeed);
                // aim directly at the player
                rb.velocity = Vector2.ClampMagnitude((player.transform.position - transform.position).normalized * topSpeed, rb.velocity.magnitude);
            }
        }

        Vector2 getAttractionVector() {
            if (player == null) return Vector2.zero;
            if (distanceToPlayer > attractRadius) return Vector2.zero;
            float force = (9.81f * accelMod) / Mathf.Pow(distanceToPlayer, 2f);
            return (player.transform.position - transform.position).normalized * force;
        }
    }
}

