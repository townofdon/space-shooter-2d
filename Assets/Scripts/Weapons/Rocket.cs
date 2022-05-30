using System.Collections;
using UnityEngine;

using Audio;
using Damage;
using Core;

namespace Weapons
{
    
    public class Rocket : MonoBehaviour
    {
        [Header("General Settings")][Space]
        [SerializeField] bool debug;
        [SerializeField] float moveSpeed = 5f;
        [SerializeField] float accel = 5f;
        [SerializeField] float turnSpeed = 1f;
        [SerializeField] float turnSpeedPreLaunch = 3f;
        [SerializeField] float startDelay = 0.25f;
        [SerializeField] float launchDrag = 1f;
        [SerializeField] float lifetime = 10f;
        [SerializeField] Vector3 initialHeading = Vector3.up;
        [SerializeField] float proximityDetonation = 0.5f;
        [SerializeField] float proximityDelay = 0.15f;
        [SerializeField] float outOfRange = 20f;
        [SerializeField] float cascadeExplodeDelay = 0.4f;

        [Header("Targeting System")]
        [Space]
        [SerializeField] bool targetingEnabled;
        [SerializeField] GameObject detector;

        [Header("Effects")][Space]
        [SerializeField] ParticleSystem thrustFX;
        [SerializeField] GameObject explosion;
        [SerializeField] float explosionLifetime = 5f;

        [Header("Audio")][Space]
        [SerializeField] Sound lockSound;
        [SerializeField] Sound impulseSound;
        [SerializeField] LoopableSound thrustSound;

        // components
        BoxCollider2D box;
        CircleCollider2D circle;
        CapsuleCollider2D capsule;
        SpriteRenderer sr;
        TrailRenderer tr;
        Rigidbody2D rb;

        // cached
        Vector3 startingPosition;
        Vector3 targetHeading;
        Vector3 heading;
        Vector3 velocity;
        Transform target;
        DamageableBehaviour targetActor;
        DamageDealer damageDealer;
        Collider2D proximityOtherCollider;

        // state
        bool isThrusting = false;
        bool isAlive = true;
        float t = 0;
        float height = 0.5f;
        Vector2 launchForce;

        // state - aim
        Quaternion startingRotation = Quaternion.identity;
        // Vector2 aimVector = Vector2.down;
        // float aimAngle = 0f;
        // Quaternion aim = Quaternion.identity;

        public void SetTarget(Transform _target) {
            target = _target;
            targetActor = target.GetComponent<DamageableBehaviour>();
            if (targetingEnabled) lockSound.Play();
        }

        public void SetIgnoreUUID(System.Guid? uuid) {
            damageDealer.SetIgnoreUUID(uuid);
        }

        public void SetIgnoreLayers(LayerMask layerMask) {
            damageDealer.SetIgnoreLayers(layerMask);
        }

        public void Launch(Vector2 force) {
            launchForce = force;
        }

        public void Explode(DamageType damageType) {
            if (damageType == DamageType.Explosion && t < cascadeExplodeDelay) return;
            OnDeath();
        }

        void Init() {
            isAlive = true;
            heading = initialHeading;
            // point heading in direction of rotation
            heading = Quaternion.AngleAxis(transform.rotation.eulerAngles.z, Vector3.forward) * heading;
            velocity = heading * moveSpeed;
            target = null;
            startingPosition = transform.position;
            startingRotation = transform.rotation;

            lockSound.Init(this);
            impulseSound.Init(this);
            thrustSound.Init(this);
            impulseSound.Play();

            AppIntegrity.AssertPresent<DamageDealer>(damageDealer);
            damageDealer.RegisterCallbacks(OnHit, OnDeath);
        }

        void Awake() {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponentInChildren<SpriteRenderer>();
            tr = GetComponentInChildren<TrailRenderer>();
            box = GetComponent<BoxCollider2D>();
            circle = GetComponent<CircleCollider2D>();
            capsule = GetComponent<CapsuleCollider2D>();
            damageDealer = GetComponent<DamageDealer>();
            Init();
        }

        void Start() {
            OnLaunch();
            if (targetingEnabled && detector != null) {
                GameObject go = new GameObject("DetectorContainer");
                detector.transform.SetParent(go.transform);
                RocketTargetingSystem targeting = detector.GetComponent<RocketTargetingSystem>();
                if (targeting != null) targeting.SetRocket(this);
                detector.SetActive(true);
            }
        }

        void Update() {
            RotateTowardsTarget();
            UpdateHeading();
            HandleDetector();
            t += Time.deltaTime;
            if (rb == null) MoveViaTransform();
            if (t > startDelay) Activate();
            if (t > lifetime) OnDeath();
            if ((transform.position - startingPosition).magnitude > outOfRange) OnDeath();
        }

