using System.Collections;
using UnityEngine;

using Audio;
using Damage;
using Core;

namespace Weapons
{

    public enum ProjectileDeathReason {
        Collision,
        Guardians,
    }

    public enum ProjectileRemovalMode {
        OutWithABang,
        Quiet,
    }

    public class Projectile : MonoBehaviour
    {
        [Header("General Settings")][Space]
        [SerializeField] ProjectileRemovalMode removalMode;
        [SerializeField] float moveSpeed = 5f;
        [SerializeField] float turnSpeed = 1f;
        [SerializeField] float lifetime = 10f;
        [SerializeField] Vector3 initialHeading = Vector3.up;
        [SerializeField] int numCollisionsMax = 1;
        [SerializeField][Range(0f, 1f)] float ricochetProbability = 0.2f;
        [SerializeField][Range(0f, 180f)] float ricochetAngle = 60f;
        [SerializeField][Range(0f, 90f)] float ricochetVariance = 20f;
        [SerializeField] float outOfRange = 20f;

        [Header("Behaviour")][Space]
        [SerializeField][Range(0f, 1f)][Tooltip("Higher number = less notify enemy incoming")] float stealthFactor = 0f;

        [Header("Effects")][Space]
        [SerializeField] bool explosive = false;
        [SerializeField] float explosionLifetime = 5f;
        [SerializeField] float impactLifetime = 1f;
        [SerializeField] GameObject explosionFX;
        [SerializeField] GameObject impactFX;

        [Header("Audio")][Space]
        [SerializeField] Sound impulseSound;
        [SerializeField] LoopableSound thrustSound;
        [SerializeField] Sound impactSound;
        [SerializeField] Sound impactShieldSound;
        [SerializeField] Sound ricochetSound;
        [SerializeField] Sound destroyedSound;

        // components
        BoxCollider2D box;
        CircleCollider2D circle;
        CapsuleCollider2D capsule;
        SpriteRenderer sr;
        TrailRenderer[] trails;
        Rigidbody2D rb;

        // cached
        Vector3 startingPosition;
        Vector3 heading;
        Vector3 velocity;
        Transform target;
        DamageDealer damageDealer;

        // state
        bool _isAlive = true;
        float t = 0;
        float height = 0.5f;
        int numCollisions = 0;
        ProjectileDeathReason deathReason;

        // public
        public bool isAlive => _isAlive;

        public void SetTarget(Transform _target) {
            target = _target;
        }

        void Init() {
            _isAlive = true;
            height = CalcHeight();
            heading = initialHeading;
            // point heading in direction of rotation
            heading = Quaternion.AngleAxis(transform.rotation.eulerAngles.z, Vector3.forward) * heading;
            velocity = heading * moveSpeed;
            target = null;
            startingPosition = transform.position;

            impulseSound.Init(this);
            thrustSound.Init(this);
            impactSound.Init(this);
            impactShieldSound.Init(this);
            destroyedSound.Init(this);
            ricochetSound.Init(this);
            impulseSound.Play();
            thrustSound.Play();

            AppIntegrity.AssertPresent<DamageDealer>(damageDealer);
            damageDealer.RegisterCallbacks(OnHit, OnDeath);
        }

        void Start() {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponentInChildren<SpriteRenderer>();
            trails = GetComponentsInChildren<TrailRenderer>();
            box = GetComponent<BoxCollider2D>();
            circle = GetComponent<CircleCollider2D>();
            capsule = GetComponent<CapsuleCollider2D>();
            damageDealer = GetComponent<DamageDealer>();
            Init();
        }

        void Update() {
            if (!_isAlive) return;
            UpdateHeading();
            if (rb == null) MoveViaTransform();
            t += Time.deltaTime;
            if (t > lifetime) OnDeathByGuardians();
            if (!Utils.IsObjectOnScreen(gameObject)) OnDeathByGuardians();
            if ((transform.position - startingPosition).magnitude > outOfRange) OnDeathByGuardians();
        }

        void FixedUpdate() {
            if (rb != null) MoveViaRigidbody();
        }

        void UpdateHeading() {
            if (!_isAlive) {
                velocity *= 0.05f;
                return;
            }
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
            if (!_isAlive) return;
            transform.position += velocity * Time.deltaTime;
        }

        void MoveViaRigidbody() {
            if (!_isAlive) return;
            rb.velocity = velocity;
        }

        void OnHit(DamageableType damageableType) {
            if (!_isAlive) return;
            numCollisions++;
            if (damageableType == DamageableType.Shield) {
                impactShieldSound.Play();
            } else {
                impactSound.Play();
            }
            bool ShouldRichochet = damageableType == DamageableType.Shield || UnityEngine.Random.Range(0f, 1f) <= ricochetProbability;
            if (explosive || numCollisions >= numCollisionsMax || !ShouldRichochet) {
                if (damageableType != DamageableType.Shield && impactFX != null) {
                    Destroy(Instantiate(impactFX, transform.position, transform.rotation * Quaternion.Euler(0, 0, 180f)), impactLifetime);
                }
                OnDeath();
            } else {
                Ricochet();
            }
        }

        void Ricochet() {
            if (!_isAlive) return;
            ricochetSound.Play();
            heading = -heading;
            Quaternion ricochet = GetRicochet();
            heading = (ricochet * heading).normalized;
            velocity = heading * moveSpeed * 2f;
            transform.rotation = transform.rotation * ricochet * Quaternion.Euler(0f, 0f, 180f);
            transform.position += heading * height;
            if (damageDealer != null) {
                // once ricochet happens, let own bullets damage parentActor
                damageDealer.SetIgnoreUUID(null);
                damageDealer.SetIgnoreTag("");
            }
        }

        void OnDeathByGuardians() {
            deathReason = ProjectileDeathReason.Guardians;
            OnDeath();
        }

        public void OnDeath() {
            if (!_isAlive) return;
            _isAlive = false;
            if (damageDealer != null) damageDealer.enabled = false;
            destroyedSound.Play();
            thrustSound.Stop();
            StartCoroutine(IDeath());
        }

        IEnumerator IDeath() {
            if (sr != null) sr.enabled = false;

            if (removalMode != ProjectileRemovalMode.Quiet || deathReason != ProjectileDeathReason.Guardians) {
                destroyedSound.Play();
                if (explosionFX != null) {
                    GameObject instance = Instantiate(explosionFX, transform.position, Quaternion.identity);
                    Explosion splosion = instance.GetComponent<Explosion>();
                    if (splosion != null && damageDealer != null) splosion.SetIsDamageByPlayer(damageDealer.GetIsDamageByPlayer());
                    Destroy(instance, explosionLifetime);
                }
            }

            if (deathReason == ProjectileDeathReason.Collision && trails != null) {
                foreach (var tr in trails) {
                    if (tr != null) tr.enabled = false;
                }
            }

            while (impactSound.isPlaying) yield return null;
            while (destroyedSound.isPlaying) yield return null;
            Destroy(gameObject, 1f);
        }

        float CalcHeight() {
            if (box != null) {
                return box.size.y;
            }
            if (circle != null) {
                return circle.radius * 2;
            }
            if (capsule) {
                return capsule.size.y;
            }
            return height;
        }

        Quaternion GetRicochet() {
            return Quaternion.Euler(
                0,
                0,
                ricochetAngle *
                // random [-1, 1]
                ((float)UnityEngine.Random.Range(0, 2) - 0.5f) * 2f +
                // variance
                UnityEngine.Random.Range(-ricochetVariance, ricochetVariance));
        }
    }
}
