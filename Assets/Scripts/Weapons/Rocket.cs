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
        [SerializeField] float startDelay = 0.5f;
        [SerializeField] float lifetime = 10f;
        [SerializeField] Vector3 initialHeading = Vector3.up;
        [SerializeField] float outOfRange = 20f;

        [Header("Effects")][Space]
        [SerializeField] bool explosive = false;
        [SerializeField] float explosionLifetime = 5f;
        [SerializeField] ParticleSystem thrustFX;
        [SerializeField] GameObject explosion;

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

        // state
        bool isThrusting = false;
        bool isAlive = true;
        float t = 0;
        float height = 0.5f;

        public void SetTarget(Transform _target) {
            target = _target;
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
            thrustSound.Play();

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
        }

        void Activate() {
          if (thrustFX != null && !thrustFX.isPlaying) thrustFX.Play();
          thrustSound.Play();
          isThrusting = true;
        }

        void Update() {
            UpdateHeading();
            if (rb == null) MoveViaTransform();
            t += Time.deltaTime;
            if (t > startDelay) Activate();
            if (t > lifetime) OnDeath();
            if ((transform.position - startingPosition).magnitude > outOfRange) OnDeath();
        }

        void FixedUpdate() {
            if (rb != null) MoveViaRigidbody();
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
            rb.velocity = velocity;
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
                Destroy(Instantiate(explosion, transform.position, Quaternion.identity), explosionLifetime);
            }
            Destroy(gameObject);
        }
    }
}
