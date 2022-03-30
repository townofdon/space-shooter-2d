using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Core;
using Damage;
using Audio;
using Pickups;

namespace NPCs {
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(DamageReceiver))]

    public class Ordnance : DamageableBehaviour {
        [Header("Ordnance")]
        [Space]
        [SerializeField] GameObject explosion;
        [SerializeField] GameObject spriteContainer;
        [SerializeField] LoopableSound engineSound;
        [SerializeField] Sound explodeSound;


        [Header("Pickups")]
        [Space]
        [SerializeField] PickupsSpawnConfig pickups;

        // cached
        SpriteRenderer sr;
        Rigidbody2D rb;

        void Start() {
            sr = GetComponentInChildren<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            AppIntegrity.AssertPresent<GameObject>(explosion);
            AppIntegrity.AssertPresent<GameObject>(spriteContainer);

            ResetHealth();
            SetColliders();
            RegisterHealthCallbacks(OnDeath, OnHealthDamage, Utils.__NOOP__);
            engineSound.Init(this);
            explodeSound.Init(this);

            engineSound.Play();
        }

        void Update() {
            TickHealth();
        }

        public void OnHealthDamage(float amount, DamageType damageType, bool isDamageByPlayer) { }
        public void OnDeath(DamageType damageType, bool isDamageByPlayer) {
            // stop dat spinning
            if (damageType != DamageType.InstakillQuiet) {
                explodeSound.Play();
                Instantiate(explosion, transform);
                pickups.Spawn(transform.position, rb);
            }
            engineSound.Stop();
            StartCoroutine(IDeathFX());
        }

        IEnumerator IDeathFX() {
            if (sr != null) sr.enabled = false;
            spriteContainer.SetActive(false);
            while (engineSound.isPlaying) yield return null;
            while (explodeSound.isPlaying) yield return null;
            Destroy(gameObject);
        }
    }
}

