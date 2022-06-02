using System.Collections.Generic;
using UnityEngine;

using Core;
using Player;
using Audio;
using Damage;

namespace Enemies
{
    enum TargetMode {
        Null,
        Position,
        Heading,
    }

    public enum MovementMode {
        Default,
        Kamikaze,
        MoveBetweenPoints,
        AttackRun,
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

        [Header("Mode")]
        [Space]
        [SerializeField] MovementMode mode = MovementMode.Default;
        MovementMode modePrev = MovementMode.Default;
        [Tooltip("Only applicable for MovementMode.MoveBetweenPoints")]
        [SerializeField] List<Transform> movePointLocations = new List<Transform>();
        [SerializeField] float movePointThreshold = 0.25f;
        List<Vector3> movePoints = new List<Vector3>();

        [Header("Movement")]
        [Space]
        [SerializeField] bool debug = false;
        [SerializeField] float turnSpeed = 4f;
        [SerializeField] float moveSpeed = 5f;
        [SerializeField] float accel = 10f;
        [SerializeField] bool initMoveBetweenPoints = false;

        [Header("Behaviour")]
        [Space]
        // TODO: MOVE TO EnemyBehaviour SCRIPT
        // [SerializeField] bool kamikaze = false;
        [Tooltip("Juke left or right when targeted by player")]
        [SerializeField][Range(0f, 1f)] float jukes = 0f;
        [SerializeField][Range(0f, 2f)] float jukeTargetedTime = 0f;
        [SerializeField][Range(0f, 2f)] float jukeTargetedVariance = 0f;
        [SerializeField][Range(0f, 5f)] float jukeSpeed = 0f;
        [SerializeField][Range(0f, 5f)] float jukeCooldown = 0f;
        [SerializeField] LayerMask jukeLayerAvoid;
        [Tooltip("Avoid hazards, explosives")]
        [SerializeField][Range(0f, 1f)] float avoids = 1f;
        [SerializeField][Range(1f, 10f)] float avoidSpeedMod = 1.5f;
        [SerializeField][Range(0.1f, 5f)] float avoidCooldown = 0.5f;

        [Header("Movement overrides when attacking")]
        [Space]
        [SerializeField] [Range(0f, 2f)] float atxTurnMod = 1f;
        [SerializeField] [Range(0f, 2f)] float atxMoveMod = 1f;
        [SerializeField] [Range(0f, 2f)] float atxAccelMod = 1f;

        [Header("Wobble")]
        [Space]
        [SerializeField] Rigidbody2D wobbler;
        [SerializeField] Vector2 wobbleForce;
        [SerializeField] Vector2 wobbleMaxSpeed;
        [SerializeField] Vector2 wobbleMaxDistance;

        [SerializeField] Transform wobble;
        [SerializeField] Vector2 wobbleMag;
        [SerializeField] Vector2 wobbleFreq = Vector2.one;
        [SerializeField] Vector2 wobbleOffset;

        [Header("Audio")]
        [Space]
        [SerializeField] LoopableSound engineSound;
        [SerializeField] LoopableSound agroSound;

        // components
        PlayerGeneral player;
        DamageableBehaviour self;
        Rigidbody2D rb;
        Rigidbody2D rbWobble;

        // state
        TargetMode targetMode = TargetMode.Null;
        Vector3 heading;
        Vector3 headingAdjusted;
        Vector3 targetHeading;
        Vector3 targetPosition;
        Vector3 targetPositionPrev;
        Vector3 minBounds;
        Vector3 maxBounds;

        // state - wobble
        Vector2 wobbleDir = Vector2.one;
        Vector2 wobbleDeltaP = Vector2.zero; // diff in position
        Vector2 wobbleDeltaV = Vector2.zero; // diff in velocity
        Vector2 wobblePosition;
        float wobbleTime;

        // state - juke
        bool isJuking = false;
        Timer juking = new Timer();
        float timeTargetedByPlayer = 0f;

        // state - avoid
        Vector3 avoidanceHeading;
        Timer avoiding = new Timer();

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


        public void SetMovePoints(List<Transform> _movePoints) {
            movePoints.Clear();
            foreach (Transform location in _movePoints) movePoints.Add(location.position);
        }

        public void SetMovePoints(Transform movePointsParentTransform) {
            if (movePointsParentTransform == null || movePointsParentTransform.GetChild(0) == null) return;
            movePoints.Clear();
            foreach (Transform child in movePointsParentTransform) movePoints.Add(child.position);
        }

        public void SetMode(MovementMode _mode) {
            modePrev = mode;
            mode = _mode;
        }

        public void SetKamikaze(bool shouldSetKamikazeMode) {
            if (shouldSetKamikazeMode) SetMode(MovementMode.Kamikaze);
            // kamikaze = value;
        }

        public void SetImmediateVelocity(float velocityMultiplier) {
            if (rb == null) return;
            rb.velocity = (heading * moveSpeed * velocityMultiplier);
        }