        void FixedUpdate() {
            if (rb != null) MoveViaRigidbody();
            HandleProximityDetonation();
        }

        void HandleDetector() {
            if (detector == null) return;
            if (isAlive && targetingEnabled && detector.activeSelf) {
                detector.transform.position = transform.position;
                detector.transform.rotation = transform.rotation;
            } else {
                detector.SetActive(false);
            }
        }

        void OnLaunch() {
            if (rb == null) return;
            rb.drag = launchDrag;
            rb.AddForce(launchForce + rb.velocity, ForceMode2D.Impulse);
        }

        void Activate() {
          if (thrustFX != null && !thrustFX.isPlaying) thrustFX.Play();
          thrustSound.Play();
          isThrusting = true;
          if (rb != null) rb.drag = 0f;
        }

        void RotateTowardsTarget() {
            if (!HasTarget()) return;
            if (!isAlive) return;

            targetHeading = (target.position - transform.position).normalized;
            heading = Vector3.RotateTowards(
                heading,
                // adjusted heading - factors in rocket's own velocity
                (targetHeading + (targetHeading * moveSpeed - GetCurrentVelocity())).normalized,
                (isThrusting ? turnSpeed : turnSpeedPreLaunch) * Mathf.PI * Time.fixedDeltaTime,
                1f
            ).normalized;
            rb.rotation = Vector2.SignedAngle(initialHeading, heading);
            // rb.rotation = Vector2.SignedAngle(startingRotation * initialHeading, -heading);
            // transform.rotation = startingRotation * Quaternion.Euler(0f, 0f, Vector2.SignedAngle(startingRotation * initialHeading, heading));

            // rotation doesn't actually affect flight path; it's just for show (since we're setting velocity manually)
            // aimVector = Vector2.MoveTowards(aimVector, heading, turnSpeed * Time.deltaTime);
            // aimAngle = Vector2.SignedAngle(startingRotation * initialHeading, aimVector);
            // transform.rotation = startingRotation * Quaternion.AngleAxis(aimAngle, Vector3.forward);
        }

        Vector3 GetCurrentVelocity() {
            if (rb == null) return Vector3.zero;
            return (Vector3)rb.velocity;
        }

        void UpdateHeading() {
            if (!isAlive) {
                velocity *= 0.05f;
                return;
            }
            if (!isThrusting) return;
            // velocity = Vector3.MoveTowards(velocity, transform.rotation * initialHeading * moveSpeed, 2f * moveSpeed * Time.fixedDeltaTime);
        }

        void MoveViaTransform() {
            if (!isThrusting || !isAlive) return;
            transform.position += velocity * Time.deltaTime;
        }

        void MoveViaRigidbody() {
            if (!isThrusting || !isAlive) return;
            // rb.velocity = velocity;
            rb.drag = 0f;
            rb.AddForce(heading * accel);
            if (rb.velocity.magnitude > moveSpeed) {
                // apply drag if over speed limit
                rb.velocity *= ( 1f - Time.fixedDeltaTime * 1.5f);
            }
        }

        void HandleProximityDetonation() {
            if (proximityDetonation <= 0f) return;
            proximityOtherCollider = Physics2D.OverlapCircle(transform.position, proximityDetonation);
            if (proximityOtherCollider == null) return;
            if (proximityOtherCollider.tag != UTag.EnemyShip && proximityOtherCollider.tag != UTag.EnemyTurret) return;
            // at this point we done tripped the thing
            StartCoroutine(IOnDeath(proximityDelay));
        }

        void OnHit(DamageableType damageableType) {
            if (!isAlive) return;
            OnDeath();
        }

        void OnDeath() {
            if (!isAlive) return;

            isAlive = false;
            if (sr != null) sr.enabled = false;
            if (tr != null) tr.enabled = false;
            if (thrustFX != null) thrustFX.Stop();
            if (thrustSound != null) thrustSound.Stop();

            if (explosion != null) {
                GameObject instance = Instantiate(explosion, transform.position, Quaternion.identity);
                Explosion splosion = instance.GetComponent<Explosion>();
                if (splosion != null) splosion.SetIsDamageByPlayer(damageDealer.GetIsDamageByPlayer());
                Destroy(instance, explosionLifetime);
            }
            Destroy(gameObject);
            if (detector != null) Destroy(detector);
        }

        IEnumerator IOnDeath(float delay) {
            yield return new WaitForSeconds(delay);
            OnDeath();
        }

        bool HasTarget() {
            if (target == null) return false;
            if (targetActor != null && !targetActor.isAlive) return false;
            return true;
        }

        private void OnDrawGizmos() {
            if (!debug) return;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + heading * moveSpeed);
            if (target != null) {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, target.position);
            }
        }
    }
}
