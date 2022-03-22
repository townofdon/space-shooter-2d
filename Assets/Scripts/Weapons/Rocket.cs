using UnityEngine;

using Audio;
using Damage;
using Core;

namespace Weapons
{
    
    public class Rocket : MonoBehaviour
    {
        [Header("General Settings")][Space]
        [SerializeField] float moveSpeed = 5f;
        [SerializeField] float accel = 5f;
        [SerializeField] float turnSpeed = 1f;
        [SerializeField] float startDelay = 0.25f;
        [SerializeField] float launchDrag = 1f;
        [SerializeField] float lifetime = 10f;
        [SerializeField] Vector3 initialHeading = Vector3.up;
        [SerializeField] float proximityDetonation = 0.5f;
        [SerializeField] float outOfRange = 20f;

        [Header("Effects")][Space]
        [SerializeField] ParticleSystem thrustFX;
        [SerializeField] GameObject explosion;
        [SerializeField] float explosionLifetime = 5f;

        [Header("Audio")][Space]
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
        Vector3 heading;
        Vector3 velocity;
        Transform target;
        DamageDealer damageDealer;
        Collider2D proximityOtherCollider;

        // state
        bool isThrusting = false;
        bool isAlive = true;
        float t = 0;
        float height = 0.5f;
        Vector2 launchForce;

        public void SetTarget(Transform _target) {
            target = _target;
        }

        public void Launch(Vector2 force) {
            launchForce = force;
        }

        void Init() {
            isAlive = true;
            heading = initialHeading;
            // point heading in direction of rotation
            heading = Quaternion.AngleAxis(transform.rotation.eulerAngles.z, Vector3.forward) * heading;
            velocity = heading * moveSpeed;
            target = null;
            startingPosition = transform.position;

            impulseSound.Init(this);
            thrustSound.Init(this);
            impulseSound.Play();

            AppIntegrity.AssertPresent<DamageDealer>(damageDealer);
            damageDealer.RegisterCallbacks(OnHit, OnDeath);
        }

        void Start() {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponentInChildren<SpriteRenderer>();
            tr = GetComponentInChildren<TrailRenderer>();
            box = GetComponent<BoxCollider2D>();
            circle = GetComponent<CircleCollider2D>();
            capsule = GetComponent<CapsuleCollider2D>();
            damageDealer = GetComponent<DamageDealer>();
            Init();
            OnLaunch();
        }

        void Update() {
            UpdateHeading();
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

        void UpdateHeading() {
            if (!isAlive) {
                velocity *= 0.05f;
                return;
            }
            if (!isThrusting) return;
            if (target != null) {
                heading = Vector3.RotateTowards(
                    heading,
                    (target.position - transform.position).normalized,
                    turnSpeed * 2f * Mathf.PI * Time.fixedDeltaTime,
                    1f
                ).normalized;
            }
            velocity = Vector3.MoveTowards(velocity, heading * moveSpeed, 2f * moveSpeed * Time.fixedDeltaTime);
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
            if (proximityOtherCollider.tag != UTag.EnemyShip) return;
            // at this point we done tripped the thing
            OnDeath();
        }

        void OnHit(DamageableType damageableType) {
            if (!isAlive) return;
            OnDeath();
        }

        public void OnDeath() {
            if (!isAlive) return;

            isAlive = false;
            if (sr != null) sr.enabled = false;
            if (tr != null) tr.enabled = false;
            if (thrustFX != null) thrustFX.Stop();
            if (thrustSound != null) thrustSound.Stop();

            if (explosion != null) {
                Destroy(Instantiate(explosion, transform.position, Quaternion.identity), explosionLifetime);
            }
            Destroy(gameObject);
        }
    }
}