        public void NotifySensorTriggered(Vector2 sensorDirection, ULayerType layerType) {
            switch (layerType) {
                case ULayerType.Enemies:
                    if (avoiding.active) break;
                    avoidanceHeading = -sensorDirection;
                    avoiding.Start();
                    if (rb != null) rb.AddForce(-sensorDirection * rb.velocity.magnitude * 0.5f * avoids, ForceMode2D.Impulse);
                    break;
                case ULayerType.Asteroids:
                // determine if avoiding (if avoids > randomFloat)
                // try and brake first
                // try to go around the asteroid - plot a course orthogonal to the current heading
                case ULayerType.Station:
                default:
                    // do nothing
                    break;
                    // default:
            }
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

        void OnDestroy() {
            if (wobbler != null) Destroy(wobbler.gameObject);
        }

        void Start() {
            self = Utils.GetRequiredComponent<DamageableBehaviour>(gameObject);
            rb = Utils.GetRequiredComponent<Rigidbody2D>(gameObject);
            player = PlayerUtils.FindPlayer();
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            engineSound.Init(this);
            agroSound.Init(this);
            juking.SetDuration(jukeCooldown);
            avoiding.SetDuration(avoidCooldown);
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

            movePoints.Clear();
            foreach (Transform location in movePointLocations) movePoints.Add(location.position);
            if (initMoveBetweenPoints && movePoints.Count > 0 && mode == MovementMode.MoveBetweenPoints) moveTarget = movePoints[0];
            (minBounds, maxBounds) = Utils.GetScreenBounds(Utils.GetCamera(), 2f);
        }

        void Update() {
            HandleTargetBehaviour();
            juking.Tick();
            avoiding.Tick();
        }


        void FixedUpdate() {
            Handlejuke();
            Wobble();
            MoveTowardsHeading();
            ApplyOffscreenBrakes();
        }

        void HandleTargetBehaviour() {
            switch (mode) {
                case MovementMode.Kamikaze:
                    ModeKamikaze();
                    break;
                case MovementMode.MoveBetweenPoints:
                    ModeMoveBetweenPoints();
                    break;
                case MovementMode.ReflectOffScreenEdges:
                    ModeReflectOffScreenEdges();
                    break;
                case MovementMode.AttackRun:
                    ModeAttackRun();
                    break;
                case MovementMode.Default:
                default:
                    ModeDefault();
                    break;
            }
            // // if (kamikaze && player != null && player.isAlive) {
            // if (mode == MovementMode.Kamikaze && player != null && player.isAlive) {
            //     if (player == null || !player.isAlive) { SetMode(MovementMode.Default); return; }
            //     targetMode = TargetMode.Position;
            //     targetPosition = player.transform.position;
            //     agroSound.Play();
            //     engineSound.Stop();
            // } else if (mode == MovementMode.MoveBetweenPoints) {
            //     ModeMoveBetweenPoints();
            // } else if (mode == MovementMode.ReflectOffScreenEdges) {
            // } else if (mode == MovementMode.AttackRun) {
            // } else {
            //     targetPosition = targetPositionPrev;
            //     engineSound.Play();
            //     agroSound.Stop();
            // }
        }

        void ModeDefault() {
            targetPosition = targetPositionPrev;
            engineSound.Play();
            agroSound.Stop();
        }

        void ModeKamikaze() {
            if (player == null || !player.isAlive) {
                player = PlayerUtils.FindPlayer();
                targetPosition = Vector2.down * 20f;
                return;
            }
            targetMode = TargetMode.Position;
            targetPosition = player.transform.position;
            targetPosition.y = Mathf.Min(transform.position.y - 2f, player.transform.position.y);
            agroSound.Play();
            engineSound.Stop();
            if (transform.position.y < minBounds.y) self.TakeDamage(1000f, DamageType.InstakillQuiet, false);
        }

        #region MoveBetweenPoints
        Vector3 moveTarget;
        Vector3 moveTargetPrev;
        struct HasCrossed {
            public bool x;
            public bool y;
            public bool point => x && y;
        }
        HasCrossed hasCrossed = new HasCrossed();

        void ModeMoveBetweenPoints() {
            if (!self.isAlive) {
                engineSound.Stop();
                agroSound.Stop();
                return;
            }
            if (moveTarget != null) {
                targetMode = TargetMode.Position;
                targetPosition = moveTarget;
                engineSound.Play();
                agroSound.Stop();
                if (Mathf.Abs(moveTarget.x - transform.position.x) < movePointThreshold) hasCrossed.x = true;
                if (Mathf.Abs(moveTarget.y - transform.position.y) < movePointThreshold) hasCrossed.y = true;
                if (hasCrossed.point) SetMoveTarget();
            } else {
                SetMoveTarget();
            }
        }

        void SetMoveTarget() {
            hasCrossed.x = false;
            hasCrossed.y = false;
            moveTargetPrev = transform.position;
            moveTarget = GetNextMovePoint();
        }

        Vector3 GetNextMovePoint() {
            if (movePoints.Count <= 0) return Vector3.zero;
            if (movePoints.Count <= 1) return movePoints[0] == moveTarget ? moveTargetPrev : movePoints[0];
            Vector3 newTarget;
            int i = 0;
            do {
                newTarget = movePoints[UnityEngine.Random.Range(0, movePoints.Count)];
                i++; // infinite loops are scary
            } while (moveTarget == newTarget && i < 100);
            return newTarget;
        }
        #endregion MoveBetweenPoints

        #region ReflectOffScreenEdges
        void ModeReflectOffScreenEdges() { }
        #endregion ReflectOffScreenEdges

        #region AttackRun
        Transform attackOrigin;
        Transform attackTarget;
        float attackProgress;

        void ModeAttackRun() {
            if (player == null || !player.isAlive) {
                SetMode(MovementMode.Default);
                return;
            }
            targetMode = TargetMode.Position;
            // TODO: evaluate based on curve
            targetPosition = player.transform.position + Vector3.up * 1.5f;
            agroSound.Play();
            engineSound.Stop();
            // on first movement
            if (attackOrigin == null || attackTarget == null) {
                attackProgress = 0f;
                attackOrigin = transform;
                attackTarget = player.transform;
            }
        }
        #endregion AttackRun

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
            if (!self.isAlive) return;
            if (targetMode == TargetMode.Null) return;
            // rotate the enemy so that it turns smoothly
            heading = Vector3.RotateTowards(
                heading,
                GetRotateTowardsTarget(),
                2f * GetTurnSpeed() * 2f * Mathf.PI * Time.fixedDeltaTime, GetTurnSpeed() * Time.fixedDeltaTime
            ).normalized;

            // vector maths - compensate for rb's current velocity vs. heading - AND factor in avoidance vector
            headingAdjusted = (Vector3.Lerp(heading, avoidanceHeading, avoiding.value * avoids) * GetMoveSpeed() - (Vector3)GetCurrentVelocity()).normalized;
            rb.AddForce(headingAdjusted * GetAccel());
            if (Vector2.Angle(heading, rb.velocity.normalized) < 30f && (rb.velocity.magnitude + wobbleDeltaV.magnitude) > GetMoveSpeed()) {
                rb.velocity *= 1f - Time.fixedDeltaTime * 1.5f;
            }
        }

