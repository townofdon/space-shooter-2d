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
        [SerializeField] CircleCollider2D nukeCore;
        [SerializeField] ParticleSystem shockwaveParticle;
        [SerializeField] SpriteRenderer spriteRenderer;

        // cached
        ParticleSystem.MinMaxCurve curve;
        float startSize = 1f;
        float size = 1f;
        float lifetime = 1f;
        float nukeCoreRadius = 1f;
        Vector3 scale;

        // state
        Vector3 hitDist;
        Vector3 blastDirection;
        float t = 0f;

        void Start() {
            AppIntegrity.AssertPresent<GameObject>(nukeShockwave);
            AppIntegrity.AssertPresent<CircleCollider2D>(nukeCore);
            AppIntegrity.AssertPresent<ParticleSystem>(shockwaveParticle);
            AppIntegrity.AssertPresent<SpriteRenderer>(spriteRenderer);

            curve = shockwaveParticle.sizeOverLifetime.size;
            startSize = shockwaveParticle.main.startSize.constant;
            lifetime = shockwaveParticle.main.startLifetime.constant;
            spriteRenderer.enabled = false;
            nukeCoreRadius = nukeCore.radius;
            nukeCore.enabled = false;
            t = 0f;

            StartCoroutine(NukeFX());
        }

        void Update() {
            size = Mathf.Max(startSize * colliderMultiple * curve.Evaluate(t / lifetime), 0f);
            scale.x = size;
            scale.y = size;
            nukeShockwave.transform.localScale = scale;
            t += Time.deltaTime;

            if (t < timeCausingDamage) BlastRadius();
        }

        void BlastRadius() {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, nukeCoreRadius);
            foreach (var hit in hits) {
                DamageReceiver actor = hit.GetComponent<DamageReceiver>();
                if (actor != null) {
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
                    if (actor.rigidbody) {
                        blastDirection = (actor.rigidbody.transform.position - transform.position);
                        actor.rigidbody.AddForce(blastDirection.normalized * (1f / blastDirection.magnitude) * blastForce);
                    }
                }
            }
        }

        IEnumerator NukeFX() {
            yield return GameFeel.PauseTime(0.15f, 0.1f);
            yield return GameFeel.ShakeScreen(Camera.main);
        }
    }
}

