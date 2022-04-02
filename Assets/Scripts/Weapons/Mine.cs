using UnityEngine;

using Damage;
using System.Collections;
using Audio;
using Core;

namespace Weapons
{
    
    public class Mine : DamageableBehaviour
    {
        [Header("Mine")][Space]
        [SerializeField] bool activeAtStart = true;
        [SerializeField] float activationDelay = 0.5f;
        [SerializeField] float tripTime = 0.05f;
        [SerializeField] GameObject explosion;

        [Header("Physics")][Space]
        [SerializeField] AnimationCurve shockwaveVelocity;
        [SerializeField] float shockwaveDuration;

        [Header("Audio")][Space]
        [SerializeField] Sound deploySound;
        [SerializeField] LoopableSound beepingSound;
        [SerializeField] LoopableSound trippedSound;

        // components
        Rigidbody2D rb;
        Animator anim;
        SpriteRenderer sr;

        // state
        bool isActive = false;
        bool isTripped = false;
        bool isSploded = false;
        Timer shockwavePositionTimer = new Timer(TimerDirection.Increment, TimerStep.FixedDeltaTime);

        // state - animation
        const string ANIM_MINE_OFF = "MineOff";
        const string ANIM_MINE_ON = "MineBlinking";
        string currentAnimState;
        string nextAnimState;

        // state - velocity
        Vector2 initialVelocity;
        Vector2 blastbackDirection;

        public void SetVelocity(Vector2 value) {
            if (rb == null) return;
            initialVelocity = value;
            rb.velocity = value;
        }

        // Start is called before the first frame update
        void Start()
        {
            rb = GetComponentInChildren<Rigidbody2D>();
            anim = GetComponentInChildren<Animator>();
            sr = GetComponentInChildren<SpriteRenderer>();
            ResetHealth();
            SetColliders();
            RegisterHealthCallbacks(OnDeath, Utils.__NOOP__, Utils.__NOOP__);

            deploySound.Init(this);
            beepingSound.Init(this);
            trippedSound.Init(this);

            shockwavePositionTimer.SetDuration(shockwaveDuration);
            if (activeAtStart) StartCoroutine(IActivate());
            if (rb != null) initialVelocity = rb.velocity;

            StartCoroutine(IPlayDeploySound());
        }

        // Update is called once per frame
        void Update()
        {
            Animate();
            TickHealth();
        }

        // void FixedUpdate() {
        //     if (rb == null) return;
        //     rb.velocity = shockwavePositionTimer.active
        //         ? Vector2.Lerp(
        //             rb.velocity + blastbackDirection * shockwaveVelocity.Evaluate(shockwavePositionTimer.value),
        //             initialVelocity,
        //             shockwavePositionTimer.value)
        //         : rb.velocity;
        //     shockwavePositionTimer.Tick();
        // }

        // TODO: CONSIDER CHANGING TO A CIRCLECAST
        void OnTriggerEnter2D(Collider2D other) {
            if (!isActive || isTripped || isSploded) return;

            if (other.tag == UTag.Mine) return;

            if ((
                other.tag == UTag.Bullet ||
                other.tag == UTag.DisruptorRing ||
                other.tag == UTag.Explosion ||
                other.tag == UTag.NukeCore
            )) {
                if (shield <= 0f) {
                    // don't call Explode directly as DamageableBehaviour.Die() handles some stuff internally, like disabling colliders and such
                    TakeDamage(100f, DamageType.Instakill);
                }
                return;
            }

            if (other.tag == UTag.NukeShockwave) {
                HandleShockwaveHit(other);
                return;
            }

            if (other.tag == UTag.Laser) {
                // the laser projectile's own DamageDealer will take care of damaging the mine's shield, if applicable
                return;
            }

            Trip();
        }

        void Animate() {
            if (anim == null) return;
            if (!isAlive) {
                anim.speed = 0f;
                return;
            }
            anim.speed = isTripped ? 0.25f : 1f;
            nextAnimState = isActive ? ANIM_MINE_ON : ANIM_MINE_OFF;
            if (currentAnimState == nextAnimState) return;
            currentAnimState = nextAnimState;
            anim.Play(currentAnimState);
        }

        void HandleShockwaveHit(Collider2D other) {
            // Vector2
            // get direction of shockwave - `transform.position - other.transform.position`
            // start shockwaveTimer
            // dynamically change velocity per animation curve (shockwavePositionTimer x shockwaveVelocity)
            shockwavePositionTimer.Start();
            blastbackDirection = (transform.position - other.transform.position).normalized;
        }

        void OnDeath(DamageType damageType, bool isDamageByPlayer) {
            Explode(damageType == DamageType.InstakillQuiet);
        }

        void Activate() {
            if (!isAlive || isTripped || isSploded) return;
            isActive = true;
            StartCoroutine(IPlayActivateSound());
        }

        void Trip() {
            if (!isAlive || isTripped || isSploded) return;
            isTripped = true;
            trippedSound.Play();
            beepingSound.Stop();
            StartCoroutine(ITripped());
        }

        void Explode(bool quiet = false) {
            if (isSploded) return;
            isSploded = true;
            StartCoroutine(ISplode(quiet));
        }

        IEnumerator IPlayDeploySound() {
            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 0.2f));
            deploySound.Play();
        }

        IEnumerator IPlayActivateSound() {
            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 0.2f));
            beepingSound.Play();
        }

        IEnumerator ISplode(bool quiet = false) {
            // wait a small amount of time to create cascade chain explosions
            yield return new WaitForSeconds(0.1f);
            if (sr != null) sr.enabled = false;
            if (anim != null) anim.speed = 0f;
            trippedSound.Stop();
            if (explosion != null && !quiet) Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(gameObject, 5f);
        }

        IEnumerator IActivate() {
            yield return new WaitForSeconds(activationDelay);
            Activate();
        }

        IEnumerator ITripped() {
            yield return new WaitForSeconds(tripTime);
            TakeDamage(100f, DamageType.Instakill);
        }
    }
}

