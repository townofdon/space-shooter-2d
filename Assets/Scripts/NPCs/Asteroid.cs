using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Core;
using Damage;
using Audio;
using Pickups;

namespace NPCs
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(DamageReceiver))]

    public class Asteroid : DamageableBehaviour
    {
        [Header("Asteroid")][Space]
        [SerializeField] Vector2 startHeading = Vector2.down;
        [SerializeField][Range(0f, 360)] float headingVariance = 0f;
        [SerializeField][Range(0f, 20)] float startSpeed = 0f;
        [SerializeField][Range(0f, 20)] float speedVariance = 0f;
        [SerializeField][Range(0f, 1080f)] float startRotation = 360f;
        [SerializeField] List<GameObject> pieces = new List<GameObject>();
        [SerializeField] ParticleSystem explodeFX;
        [SerializeField] Sound rockExplodeSound;
        [SerializeField] Sound rockHitRockSound;

        [Header("Pickups")]
        [Space]
        [SerializeField] PickupsSpawnConfig pickups;

        // cached
        SpriteRenderer sr;
        Rigidbody2D rb;

        void Start() {
            sr = GetComponentInChildren<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            rb.angularVelocity = UnityEngine.Random.Range(-startRotation, startRotation);

            if (rb.velocity == Vector2.zero) {
                rb.velocity = GetHeadingVariance() * startHeading.normalized * Utils.RandomVariance2(startSpeed, speedVariance, startSpeed / 2f);
            }

            ResetHealth();
            SetColliders();
            RegisterHealthCallbacks(OnDeath, OnHealthDamage, Utils.__NOOP__);
            InitRoidCollisionCallback();
            rockExplodeSound.Init(this);
            rockHitRockSound.Init(this);
        }

        void Update() {
            TickHealth();
        }

        void InitRoidCollisionCallback() {
            DamageDealer dd = GetComponentInChildren<DamageDealer>();
            if (dd == null) return;
            if (dd.type != DamageType.Collision) return;
            dd.RegisterRoidHitRoidCallback(OnRoidHitRoid);
        }

        void OnRoidHitRoid() {
            rockHitRockSound.Play();
        }

        public void OnHealthDamage(float amount, DamageType damageType, bool isDamageByPlayer) { }
        public void OnDeath(DamageType damageType, bool isDamageByPlayer) {
            // stop dat spinning
            rb.angularVelocity = 0f;
            if (damageType != DamageType.Instakill && damageType != DamageType.InstakillQuiet) {
                SpawnDebris();
                pickups.Spawn(transform.position, rb);
            }
            if (damageType != DamageType.InstakillQuiet) {
                rockExplodeSound.Play();
            }
            StartCoroutine(IDeathFX());
        }

        void SpawnDebris() {
            foreach (var piece in pieces) SpawnLilGuy(piece);
            if (UnityEngine.Random.Range(0,2) == 1 && pieces.Count > 0) SpawnLilGuy(pieces[0]);
        }

        void SpawnLilGuy(GameObject smallRock) {
            GameObject instance = Instantiate(smallRock, transform.position, Quaternion.identity);
            Rigidbody2D rbInstance = instance.GetComponent<Rigidbody2D>();
            if (rbInstance != null) rbInstance.velocity = rb.velocity + UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(0.5f, 5f);
        }

        IEnumerator IDeathFX() {
            if (sr != null) sr.enabled = false;
            if (explodeFX != null) {
                explodeFX.Play();
                while (explodeFX != null && explodeFX.isPlaying) yield return null;
            }
            while (rockExplodeSound.isPlaying) yield return null;
            Destroy(gameObject, 2f);
        }

        // copied from AsteroidLauncher
        Quaternion GetHeadingVariance() {
            return Quaternion.AngleAxis(Utils.RandomVariance(0f, headingVariance), Vector3.forward);
        }
    }
}

