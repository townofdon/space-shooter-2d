using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;

namespace Enemies {

    public class Pathfinder : MonoBehaviour
    {
        [SerializeField] Vector2 offscreenPosition = new Vector2(100f, -100f);
        [SerializeField] float waypointTriggerRadius = 0.25f;
        [SerializeField] bool debug = false;

        // components
        EnemyShip enemy;
        Rigidbody2D rb;

        // cached
        List<Transform> _wayPoints;
        WaveConfigSO _wave;
        float initialDrag;

        // state
        int wayPointIndex = 0;
        Vector3 velocity; // the direction we're currently moving
        Vector3 heading; // the direction we want to go this frame
        Vector3 optimalHeading; // the direction we need to go this frame accounting for velocity
        Vector3 target; // the ultimate destination
        bool hasCrossedHeadingX = false;
        bool hasCrossedHeadingY = false;

        void Start() {
            enemy = Utils.GetRequiredComponent<EnemyShip>(gameObject);
            rb = Utils.GetRequiredComponent<Rigidbody2D>(gameObject);
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            Init();
        }

        void FixedUpdate() {
            FollowPath();
        }
        
        void Init() {
             if (_wave != null) {
                _wayPoints = _wave.GetWaypoints();
                wayPointIndex = 0;
                hasCrossedHeadingX = false;
                hasCrossedHeadingY = false;
                transform.position = _wayPoints[wayPointIndex].position;
                InitHeading();
                UpdateTarget();
            } else {
                transform.position = offscreenPosition;
            }
        }

        void InitHeading() {
            heading = (_wayPoints[wayPointIndex].position - transform.position).normalized;
        }

        void UpdateTarget() {
            if (wayPointIndex < _wayPoints.Count) {
                target = _wayPoints[wayPointIndex].position;
            }
        }

        void TargetNextWaypoint() {
            wayPointIndex++;
            hasCrossedHeadingX = false;
            hasCrossedHeadingY = false;
            if (wayPointIndex >= _wayPoints.Count) {
                Init();
            } else {
                UpdateTarget();
            }
        }

        void FollowPath() {
            if (!enemy.isAlive) return;

            if (enemy.timeHit > 0) {
                velocity = rb.velocity;
                return;
            }

            if (_wave == null || _wayPoints.Count == 0) return;

            if (wayPointIndex >= _wayPoints.Count) return;

            // rotate the enemy so that it turns smoothly
            heading = Vector3.RotateTowards(heading, (target - transform.position).normalized, 2f * enemy.turnSpeed * 2f * Mathf.PI * Time.fixedDeltaTime, enemy.turnSpeed * Time.fixedDeltaTime).normalized;

            // vector maths
            optimalHeading = (heading * enemy.moveSpeed - (Vector3)rb.velocity).normalized;
            rb.AddForce(optimalHeading * enemy.accel);
            if (Vector2.Angle(heading, rb.velocity.normalized) < 30f && rb.velocity.magnitude > enemy.moveSpeed) {
                rb.velocity *= ( 1f - Time.fixedDeltaTime * 1.5f);
            }

            // we keep track of separate axis crossings to avoid circling around a waypoint indefinitely
            if (Mathf.Abs(target.x - transform.position.x) <= waypointTriggerRadius) hasCrossedHeadingX = true;
            if (Mathf.Abs(target.y - transform.position.y) <= waypointTriggerRadius) hasCrossedHeadingY = true;
            if (hasCrossedHeadingX && hasCrossedHeadingY) TargetNextWaypoint();
        }

        public void SetWave(WaveConfigSO wave) {
            _wave = wave;
            Init();
        }

        void OnDrawGizmos() {
            if (!debug) return;
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, transform.position + heading);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + optimalHeading);
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
