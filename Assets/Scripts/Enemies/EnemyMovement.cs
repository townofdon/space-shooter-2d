using UnityEngine;

using Core;
using Player;
using Audio;

namespace Enemies
{

    public class EnemyMovement : MonoBehaviour
    {
        [Header("Movement")][Space]
        [SerializeField] bool debug = false;
        [SerializeField] float turnSpeed = 4f;
        [SerializeField] float moveSpeed = 5f;
        [SerializeField] float accel = 10f;

        [Header("Behaviour")][Space]
        // TODO: MOVE TO EnemyBehaviour SCRIPT
        [SerializeField] bool kamikaze = false;
        [Tooltip("Juke left or right when targeted by player")]
        [SerializeField][Range(0f, 1f)] float jukes = 0f;
        [SerializeField][Range(0f, 2f)] float jukeTargetedTime = 0f;
        [SerializeField][Range(0f, 2f)] float jukeTargetedVariance = 0f;
        [SerializeField][Range(0f, 5f)] float jukeSpeed = 0f;
        [SerializeField][Range(0f, 5f)] float jukeCooldown = 0f;
        [SerializeField] LayerMask jukeLayerAvoid;
        [Tooltip("Avoid hazards, explosives")]
        [SerializeField][Range(0f, 1f)] float avoids = 0f;
        [SerializeField][Range(0f, 5f)] float avoidSpeed = 0f;
        [SerializeField][Range(0f, 5f)] float avoidCooldown = 0f;

        [Header("Drift")][Space]
        [SerializeField][Range(0f, 1f)] float driftXMag = 0f;
        [SerializeField][Range(0.01f, 1f)] float driftXFreq = 0f;
        [SerializeField][Range(0f, 2f)] float driftXOffset = 0f;
        [SerializeField][Range(0f, 1f)] float driftYMag = 0f;
        [SerializeField][Range(0.01f, 1f)] float driftYFreq = 0f;
        [SerializeField][Range(0f, 2f)] float driftYOffset = 0f;

        [Header("Audio")][Space]
        [SerializeField] LoopableSound engineSound;
        [SerializeField] LoopableSound agroSound;

        // components
        PlayerGeneral player;
        EnemyShip enemy;
        Rigidbody2D rb;

        // state
        bool hasTarget = false;
        Vector3 heading;
        Vector3 optimalHeading;
        Vector3 target;
        Vector3 targetPrev;

        // state - drift
        Vector3 drift;
        float driftTime;

        // state - juke
        bool isJuking = false;
        Timer juking = new Timer();
        float timeTargetedByPlayer = 0f;

        // state - avoid
        bool isAvoiding = false;
        Timer avoiding = new Timer();
        GameObject avoidObject;

        // state - raycasts
        RaycastHit2D hit;

        public void SetTarget(Vector3 value) {
            hasTarget = true;
            // targetPrev is set here so that when the player is destroyed the enemy craft can go back to its original path
            targetPrev = value;
            target = value;
        }

        public void SetKamikaze(bool value) {
            kamikaze = value;
        }

        public void SetImmediateVelocity(float velocityMultiplier) {
            if (rb == null) return;
            rb.velocity = heading * moveSpeed * velocityMultiplier;
        }

        // TODO: ADD A PLAYER_GENERAL LOOP (USE COROUTINE WHILE(TRUE) YIELD WAITFORTIME(0.2S) RAYCAST TO FIRST ENEMY SHIP -> NotifyTargeted())
        public void NotifyTargeted(float duration) {
            if (juking.active) return;
            timeTargetedByPlayer += duration;
            if (timeTargetedByPlayer > Mathf.Max(0.2f, jukeTargetedTime + UnityEngine.Random.Range(-jukeTargetedVariance, jukeTargetedVariance))) {
                juking.Start();
                isJuking = true;
                timeTargetedByPlayer = 0f;
            }
        }

        void Start() {
            enemy = Utils.GetRequiredComponent<EnemyShip>(gameObject);
            rb = Utils.GetRequiredComponent<Rigidbody2D>(gameObject);
            player = FindObjectOfType<PlayerGeneral>();
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            engineSound.Init(this);
            agroSound.Init(this);
            juking.SetDuration(jukeCooldown);
        }

        void Update() {
            HandleTargetBehaviour();
            juking.Tick();
        }


        void FixedUpdate() {
            Handlejuke();
            MoveTowardsHeading();
            ApplyDrift();
        }

        void HandleTargetBehaviour() {
            if (kamikaze && player != null && player.isAlive) {
                hasTarget = true;
                target = player.transform.position;
                agroSound.Play();
                engineSound.Stop();
            } else {
                target = targetPrev;
                engineSound.Play();
                agroSound.Stop();
            }
        }

        void Handlejuke() {
            if (!isJuking) return;
            // move left or right to avoid incoming
            if (rb.velocity.x > 0f) {
                if (!CheckRaycastHazard(Vector2.left, jukeSpeed)) rb.AddForce(Vector2.left * jukeSpeed, ForceMode2D.Impulse);
            } else {
                if (!CheckRaycastHazard(Vector2.right, jukeSpeed)) rb.AddForce(Vector2.right * jukeSpeed, ForceMode2D.Impulse);
            }
            isJuking = false;
        }

        void MoveTowardsHeading() {
            if (!enemy.isAlive) return;
            if (!hasTarget) return;
            // rotate the enemy so that it turns smoothly
            heading = Vector3.RotateTowards(
                heading,
                (target - transform.position).normalized,
                2f * turnSpeed * 2f * Mathf.PI * Time.fixedDeltaTime, turnSpeed * Time.fixedDeltaTime
            ).normalized;

            // vector maths
            optimalHeading = (heading * moveSpeed - (Vector3)rb.velocity).normalized;
            rb.AddForce(optimalHeading * accel);
            if (Vector2.Angle(heading, rb.velocity.normalized) < 30f && rb.velocity.magnitude > moveSpeed) {
                rb.velocity *= ( 1f - Time.fixedDeltaTime * 1.5f);
            }
        }

        void ApplyDrift() {
            if (!enemy.isAlive || kamikaze) return;

            drift.x = Mathf.Cos(driftTime / driftXFreq + driftXOffset * Mathf.PI) * driftXMag;
            drift.y = Mathf.Cos(driftTime / driftYFreq + driftYOffset * Mathf.PI) * driftYMag;
            driftTime += Time.fixedDeltaTime;
            transform.position += drift;
            // rb.velocity += (Vector2)drift;
        }

        bool CheckRaycastHazard(Vector2 direction, float distance) {
            hit = Physics2D.Raycast(transform.position, direction, distance, jukeLayerAvoid);
            return hit.collider != null;
        }

        void OnDrawGizmos() {
            if (!debug) return;
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, transform.position + heading);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + optimalHeading);
        }
    }
}
