using System.Collections.Generic;
using UnityEngine;

using Game;
using Core;
using Weapons;

namespace Damage
{

    [RequireComponent(typeof(Collider2D))]

    public class DamageDealer : MonoBehaviour
    {
        [Header("Damage Settings")][Space]
        [SerializeField] DamageType damageType = DamageType.Default;
        [SerializeField][Range(0f, 10f)] float baseDamageMultiplier = 1f;

        [Header("Circlecast Settings")][Space]
        [SerializeField] bool preferCirclecast = false;

        [Header("Raycast Settings")][Space]
        [SerializeField] bool raycast = false;
        [SerializeField][Tooltip("Minimum distance the projectile must travel before ignore settings are turned off and it can start raycasting")]
        float minSafeDistance = 2f;

        [Header("Ignore Settings")][Space]
        [SerializeField] bool ignoreParentUUID = true;
        [SerializeField] LayerMask ignoreLayers;
        [SerializeField] string ignoreTag;
        [SerializeField] List<Collider2D> ignoreColliders = new List<Collider2D>();

        // callbacks
        System.Action<DamageableType> onHit;
        System.Action onDeath;

        // cached
        Rigidbody2D rb;
        Collider2D _collider;
        DamageableBehaviour parentActor;
        DamageClass damageClass;
        Vector3 initPosition;
        Vector3 prevPosition;

        // state
        System.Nullable<System.Guid> ignoreUUID;
        float circlecastRadius = 1f;
        bool passedSafeDistance = false;
        bool hitThisFrame = false; // prevent multiple hits in the same frame
        float damageWaitTime = 0f; // prevent damage spamming -> spread over time interval
        float upgradeDamageMultiplier = 1f;

        // getters
        new public Collider2D collider => _collider;

        public void SetIgnoreUUID(System.Nullable<System.Guid> uuid) {
            ignoreUUID = uuid;
        }

        public void SetIgnoreTag(string tag) {
            ignoreTag = tag;
        }

        public void SetDamageMultiplier(float value) {
            upgradeDamageMultiplier = value;
        }

        public void RegisterCallbacks(System.Action<DamageableType> OnHit, System.Action OnDeath) {
            onHit = OnHit;
            onDeath = OnDeath;
        }

        void Start() {
            rb = GetComponentInParent<Rigidbody2D>();
            parentActor = GetComponentInParent<DamageableBehaviour>();
            damageClass = GameManager.current.GetDamageClass(damageType);
            prevPosition = transform.position;
            initPosition = transform.position;

            if (ignoreParentUUID && parentActor!= null && parentActor.uuid != null) {
                SetIgnoreUUID(parentActor.uuid);
            }

            if (preferCirclecast) {
                _collider = Utils.GetRequiredComponent<CircleCollider2D>(gameObject);
                circlecastRadius = ((CircleCollider2D)_collider).radius;
                _collider.enabled = false;
            } else {
                _collider = Utils.GetRequiredComponent<Collider2D>(gameObject);
            }
        }

        void Update() {
            hitThisFrame = false;
            HandleCirclecast();
            HandleRaycast();
            damageWaitTime = Mathf.Max(0f, damageWaitTime - Time.deltaTime);
        }

        void HandleCirclecast() {
            if (!preferCirclecast) return;
            Collider2D[] otherColliders = Physics2D.OverlapCircleAll(transform.position, circlecastRadius);
            foreach (var other in otherColliders) {
                HandleColliderHit(other, damageType == DamageType.Disruptor);
            }
        }

        void HandleRaycast() {
            if (!raycast) return;
            if (Vector3.Distance(initPosition, transform.position) >= minSafeDistance) {
                passedSafeDistance = true;
            }
            if (passedSafeDistance) {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, (prevPosition - transform.position).normalized, (prevPosition - transform.position).magnitude);
                if (hit.collider) HandleColliderHit(hit.collider);
            }
            prevPosition = transform.position;
        }