        Vector3 GetCurrentVelocity() {
            if (wobbler == null) return rb.velocity;
            return rb.velocity + wobbler.velocity;
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
        void Wobble() {
            // if (wobble == null) return;
            // if (wobbleMag.x > 0f && wobbleFreq.x > 0f) wobblePosition.x = Mathf.Sin(wobbleTime / wobbleFreq.x + wobbleOffset.x * Mathf.PI) * wobbleMag.x;
            // if (wobbleMag.y > 0f && wobbleFreq.y > 0f) wobblePosition.y = Mathf.Sin(wobbleTime / wobbleFreq.y + wobbleOffset.y * Mathf.PI) * wobbleMag.y;
            // wobbleTime += Time.fixedDeltaTime;
            // wobbleTime = wobbleTime % 2f;
            // wobble.position = wobblePosition;

            if (wobbler == null) return;
            if (!Utils.IsObjectOnScreen(gameObject, Utils.GetCamera(), -1f)) {
                wobbler.transform.position = Vector3.zero;
                wobbler.velocity = Vector3.zero;
                return;
            }
            if (!self.isAlive) {
                Destroy(wobbler.gameObject);
                return;
            }
            if (mode == MovementMode.Kamikaze) {
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
            (Vector2 min, Vector2 max) = Utils.GetScreenBounds(Utils.GetCamera(), -3f);
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

        float GetTurnSpeed() {
            switch (mode) {
                case MovementMode.Kamikaze:
                case MovementMode.AttackRun:
                    return turnSpeed * atxTurnMod;
                case MovementMode.Default:
                default:
                    return turnSpeed;
            }
        }

        float GetMoveSpeed() {
            switch (mode) {
                case MovementMode.Kamikaze:
                case MovementMode.AttackRun:
                    return Mathf.Lerp(moveSpeed, moveSpeed * avoidSpeedMod, avoiding.value) * atxMoveMod;
                case MovementMode.Default:
                default:
                    return Mathf.Lerp(moveSpeed, moveSpeed * avoidSpeedMod, avoiding.value);
            }
        }

        float GetAccel() {
            switch (mode) {
                case MovementMode.Kamikaze:
                case MovementMode.AttackRun:
                    return accel * atxAccelMod;
                case MovementMode.Default:
                default:
                    return accel;
            }
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

            if (mode == MovementMode.MoveBetweenPoints) {
                Gizmos.color = Color.yellow;
                foreach (var point in movePoints) Gizmos.DrawCube(point, Vector3.one * .2f);
            }

            if (rb == null) return;
            if (wobbler == null) return;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + rb.velocity + wobbler.velocity);
        }
    }
}
