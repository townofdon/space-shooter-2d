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
        [Header("Damage Settings")]
        [Space]
        [SerializeField] DamageType damageType = DamageType.Default;
        [SerializeField][Range(0f, 10f)] float baseDamageMultiplier = 1f;
        [SerializeField] bool isDamageByPlayer = false;
        [SerializeField] bool dieOnPlayerCollision = false;

        [Space]

        [Header("Collision")]
        [Space]
        [SerializeField][Range(0f, 2f)] float collisionForceMod = 1f;

        [Space]

        [Header("Circlecast Settings")]
        [Space]
        [SerializeField] bool preferCirclecast = false;

        [Space]

        [Header("Raycast Settings")]
        [Space]
        [SerializeField] bool raycast = false;
        [SerializeField][Tooltip("Minimum distance the projectile must travel before ignore settings are turned off and it can start raycasting")]
        float minSafeDistance = 2f;

        [Space]

        [Header("Ignore Settings")]
        [Space]
        [SerializeField] bool ignoreParentUUID = true;
        [SerializeField] LayerMask ignoreLayers;
        [SerializeField] string ignoreTag;
        [SerializeField] List<Collider2D> ignoreColliders = new List<Collider2D>();
        [SerializeField] bool ignoreProjectiles = false;

        // callbacks
        System.Action<DamageableType> onHit;
        System.Action onRoidHitRoid;
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
        public DamageType type => damageType;

        public void SetIsDamageByPlayer(bool value) {
            isDamageByPlayer = value;
        }

        public bool GetIsDamageByPlayer() {
            return isDamageByPlayer;
        }

        public void SetIgnoreUUID(System.Nullable<System.Guid> uuid) {
            ignoreUUID = uuid;
        }

        public void SetIgnoreLayers(LayerMask layerMask) {
            ignoreLayers = layerMask;
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

        public void RegisterRoidHitRoidCallback(System.Action OnRoidHitRoid) {
            onRoidHitRoid = OnRoidHitRoid;
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

        void OnCollisionEnter2D(Collision2D other) {
            HandleCollision(other);

        }

        void OnTriggerEnter2D(Collider2D other) {
            HandleColliderHit(other);
        }

        void HandleCollision(Collision2D other) {
            if (!enabled) return;
            if (damageWaitTime > 0f) return;
            if (other.collider == null) return;
            if (damageType != DamageType.Collision) return;
            if (!passedSafeDistance && hitThisFrame) return;
            if (!passedSafeDistance && ignoreTag == other.collider.tag) return;
            if (ULayerUtils.LayerMaskContainsLayer(ignoreLayers, other.gameObject.layer)) return;
            if (other.collider.tag == UTag.Laser) return;
            if (other.collider.tag == UTag.Bullet) return;
            if (other.collider.tag == UTag.Missile) return;
            if (other.collider.tag == UTag.Nuke) return;

            DamageReceiver actor = other.collider.GetComponent<DamageReceiver>();
            if (actor == null) return;
            if (ignoreUUID != null && ignoreUUID == actor.uuid) return;

            HandleJarringCollision(actor, other.relativeVelocity.magnitude);
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
            if (other.tag == UTag.Missile || other.tag == UTag.Nuke) {
                HandleHitOtherOrdnance(other);
                return;
            }

            DamageReceiver actor = other.GetComponent<DamageReceiver>();
            if (actor == null) {
                // InvokeCallback(onHit, DamageableType.Default);
            } else {
                if (ignoreUUID != null && ignoreUUID == actor.uuid) return;
                if (damageType == DamageType.Collision) {
                    // collisions are now handled by actual colliders
                    // HandleJarringCollision(actor);
                    return;
                }
                if (actor != null && actor.TakeDamage(
                    damageClass.baseDamage * baseDamageMultiplier * upgradeDamageMultiplier * (makeFramerateIndependent ? Time.deltaTime : 1f),
                    damageType,
                    isDamageByPlayer
                )) {
                    hitThisFrame = true;
                }
                if (rb != null) {
                    actor.TakeImpactForce(rb.velocity, rb.mass, damageClass.throwbackForceMultiplier);
                }
                InvokeCallback(onHit, actor.damageableType);
            }
        }

        void HandleHitOtherProjectile(Collider2D other) {
            if (ignoreProjectiles) return;
            if (this.tag == UTag.DisruptorRing || this.damageType == DamageType.Disruptor) {
                Projectile projectile = other.GetComponent<Projectile>();
                if (projectile == null || !projectile.isAlive) return;
                projectile.OnDeath();
                DrainSelfShields();
            }
            if (this.tag == UTag.Explosion || this.damageType == DamageType.Explosion) {
                InvokeCallback(onDeath);
            }
        }

        void HandleHitOtherOrdnance(Collider2D other) {
            if (this.tag == UTag.DisruptorRing || this.damageType == DamageType.Disruptor) {
                // do not apply to the player
                if (parentActor != null && parentActor.tag == UTag.Player) return;

                DrainSelfShields();

                if (other.tag == UTag.Nuke) {
                    Nuke nuke = other.GetComponent<Nuke>();
                    if (nuke != null) nuke.Explode();
                }

                if (other.tag == UTag.Missile) {
                    Rocket missile = other.GetComponent<Rocket>();
                    if (missile != null) missile.Explode(damageType);
                }
            }
        }

        void HandleJarringCollision(DamageReceiver actor, float collisionMagnitude = 1f) {
            if (damageType != DamageType.Collision) return;
            if (actor == null) return;
            if (actor.rigidbody == null || !actor.rigidbody.simulated) return;
            if (rb == null) return;
            // float canCollideMod = actor.canCollide ? 1f : 0.1f;
            // float mSelf = Mathf.Min(0.8f, rb.mass);
            // float mOther = Mathf.Min(0.8f, actor.rigidbody.mass);
            // // a very inelastic collision
            // Vector3 forceToSelf = (actor.rigidbody.velocity * mOther - Vector2.ClampMagnitude(rb.velocity, 3f) * mSelf) * collisionForceMod * canCollideMod;
            // Vector3 forceToActor = (rb.velocity * mSelf - Vector2.ClampMagnitude(actor.rigidbody.velocity, 3f) * mOther) * collisionForceMod * canCollideMod;
            // rb.AddForce(forceToSelf, ForceMode2D.Impulse);
            // if (!actor.rigidbody.isKinematic) actor.rigidbody.AddForce(forceToActor * damageClass.throwbackForceMultiplier, ForceMode2D.Impulse);
            // float collisionMagnitude = (actor.rigidbody.velocity.magnitude + rb.velocity.magnitude);
            collisionMagnitude += (actor.rigidbody.velocity.magnitude + rb.velocity.magnitude);
            float collisionDamage = GameManager.current.GetDamageClass(DamageType.Collision).baseDamage * baseDamageMultiplier;
            float damageToActor = collisionDamage * collisionMagnitude * Mathf.Clamp((rb.mass / Mathf.Max(actor.rigidbody.mass, 0.1f)), 0.1f, 10f);
            if (collisionMagnitude < 2f) damageToActor = Mathf.Max(30f, damageToActor);
            if (collisionMagnitude < 1.5f) damageToActor = Mathf.Max(10f, damageToActor);
            if (collisionMagnitude < 1f) damageToActor = Mathf.Max(4f, damageToActor);
            if (collisionMagnitude < 0.5f) damageToActor = Mathf.Max(1f, damageToActor);
            actor.TakeDamage(damageToActor * baseDamageMultiplier, DamageType.Collision, isDamageByPlayer);
            if (actor.tag == UTag.Asteroid) InvokeCallback(onRoidHitRoid);
            if (dieOnPlayerCollision && actor.tag == UTag.Player && collisionMagnitude > 10f) parentActor.TakeDamage(1000f, DamageType.Collision);
        }

        void HandleJarringCollision(Collider2D other) {
            if (damageType != DamageType.Collision) return;
            DamageReceiver actor = other.gameObject.GetComponent<DamageReceiver>();
            HandleJarringCollision(actor);
        }

        void DrainSelfShields() {
            if (parentActor != null) {
                parentActor.DrainShield(
                    GameManager.current.GetWeaponClass(Weapons.WeaponType.DisruptorRing).shieldDrain * Time.deltaTime * 15f
                );
            }
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