        void OnTriggerEnter2D(Collider2D other) {
            HandleColliderHit(other);
        }

        void HandleColliderHit(Collider2D other, bool makeFramerateIndependent = false) {
            if (!enabled) return;
            if (damageWaitTime > 0f) return;
            if (!passedSafeDistance && hitThisFrame) return;
            if (!passedSafeDistance && ignoreTag == other.tag) return;
            if (ULayerUtils.LayerMaskContainsLayer(ignoreLayers, other.gameObject.layer)) return;
            if (other.tag == UTag.Laser || other.tag == UTag.Bullet) {
                HandleHitOtherProjectile(other);
                return;
            }
            DamageReceiver actor = other.GetComponent<DamageReceiver>();
            if (actor == null) {
                // InvokeCallback(onHit, DamageableType.Default);
            } else {
                if (ignoreUUID != null && ignoreUUID == actor.uuid) return;
                if (damageType == DamageType.Collision) {
                    HandleJarringCollision(actor);
                    return;
                }
                if (actor.TakeDamage(damageClass.baseDamage * baseDamageMultiplier * upgradeDamageMultiplier * (makeFramerateIndependent ? Time.deltaTime : 1f), damageType)) {
                    hitThisFrame = true;
                }
                if (rb != null) {
                    actor.TakeImpactForce(rb.velocity, rb.mass, damageClass.throwbackForceMultiplier);
                }
                InvokeCallback(onHit, actor.damageableType);
            }
        }

        void HandleHitOtherProjectile(Collider2D other) {
            if (this.tag == UTag.DisruptorRing || this.damageType == DamageType.Disruptor) {
                if (parentActor != null) {
                    parentActor.DrainShield(
                        GameManager.current.GetWeaponClass(Weapons.WeaponType.DisruptorRing).shieldDrain * 0.25f
                    );
                }
                Projectile projectile = other.GetComponent<Projectile>();
                if (projectile != null) projectile.OnDeath();
            }
            if (this.tag == UTag.Explosion || this.damageType == DamageType.Explosion) {
                InvokeCallback(onDeath);
            }
        }

        void HandleJarringCollision(DamageReceiver actor) {
            if (damageType != DamageType.Collision) return;
            if (actor == null) return;
            if (actor.rigidbody == null) return;
            if (rb == null) return;
            float collisionDamage = GameManager.current.GetDamageClass(DamageType.Collision).baseDamage;
            float collisionMagnitude = (actor.rigidbody.velocity.magnitude + rb.velocity.magnitude);
            // a very inelastic collision
            Vector3 forceToSelf = (actor.rigidbody.velocity - rb.velocity);
            Vector3 forceToActor = (rb.velocity - actor.rigidbody.velocity);
            rb.AddForce(forceToSelf, ForceMode2D.Impulse);
            actor.rigidbody.AddForce(forceToActor * damageClass.throwbackForceMultiplier, ForceMode2D.Impulse);
            float damageToActor = collisionDamage * collisionMagnitude * Mathf.Max(1f, (rb.mass / actor.rigidbody.mass) * 0.05f);
            actor.TakeDamage(damageToActor * baseDamageMultiplier, DamageType.Collision);
        }
        void HandleJarringCollision(Collider2D other) {
            if (damageType != DamageType.Collision) return;
            DamageReceiver actor = other.gameObject.GetComponent<DamageReceiver>();
            HandleJarringCollision(actor);
        }

        void IgnoreColliders() {
            foreach (var ignoreCollider in ignoreColliders) {
                Physics2D.IgnoreCollision(_collider, ignoreCollider);
            }
        }

        void InvokeCallback(System.Action callback) {
            if (callback != null) callback.Invoke();
        }
        void InvokeCallback(System.Action<DamageableType> callback, DamageableType damageableType) {
            if (callback != null) {
                callback.Invoke(damageableType);
            }
        }
    }
}
