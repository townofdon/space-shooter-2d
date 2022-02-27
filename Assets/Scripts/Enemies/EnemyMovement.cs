using UnityEngine;

using Core;
using Player;
using Audio;

namespace Enemies
{
    enum TargetMode {
        Null,
        Position,
        Heading,
    }

    enum MovementMode {
        Waypoints,
        ReflectOffScreenEdges,
    }

    // TODO: REMOVE
    // [System.Serializable]
    // public struct WobbleProps {
    //     [SerializeField] float _accel = 0f;
    //     [SerializeField] float _maxSpeed = 0f;
    //     [SerializeField] float _maxDistance = 0f;

    //     public WobbleProps(float accel, float maxSpeed, float maxDistance) {
    //         _accel = accel;
    //         _maxSpeed = maxSpeed;
    //         _maxDistance = maxDistance;
    //     }

    //     public float accel => _accel;
    //     public float maxSpeed => _maxSpeed;
    //     public float maxDistance => _maxDistance;
    // }

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

        [Header("Wobble")][Space]
        [SerializeField] Rigidbody2D wobbler;
        [SerializeField] Vector2 wobbleForce;
        [SerializeField] Vector2 wobbleMaxSpeed;
        [SerializeField] Vector2 wobbleMaxDistance;

        [Header("Audio")][Space]
        [SerializeField] LoopableSound engineSound;
        [SerializeField] LoopableSound agroSound;

        // components
        PlayerGeneral player;
        EnemyShip enemy;
        Rigidbody2D rb;
        Rigidbody2D rbWobble;

        // state
        TargetMode targetMode = TargetMode.Null;
        Vector3 heading;
        Vector3 headingAdjusted;
        Vector3 targetHeading;
        Vector3 targetPosition;
        Vector3 targetPositionPrev;

