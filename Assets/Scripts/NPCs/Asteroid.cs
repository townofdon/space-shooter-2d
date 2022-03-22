using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Core;
using Damage;
using Audio;

namespace NPCs
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(DamageReceiver))]

    public class Asteroid : DamageableBehaviour
    {
        [Header("Asteroid")][Space]
        [SerializeField][Range(0f, 1080f)] float startRotation = 360f;
        [SerializeField] List<GameObject> pieces = new List<GameObject>();
        [SerializeField] ParticleSystem explodeFX;
        [SerializeField] Sound rockExplodeSound;

        // cached
        SpriteRenderer sr;
        Rigidbody2D rb;

        void Start() {
            sr = GetComponentInChildren<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            rb.angularVelocity = UnityEngine.Random.Range(-startRotation, startRotation);

            ResetHealth();
            SetColliders();
            RegisterHealthCallbacks(OnDeath, OnHealthDamage, Utils.__NOOP__);
            rockExplodeSound.Init(this);
        }

        void Update() {
            TickHealth();
        }

        public void OnHealthDamage(float amount, DamageType damageType, bool isDamageByPlayer) { }
        public void OnDeath(bool isDamageByPlayer) {
            // stop dat spinning
            rb.angularVelocity = 0f;
            SpawnDebris();
            rockExplodeSound.Play();
            StartCoroutine(IDeathFX());
        }

        void SpawnDebris() {
            foreach (var piece in pieces) SpawnLilGuy(piece);
            if (UnityEngine.Random.Range(0,2) == 1 && pieces.Count > 0) SpawnLilGuy(pieces[0]);
        }

        void SpawnLilGuy(GameObject smallRock) {
            GameObject instance = Instantiate(smallRock, transform.position, Quaternion.identity);
            Rigidbody2D rbInstance = instance.GetComponent<Rigidbody2D>();
            rbInstance.velocity = rb.velocity + UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(0.5f, 5f);
        }

        IEnumerator IDeathFX() {
            if (sr != null) sr.enabled = false;
            if (explodeFX != null) {
                explodeFX.Play();
                while (explodeFX.isPlaying) yield return null;
            }
            while (rockExplodeSound.isPlaying) yield return null;
            Destroy(gameObject);
        }
    }
}

