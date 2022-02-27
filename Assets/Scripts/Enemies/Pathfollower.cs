using System.Collections.Generic;
using Core;
using UnityEngine;

namespace Enemies {

    public enum PathfinderTargetMode {
        Waypoints,
        Heading,
    }

    public enum PathfinderLoopMode {
        HaltAtEnd,
        Teleport,
        Circular,
        Destroy,
    }

    public class Pathfollower : MonoBehaviour
    {
        [SerializeField] bool debug = false;
        [SerializeField] PathfinderTargetMode targetMode;
        [SerializeField] PathfinderLoopMode loopMode;
        [SerializeField] float waypointTriggerRadius = 0.25f;
        [SerializeField] Transform path;

        // components
        EnemyShip enemy;
        EnemyMovement movement;

        // cached
        List<Transform> waypoints = new List<Transform>();
        float initialDrag;
        Vector2 minBounds;
        Vector2 maxBounds;
 
        // state
        int waypointIndex = 0;
        Vector3 lastOrigin;
        Vector3 target;
        bool hasCrossedHeadingX = false;
        bool hasCrossedHeadingY = false;
        bool isOffscreen = false;

        // FSM STATE
        bool _isPathComplete = false;
        bool _isStarted = false;
        bool _isRunning = false;

        public bool isPathComplete => _isPathComplete;
        public bool isStarted => _isStarted;
        public bool isRunning => _isRunning;
        public bool hasWaypoints => waypoints.Count > 0;

        public void SetTargetMode(PathfinderTargetMode mode) {
            targetMode = mode;
        }

        public void SetLoopMode(PathfinderLoopMode mode) {
            loopMode = mode;
        }

        public void SetWaypoints(List<Transform> waypoints) {
            this.waypoints = new List<Transform>();
            foreach (Transform waypoint in waypoints) this.waypoints.Add(waypoint);
        }

        public void Begin() {
            _isStarted = true;
            _isRunning = true;
            _isPathComplete = false;
        }

        public void Resume() {
            if (waypoints.Count > 0 && enemy != null) {
                Init(GetClosestWaypointIndex(), false);
            }
        }

        public void Halt() {
            movement.SetHeading(Vector3.zero);
            movement.SetTarget(transform.position);
            _isRunning = false;
        }

        void Start() {
            enemy = Utils.GetRequiredComponent<EnemyShip>(gameObject);
            movement = Utils.GetRequiredComponent<EnemyMovement>(gameObject);
            (minBounds, maxBounds) = Utils.GetScreenBounds(Camera.main, -1f);
            if (path != null) {
                SetWaypointsFromPath();
                Begin();
            }
            Init();
        }

        void FixedUpdate() {
            FollowPath();
            StayOnScreen();
        }

        void Init(int initialWaypointIndex = 1, bool shouldSetImmediateVelocity = true) {
            if (waypoints.Count <= 0) return;
            if (enemy == null) return;
            if (movement == null) return;
            // the first waypoint is the spawn point, so thus the first target should be the second waypoint
            waypointIndex = initialWaypointIndex;
            hasCrossedHeadingX = false;
            hasCrossedHeadingY = false;
            UpdateTarget();
            if (shouldSetImmediateVelocity) movement.SetImmediateVelocity(1f);
        }

        void SetWaypointsFromPath() {
            if (path == null) return;
            waypoints = new List<Transform>();
            foreach (Transform child in path) waypoints.Add(child);
        }

        void SetHeading() {
            if (isOffscreen) {
                movement.SetHeading((GetCurrentWaypointVector().normalized + GetOffscreenCorrectionVector().normalized).normalized);
            } else {
                // set heading to be the vector from one waypoint to the next
                movement.SetHeading(GetCurrentWaypointVector().normalized);
            }
        }

        void UpdateTarget() {
            if (waypointIndex >= waypoints.Count) return;
            target = waypoints[waypointIndex].position;
            lastOrigin = transform.position;
            switch(targetMode) {
                case PathfinderTargetMode.Heading:
                    SetHeading();
                    break;
                case PathfinderTargetMode.Waypoints:
                default:
                    movement.SetTarget(target);
                    break;
            }
        }

        void TargetNextWaypoint() {
            lastOrigin = transform.position;
            waypointIndex++;
            hasCrossedHeadingX = false;
            hasCrossedHeadingY = false;
            if (waypointIndex >= waypoints.Count) {
                OnPathEnd();
            } else {
                UpdateTarget();
            }
        }

