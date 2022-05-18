using System.Collections;
using UnityEngine;

using Core;
using Damage;

namespace Weapons
{
    
    public class NukeExplosion : MonoBehaviour
    {
        [Header("Settings")][Space]
        [SerializeField] float colliderMultiple = 0.85f;
        [SerializeField] float timeCausingDamage = 1.5f;
        [SerializeField] float damageBegin = 200f;
        [SerializeField] float damageEnd = 10f;
        [SerializeField] float damageAtEdge = 0.25f;
        [SerializeField] float blastForce = 5f;

        [Header("Components")][Space]
        [SerializeField] GameObject nukeShockwave;
        [SerializeField] GameObject nukeShockwave2;
        [SerializeField] CircleCollider2D nukeCore;
        [SerializeField] ParticleSystem shockwaveParticle;
        [SerializeField] ParticleSystem shockwaveParticle2;
        [SerializeField] SpriteRenderer spriteRenderer;

        // cached
        ParticleSystem.MinMaxCurve curve;
        float secondShockwaveDelay = 1f;
        float startSize = 1f;
        float size = 1f;
        float lifetime = 1f;
        float nukeCoreRadius = 1f;
        Vector3 scale;
        Collider2D[] colliders;

        // state
        Vector3 hitDist;
        Vector3 blastDirection;
        float t = 0f;
        bool didDisableColliders = false;

        void Start() {
            AppIntegrity.AssertPresent<GameObject>(nukeShockwave);
            AppIntegrity.AssertPresent<GameObject>(nukeShockwave2);
            AppIntegrity.AssertPresent<CircleCollider2D>(nukeCore);
            AppIntegrity.AssertPresent<ParticleSystem>(shockwaveParticle);
            AppIntegrity.AssertPresent<ParticleSystem>(shockwaveParticle2);
            AppIntegrity.AssertPresent<SpriteRenderer>(spriteRenderer);
            secondShockwaveDelay = shockwaveParticle2.main.startDelay.constant;
            curve = shockwaveParticle.sizeOverLifetime.size;
            startSize = shockwaveParticle.main.startSize.constant;
            lifetime = shockwaveParticle.main.startLifetime.constant;
            spriteRenderer.enabled = false;
            nukeCoreRadius = nukeCore.radius;
            nukeCore.enabled = false;
            t = 0f;
            colliders = GetComponentsInChildren<Collider2D>();

            StartCoroutine(GameFeel.ShakeGamepad(0.75f, 1f, 1f));
            StartCoroutine(NukeFX());
        }

        void Update() {
            size = Mathf.Max(startSize * colliderMultiple * curve.Evaluate(t / lifetime), 0f);
            scale.x = size;
            scale.y = size;
            nukeShockwave.transform.localScale = scale;
            size = Mathf.Max(startSize * colliderMultiple * curve.Evaluate((t - secondShockwaveDelay) / lifetime), 0f);
            scale.x = size;
            scale.y = size;
            nukeShockwave2.transform.localScale = scale;
            t += Time.deltaTime;

            if (t < timeCausingDamage) {
                BlastRadius();
            } else if (colliders != null && !didDisableColliders) {
                foreach (Collider2D col in colliders) {
                    col.enabled = false;
                }
                didDisableColliders = true;
            }
        }

        void BlastRadius() {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, nukeCoreRadius);
            foreach (var hit in hits) {
                Projectile projectile = hit.GetComponent<Projectile>();
                if (projectile != null) {
                    projectile.OnDeath();
                    return;
                }
                Rocket rocket = hit.GetComponent<Rocket>();
                if (rocket != null) {
                    rocket.OnDeath();
                    return;
                }
                DamageReceiver actor = hit.GetComponent<DamageReceiver>();
                if (actor != null && actor.isAlive) {
                    hitDist = hit.transform.position - transform.position;
                    if (hitDist.magnitude > nukeCoreRadius) return;

                    actor.TakeDamage(
                        // damage amount per time
                        Mathf.Lerp(damageBegin, damageEnd, t / timeCausingDamage) *
                        // account for distance from nuke core center
                        Mathf.Lerp(1f, damageAtEdge, hitDist.magnitude / nukeCoreRadius),
                        DamageType.Nuke
                    );

                    // push the actor away from the center of the blast
                    if (actor.rigidbody != null) {
                        blastDirection = (actor.rigidbody.transform.position - transform.position);
                        if (blastDirection == Vector3.zero) blastDirection = Vector3.down * 0.1f;
                        actor.rigidbody.AddForce(blastDirection.normalized * (1f / blastDirection.magnitude) * blastForce);
                    }
                }
            }
        }

        IEnumerator NukeFX() {
            yield return GameFeel.PauseTime(0.15f, 0.1f);
            yield return GameFeel.ShakeScreen(Utils.GetCamera());
        }
    }
}

