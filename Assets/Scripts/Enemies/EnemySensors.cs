using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Core;
using Physics;

namespace Enemies {

    public class EnemySensors : MonoBehaviour {

        [SerializeField] bool debug = false;
        [SerializeField] float raycastInterval = 0.2f;
        [SerializeField] float obstacleAvoidDist = 0.75f;

        // callbacks
        System.Action<Vector2, ULayerType> OnSensorTriggered;

        // cached
        EnemyShip enemy;
        EnemyMovement movement;
        Rigidbody2D rb;
        Vector2 velocity;
        // TODO: REMOVE
        // List<Vector2> raycastHeadings = new List<Vector2>();
        List<RaycastCheck> raycastChecks = new List<RaycastCheck>();
        bool shouldSense = false;
        bool didHit = false;
        bool isRaycasting = false;
        RaycastCheck closestRaycastCheck;
        RaycastCheck noopRaycastCheck;

        Vector2 accumulationVector;

        float[] sensorAngles = new float[] {
            0f,
            45f,
            90f,
            135f,
            180f,
            225f,
            270f,
            315f,
        };

        // add array of sensor vector3s
        // raycast for all in array
        // 
        void Start() {
            enemy = GetComponent<EnemyShip>();
            movement = GetComponent<EnemyMovement>();
            rb = GetComponent<Rigidbody2D>();

            for (int i = 0; i < sensorAngles.Length; i++) {
                // TODO: REMOVE
                // raycastHeadings.Add(transform.position + Quaternion.AngleAxis(sensorAngles[i], Vector3.forward) * Vector2.down);
                // raycastChecks.Add(new RaycastCheck(Quaternion.AngleAxis(sensorAngles[i], Vector3.forward) * Vector2.down));
                // raycastChecks.Add(new RaycastCheck(sensorAngles[i], enemy.GetColliders()));
                raycastChecks.Add(new RaycastCheck(transform.position, sensorAngles[i], gameObject.GetInstanceID(), ULayer.Enemies.mask));
            }

            noopRaycastCheck = new RaycastCheck(Vector2.zero, 0f, 0, ULayer.Enemies.mask);

            StartCoroutine(CheckSurroundings());
        }

        void Update() {
            // Debug.Log(GetQuantizedVelocityAngle());
        }

        void FixedUpdate() {
            if (rb != null) velocity = rb.velocity;

            foreach (var raycastCheck in raycastChecks) {
                raycastCheck.FixedTick();
            }

            if (shouldSense && !isRaycasting) {
                shouldSense = false;
                isRaycasting = true;
                didHit = false;
                accumulationVector = Vector2.zero;
                closestRaycastCheck = noopRaycastCheck;
                foreach (var raycastCheck in raycastChecks) {
                    raycastCheck.CheckForHit(transform.position,
                        GetObstacleAvoidDistance(raycastCheck.angle),
                        GetAdjustmentAngle(raycastCheck),
                        ULayer.Enemies.mask | ULayer.Asteroids.mask | ULayer.Station.mask
                    );
                    if (raycastCheck.didHit) {
                        didHit = true;
                        accumulationVector += raycastCheck.heading * raycastCheck.closeness;
                        if (closestRaycastCheck == noopRaycastCheck || raycastCheck.closeness > closestRaycastCheck.closeness) {
                            closestRaycastCheck = raycastCheck;
                        }
                    }

                    // instead of calling callbacks individually for each hit,
                    // instead combine the hit heading into a single accumulation vector
                    // and weight each according to sensor distance (less distance receives more weight)

                }

                if (didHit) {
                    if (OnSensorTriggered != null) OnSensorTriggered.Invoke(accumulationVector, closestRaycastCheck.layerType);
                    if (movement != null) movement.NotifySensorTriggered(accumulationVector, closestRaycastCheck.layerType);
                }
                isRaycasting = false;
            }
        }

        IEnumerator CheckSurroundings() {
            while (enemy.isAlive) {
                shouldSense = true;
                yield return new WaitForSeconds(raycastInterval);
            }
        }

        float GetQuantizedAngle(Vector2 heading) {
            return Utils.AbsAngle(Mathf.Floor(Vector2.SignedAngle(Vector2.down, heading) / 45f) * 45f);
        }

        bool AngleMatchesCurrentVelocity(float angle) {
            return GetQuantizedAngle(velocity) == Utils.AbsAngle(angle);
        }

        float GetObstacleAvoidDistance(float angle) {
            if (AngleMatchesCurrentVelocity(angle)) return obstacleAvoidDist + obstacleAvoidDist * velocity.magnitude;
            return obstacleAvoidDist;
        }

        float GetAdjustmentAngle(RaycastCheck raycastCheck) {
            return AngleMatchesCurrentVelocity(raycastCheck.angle) ? Vector2.SignedAngle(raycastCheck.heading, velocity) : 0f;
        }

        void OnDrawGizmos() {
            if (!debug) return;
            foreach (var raycastCheck in raycastChecks) {
                raycastCheck.OnDebug(transform.position, GetObstacleAvoidDistance(raycastCheck.angle), GetAdjustmentAngle(raycastCheck));
            }
        }
    }
}