        void FollowPath() {
            if (!_isRunning) return;
            if (_isPathComplete) return;
            if (enemy == null) return;
            if (movement == null) return;
            if (!enemy.isAlive) return;
            if (enemy.timeHit > 0) return;
            if (waypoints.Count == 0) return;
            if (waypointIndex >= waypoints.Count) return;

            if (targetMode == PathfinderTargetMode.Waypoints) {
                // we keep track of separate axis crossings to avoid circling around a waypoint indefinitely
                if (Mathf.Abs(target.x - transform.position.x) <= waypointTriggerRadius) hasCrossedHeadingX = true;
                if (Mathf.Abs(target.y - transform.position.y) <= waypointTriggerRadius) hasCrossedHeadingY = true;
                if (hasCrossedHeadingX && hasCrossedHeadingY) TargetNextWaypoint();
            }
            if (targetMode == PathfinderTargetMode.Heading) {
                if ((transform.position - lastOrigin).magnitude >= GetCurrentWaypointVector().magnitude) {
                    if (IsLastWaypoint() && (loopMode == PathfinderLoopMode.Teleport || loopMode == PathfinderLoopMode.Destroy)) {
                        // make sure enemy is offscreen before looping
                        if (!Utils.IsObjectOnScreen(gameObject)) TargetNextWaypoint();
                    } else {
                        TargetNextWaypoint();
                    }
                }
            }
        }

        void StayOnScreen() {
            if (targetMode != PathfinderTargetMode.Heading) return;
            if (!_isRunning) return;
            if (_isPathComplete) return;
            if (enemy == null) return;
            if (movement == null) return;
            if (!enemy.isAlive) return;
            if (enemy.timeHit > 0) return;
            if (waypoints.Count == 0) return;
            if (waypointIndex >= waypoints.Count) return;
            if (isOffscreen) {
                if (!Utils.IsObjectOnScreen(gameObject)) return;
                isOffscreen = false;
                SetHeading();
                return;
            }
            if (Utils.IsObjectOnScreen(gameObject)) return;
            isOffscreen = true;
            SetHeading();
        }

        Vector2 GetOffscreenCorrectionVector() {
            return new Vector2(
                (transform.position.x > Mathf.Max(maxBounds.x, GetCurrentWaypointPosition().x)) ? -1f :
                (transform.position.x < Mathf.Min(minBounds.x, GetCurrentWaypointPosition().x)) ? 1f :
                0f,
                (transform.position.y > Mathf.Max(maxBounds.y, GetCurrentWaypointPosition().y)) ? -1f :
                (transform.position.y < Mathf.Min(minBounds.y, GetCurrentWaypointPosition().y)) ? 1f :
                0f);
        }

        void OnPathEnd() {
            waypointIndex = 0;
            switch (loopMode)
            {
                case PathfinderLoopMode.HaltAtEnd:
                    _isPathComplete = true;
                    Halt();
                    break;
                case PathfinderLoopMode.Destroy:
                    enemy.OnDeath();
                    break;
                case PathfinderLoopMode.Circular:
                    UpdateTarget();
                    break;
                case PathfinderLoopMode.Teleport:
                default:
                    if (waypoints.Count > 0) transform.position = waypoints[0].position;
                    Init();
                    break;
            }
        }

        int GetPrevWaypointIndex() {
            return waypointIndex > 0 ? waypointIndex - 1 : waypoints.Count - 1;
        }

        int GetClosestWaypointIndex() {
            if (waypoints.Count <= 0) return -1;
            if (movement == null) return -1;
            float min = 999f;
            float current = 999f;
            int closestIndex = 0;
            for (int i = 0; i < waypoints.Count; i++)
            {
                current = (waypoints[i].position - transform.position).magnitude;
                if (current < min) {
                    min = current;
                    closestIndex = i;
                }
            }
            return closestIndex;
        }

        Vector2 GetCurrentWaypointVector() {
            return (
                waypoints[waypointIndex].position -
                waypoints[GetPrevWaypointIndex()].position
            );
        }

        Vector2 GetCurrentWaypointPosition() {
            return waypoints[waypointIndex].position;
        }

        bool IsLastWaypoint() {
            return waypointIndex >= waypoints.Count - 1;
        }

