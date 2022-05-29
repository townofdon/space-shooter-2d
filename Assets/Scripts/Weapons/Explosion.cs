using System.Collections.Generic;
using System.Collections;
using UnityEngine;

using Core;
using Damage;
using Audio;

namespace Weapons
{
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(ParticleSystem))]

    public class Explosion : MonoBehaviour
    {
        [Header("Splosion Settings")]
        [Space]
        [SerializeField] bool hasSingleFrameDamage = false;
        [SerializeField] float timeCausingDamage = 1.5f;
        [SerializeField] float damageBegin = 200f;
        [SerializeField] float damageEnd = 10f;
        [SerializeField] float damageAtEdge = 0.25f;
        [SerializeField] float blastForce = 5f;
        [SerializeField] float lifetime = 10f;

        [Header("Gamefeel")]
        [Space]
        [SerializeField] float shakeScreenMagnitude = 0.1f;
        [SerializeField] float shakeScreenDuration = 0.1f;

        [Header("Audio")]
        [Space]
        [SerializeField] Sound splosion;


        // components
        new CircleCollider2D collider;

        // cached
        ParticleSystem.MinMaxCurve curve;
        float secondShockwaveDelay = 1f;
        float startSize = 1f;
        float size = 1f;
        float blastRadius = 1f;
        Vector3 scale;

        // hit cache
        Dictionary<System.Nullable<System.Guid>, bool> hitMap = new Dictionary<System.Nullable<System.Guid>, bool>(20);

        // state
        bool isDamageByPlayer;
        Vector3 hitDist;
        Vector3 blastDirection;
        float t = 0f;

        public void SetIsDamageByPlayer(bool value) {
            isDamageByPlayer = value;
        }

        void Start() {
            collider = GetComponent<CircleCollider2D>();
            blastRadius = collider.radius;
            collider.enabled = false;
            t = 0f;

            splosion.Init(this);
            splosion.Play();
            StartCoroutine(GameFeel.ShakeGamepad(0.25f, 0.25f, 0.25f));
            StartCoroutine(GameFeel.ShakeScreen(Utils.GetCamera(), shakeScreenDuration, shakeScreenMagnitude));
            Destroy(gameObject, lifetime);
        }

        void Update() {
            if (t < timeCausingDamage) BlastRadius();
            t += Time.deltaTime;
        }

        void BlastRadius() {
            if (t > timeCausingDamage) return;
            foreach (var hit in Physics2D.OverlapCircleAll(transform.position, blastRadius * 3f)) OnHit(hit);
        }

        void OnHit(Collider2D hit) {
            // Projectile projectile = hit.GetComponent<Projectile>();
            // if (projectile != null) {
            //     projectile.OnDeath();
            //     return;
            // }

            Rocket rocket = hit.GetComponent<Rocket>();
            if (rocket != null) {
                hitDist = hit.transform.position - transform.position;
                if (hitDist.magnitude > blastRadius) return;
                StartCoroutine(BlowUpRocket(rocket));
                return;
            }

            DamageReceiver actor = hit.GetComponent<DamageReceiver>();
            if (actor != null) {
                // if actor is already damaged by this splosion, ignore
                if (hasSingleFrameDamage && actor.uuid != null && hitMap.ContainsKey(actor.uuid) && hitMap[actor.uuid]) return;

                // push the actor away from the center of the blast
                if (actor.rigidbody) {
                    blastDirection = (actor.rigidbody.transform.position - transform.position);
                    actor.rigidbody.AddForce(blastDirection.normalized * (1f / blastDirection.magnitude) * blastForce);
                }

                hitDist = hit.transform.position - transform.position;
                if (hitDist.magnitude > blastRadius) return;

                if (actor.TakeDamage(
                    // damage amount per time
                    Mathf.Lerp(damageBegin, damageEnd, t / timeCausingDamage) *
                    // account for distance from explosion center
                    Mathf.Lerp(1f, damageAtEdge, hitDist.magnitude / blastRadius),
                    DamageType.Explosion,
                    isDamageByPlayer
                ) && hasSingleFrameDamage && actor.uuid != null) hitMap[actor.uuid] = true;
                return;
            }
        }

        IEnumerator BlowUpRocket(Rocket rocket) {
            // wait a small amount of time in order to create the illusion of chain-reaction explosions
            yield return new WaitForSeconds(0.2f);
            rocket.Explode(DamageType.Explosion);
        }
    }
}

