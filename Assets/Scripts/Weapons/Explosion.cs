using System.Collections;
using UnityEngine;

using Core;
using Damage;

namespace Weapons
{
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(ParticleSystem))]

    public class Explosion : MonoBehaviour
    {
        [Header("Splosion Settings")][Space]
        [SerializeField] float timeCausingDamage = 1.5f;
        [SerializeField] float damageBegin = 200f;
        [SerializeField] float damageEnd = 10f;
        [SerializeField] float damageAtEdge = 0.25f;
        [SerializeField] float blastForce = 5f;

        [Header("Components")][Space]
        [SerializeField] ParticleSystem splosion;
        [SerializeField] GameObject nukeShockwave2;
        [SerializeField] new CircleCollider2D collider;

        // cached
        ParticleSystem.MinMaxCurve curve;
        float secondShockwaveDelay = 1f;
        float startSize = 1f;
        float size = 1f;
        float lifetime = 1f;
        float blastRadius = 1f;
        Vector3 scale;

        // state
        Vector3 hitDist;
        Vector3 blastDirection;
        float t = 0f;

        void Start() {
            AppIntegrity.AssertPresent<CircleCollider2D>(collider);
            AppIntegrity.AssertPresent<ParticleSystem>(splosion);

            collider.enabled = false;
            t = 0f;

            StartCoroutine(GameFeel.ShakeGamepad(0.75f, 1f, 1f));
            StartCoroutine(NukeFX());
        }

        void Update() {
            t += Time.deltaTime;

            if (t < timeCausingDamage) BlastRadius();
        }

        void BlastRadius() {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, blastRadius);
            foreach (var hit in hits) {
                Projectile projectile = hit.GetComponent<Projectile>();
                if (projectile != null || hit.tag == UTag.Bullet || hit.tag == UTag.Laser) {
                    Destroy(hit.gameObject);
                    return;
                }
                DamageReceiver actor = hit.GetComponent<DamageReceiver>();
                if (actor != null) {
                    hitDist = hit.transform.position - transform.position;
                    if (hitDist.magnitude > blastRadius) return;

                    actor.TakeDamage(
                        // damage amount per time
                        Mathf.Lerp(damageBegin, damageEnd, t / timeCausingDamage) *
                        // account for distance from explosion center
                        Mathf.Lerp(1f, damageAtEdge, hitDist.magnitude / blastRadius),
                        DamageType.Explosion
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