        //
        // DEBUG
        //
        void OnDrawGizmos() {
            if (!debug) return;
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (i == waypointIndex) Gizmos.color = Color.green;
                else Gizmos.color = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.5f);
                Gizmos.DrawSphere(waypoints[i].position, .2f);
            }
        }

        


        // PHYSICS STUFF
        // 
        // Below you can see the by-product of me trying to figure out how to get a particle
        // to accelerate to a target speed. The problem I ran into:
        // - Since the force applied via rb.AddForce() is continuous, I needed to apply less force as
        //   the object neared the target speed
        // - I found that anytime I modifying rb.velocity instead of overwriting it, drag would also
        //   get applied, thus nulling out my efforts to get the object up to max speed. This happened
        //   regardless of any method I tried - Vector3.MoveTowards, calculating the exact force needed
        //   to reach final velocity, etc.
        // - I also tried completely removing drag, but it turns out that this is a core movement
        //   mechanic in this space game. Drag is useful to allow characters to turn in mid-air;
        //   the character can simply thrust towards the target vector without worrying about nulling
        //   out its inertia.
        // - SOLUTION: I simply cached the velocity in a Vector3 and overwrote rb.velocity each time.
        //   For bumps, hits and such, I added a check to determine if the cached v is different from
        //   the current v, and if so, update the cached v (this will ease the velocity delta).

        // void OldCalcs() {
        //     // ease into velocity to accommodate other physics forces
        //     velocity = Vector3.MoveTowards(velocity, heading * enemy.moveSpeed, 2f * enemy.moveSpeed * Time.fixedDeltaTime);
        //     // direct assignment
        //     rb.velocity = velocity;
        //     // alt method: trying to constantly accel up to a limit
        //     if (rb.velocity.magnitude < enemy.moveSpeed) {
        //         // rb.AddForce(heading * enemy.moveSpeed * 2f * rb.mass);
        //         rb.AddForce(velocity * 1.75f * rb.mass);
        //     }
        // }

        // // see: https://www.calculatorsoup.com/calculators/physics/velocity-calculator-vuas.php
        // // also, pretty sure this is the calculation that sebastian lague used
        // // (final_velocity ** 2 + initial_velocity ** 2) / displacement * 2
        // // also, displacement cannot be zero
        // void CalcVelocity() {
        //     float delta = (
        //         Mathf.Pow(enemy.moveSpeed * GetDragInverse(), 2f) -
        //         Mathf.Pow(Mathf.Max(rb.velocity.magnitude, 2f), 2f)
        //     ) / (2 * Mathf.Max(rb.velocity.magnitude, 2f));
        //     Debug.Log("delta=" + delta + " >> velocity=" + rb.velocity.magnitude + " >> drag^-1=" + GetDragInverse());
        //     rb.AddForce(heading * delta * GetDragInverse() * rb.mass);
        // }

        // // THIS METHOD INVERSES UNITY'S DRAG CALCULATION:
        // // velocity *= ( 1f - deltaTime * drag)
        // // see: https://answers.unity.com/questions/652010/how-drag-is-calculated-by-unity-engine.html
        // float GetDragInverse()
        // {
        //     if (Time.fixedDeltaTime * rb.drag >= 1f) return 1f;
        //     return 1f / ( 1f - Time.fixedDeltaTime * rb.drag);
        // }

        // // void CalcVelocity() {
        // //     float requiredSpeed = GetRequiredVelocityChange(enemy.moveSpeed, rb.drag);
        // //     float currentSpeed = GetRequiredVelocityChange(rb.velocity.magnitude, rb.drag);
        // //     float delta = requiredSpeed - Mathf.Min(currentSpeed, requiredSpeed);
        // //     Vector2 force = heading.normalized * delta;
        // //     // rb.AddForce(force, ForceMode2D.Impulse);

        // //     rb.velocity += force;

        // //     // TODO: REMOVE
        // //     Debug.Log("delta=" + delta + " >> velocity=" + rb.velocity.magnitude);
        // // }

        // // we need to take drag into account in order to accelerate the obj to max speed
        // // see: http://answers.unity.com/answers/819444/view.html
        // float GetRequiredVelocityChange(float aFinalSpeed, float aDrag)
        // {
        //     float m = Mathf.Clamp01(aDrag * Time.fixedDeltaTime);
        //     return aFinalSpeed * m / (1f - m);
        // }

        // float GetRequiredAcceleraton(float aFinalSpeed, float aDrag)
        // {
        //     return GetRequiredVelocityChange(aFinalSpeed, aDrag) / Time.fixedDeltaTime;
        // }
    }
}