        // state - wobble
        Vector2 wobbleDir = Vector2.one;
        Vector2 wobbleDeltaP = Vector2.zero; // diff in position
        Vector2 wobbleDeltaV = Vector2.zero; // diff in velocity

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
            targetMode = TargetMode.Position;
            // targetPrev is set here so that when the player is destroyed the enemy craft can go back to its original path
            targetPositionPrev = value;
            targetPosition = value;
        }

        public void SetHeading(Vector3 value) {
            targetMode = TargetMode.Heading;
            targetHeading = value;
        }

        public void SetKamikaze(bool value) {
            kamikaze = value;
        }

        public void SetImmediateVelocity(float velocityMultiplier) {
            if (rb == null) return;
            rb.velocity = (heading * moveSpeed * velocityMultiplier);
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
            // init wobble
            if (wobbler != null && rb != null) {
                Rigidbody2D temp = Instantiate(wobbler, Vector3.zero, Quaternion.identity);
                Destroy(wobbler.gameObject);
                wobbler = temp;
                wobbler.gravityScale = rb.gravityScale;
                wobbler.mass = rb.mass;
                wobbler.drag = rb.drag;
                wobbler.angularDrag = rb.drag;
            }
        }

        void Update() {
            HandleTargetBehaviour();
            juking.Tick();
        }


        void FixedUpdate() {
            Handlejuke();
            Applywobble();
            MoveTowardsHeading();
            ApplyOffscreenBrakes();
        }

        void HandleTargetBehaviour() {
            if (kamikaze && player != null && player.isAlive) {
                targetMode = TargetMode.Position;
                targetPosition = player.transform.position;
                agroSound.Play();
                engineSound.Stop();
            } else {
                targetPosition = targetPositionPrev;
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
            if (targetMode == TargetMode.Null) return;
            // rotate the enemy so that it turns smoothly
            heading = Vector3.RotateTowards(
                heading,
                GetRotateTowardsTarget(),
                2f * turnSpeed * 2f * Mathf.PI * Time.fixedDeltaTime, turnSpeed * Time.fixedDeltaTime
            ).normalized;

            // vector maths - compensate for rb's current velocity vs. heading
            headingAdjusted = (heading * moveSpeed - (Vector3)GetCurrentVelocity()).normalized;
            rb.AddForce(headingAdjusted * accel);
            if (Vector2.Angle(heading, rb.velocity.normalized) < 30f && (rb.velocity.magnitude + Mathf.Abs(wobbleDeltaV.magnitude)) > moveSpeed) {
                rb.velocity *= 1f - Time.fixedDeltaTime * 1.5f;
            }
        }

        Vector3 GetCurrentVelocity() {
            if (wobbler == null) return rb.velocity;
            return rb.velocity + wobbler.velocity;
        }

        bool IsOverSpeedLimit() {
            if (wobbler == null) return rb.velocity.magnitude > moveSpeed;
            return (rb.velocity + wobbler.velocity).magnitude > moveSpeed;
        }

        Vector2 GetRotateTowardsTarget() {;
            if (targetMode == TargetMode.Heading) return targetHeading;
            if (targetMode == TargetMode.Position) return (targetPosition - transform.position).normalized;
            return Vector2.zero;
        }

        // Wobble is achieved by using a "counterweight" strategy (an independent rigidbody that wobbles opposite the gameobject)
        // When calculating wobble, we check the following (independent for each axis):
        // - distance between the world origin and wobbler position
        // - velocity diff between rigidbodies
        // We then determine when to flip the wobble direction based on distance traversed
        // Force is only applied if below the arbitrarily-set wobbleMaxSpeed
        void Applywobble() {
            if (wobbler == null) return;
            if (!enemy.isAlive) {
                Destroy(wobbler.gameObject);
                return;
            }
            if (kamikaze) {
                wobbler.transform.position = Vector3.zero;
                return;
            }
            if (wobbleForce.x > 0f && wobbleMaxSpeed.x > 0f && wobbleMaxDistance.x > 0f) {
                wobbleDeltaP.x = Mathf.Sign(wobbleDir.x) * (0f - wobbler.transform.position.x);
                wobbleDeltaV.x = Mathf.Sign(wobbleDir.x) * (0f - wobbler.velocity.x);
                if (wobbleDeltaP.x > wobbleMaxDistance.x) wobbleDir.x *= -1f;
                if (wobbleDeltaV.x <= wobbleMaxSpeed.x) AddWobbleForce(Vector2.right * wobbleForce.x * wobbleDir.x);
            }
            if (wobbleForce.y > 0f && wobbleMaxSpeed.y > 0f && wobbleMaxDistance.y > 0f) {
                wobbleDeltaP.y = Mathf.Sign(wobbleDir.y) * (0f - wobbler.transform.position.y);
                wobbleDeltaV.y = Mathf.Sign(wobbleDir.y) * (0f - wobbler.velocity.y);
                if (wobbleDeltaP.y > wobbleMaxDistance.y) wobbleDir.y *= -1f;
                if (wobbleDeltaV.y <= wobbleMaxSpeed.y) AddWobbleForce(Vector2.up * wobbleForce.y * wobbleDir.y);
            }

            // wobbleForce.x = Mathf.Sign(Mathf.Cos(wobbleTime / _wobbleFreq.x + _wobbleOffset.x * Mathf.PI)) * _wobbleMag.x;
            // wobbleForce.y = Mathf.Sign(Mathf.Cos(wobbleTime / _wobbleFreq.y + _wobbleOffset.y * Mathf.PI)) * _wobbleMag.y;
            // wobbleTime += Time.fixedDeltaTime;
            // wobbleTime = wobbleTime % 2f;
            // // add velocity to the enchilada
        }

        void AddWobbleForce(Vector2 value, ForceMode2D mode = ForceMode2D.Force) {
            if (!Utils.IsObjectOnScreen(gameObject, Camera.main, -1f)) return;
            (Vector2 min, Vector2 max) = Utils.GetScreenBounds(Camera.main, -3f);
            if (rb != null) rb.AddForce(value, mode);
            if (wobbler != null) wobbler.AddForce(-value, mode);
        }

        void ApplyOffscreenBrakes() {
            if (Utils.IsObjectOnScreen(gameObject)) return;
            if (!Utils.IsObjectHeadingAwayFromCenterScreen(gameObject, rb)) return;
            // excuse me sir, do you know how fast you were going?
            rb.velocity = Vector2.ClampMagnitude(rb.velocity, moveSpeed);
        }

        bool CheckRaycastHazard(Vector2 direction, float distance) {
            hit = Physics2D.Raycast(transform.position, direction, distance, jukeLayerAvoid);
            return hit.collider != null;
        }

        //
        // DEBUG
        //
        void OnDrawGizmos() {
            if (!debug) return;
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, transform.position + heading);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + headingAdjusted);
            Gizmos.color = Color.cyan;

            if (rb == null) return;
            if (wobbler == null) return;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + rb.velocity + wobbler.velocity);
        }
    }
}
