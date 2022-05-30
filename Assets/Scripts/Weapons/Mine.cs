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
        [SerializeField] Animator animBlink;
        [SerializeField] Animator animPulse;

        [Header("Physics")][Space]
        [SerializeField] AnimationCurve shockwaveVelocity;
        [SerializeField] float shockwaveDuration;

        [Header("Trip Target Behaviour")]
        [Space]
        [SerializeField][Range(0f, 20f)] float attractRadius = 1f;
        [SerializeField][Range(0f, 20f)] float attractMin = .25f;
        [SerializeField][Range(0.1f, 50f)] float accelMod = 1f;
        [SerializeField][Range(1f, 100f)] float topSpeed = 20f;
        [SerializeField][Range(1f, 100f)] float targetDrag = 2f;

        [Header("Audio")][Space]
        [SerializeField] Sound deploySound;
        [SerializeField] LoopableSound beepingSound;
        [SerializeField] LoopableSound trippedSound;

        // components
        Rigidbody2D rb;
        Collider2D col;
        SpriteRenderer[] sprites;

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

        // state - target
        Transform target;
        float distanceToTarget;
        Vector2 attraction;

        // Start is called before the first frame update
        void Start()
        {
            rb = GetComponentInChildren<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            sprites = GetComponentsInChildren<SpriteRenderer>();
            ResetHealth();
            SetColliders();
            RegisterHealthCallbacks(OnDeath, Utils.__NOOP__, Utils.__NOOP__);

            deploySound.Init(this);
            beepingSound.Init(this);
            trippedSound.Init(this);

            shockwavePositionTimer.SetDuration(shockwaveDuration);
            if (activeAtStart) StartCoroutine(IActivate());
            if (rb != null) initialVelocity = rb.velocity;
            if (animPulse != null) animPulse.enabled = false;

            StartCoroutine(IPlayDeploySound());
        }

        // Update is called once per frame
        void Update()
        {
            Animate();
            TickHealth();
        }

        void FixedUpdate() {
            LatchToTarget();
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
            if (other.tag == UTag.Pickup) return;
            if (!Utils.IsObjectOnScreen(gameObject, Utils.GetCamera(), 0f)) return;

            if ((
                other.tag == UTag.Bullet ||
                other.tag == UTag.DisruptorRing ||
                other.tag == UTag.Explosion ||
                other.tag == UTag.NukeCore ||
                other.tag == UTag.Detector
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

            target = other.transform;

            Trip();
        }

        void LatchToTarget() {
            if (!isTripped || target == null) return;
            attraction = GetAttractionVector();
            if (attraction == Vector2.zero) {
                rb.drag = targetDrag;
                return;
            }
            rb.drag = 0f;
            rb.AddForce(GetAttractionVector());
            rb.velocity = Vector2.ClampMagnitude(rb.velocity, topSpeed);
            // aim directly at the player
            rb.velocity = Vector2.ClampMagnitude((target.position - transform.position).normalized * topSpeed, rb.velocity.magnitude);
        }

        void Animate() {
            if (animBlink == null) return;
            if (!isAlive) return;
            animBlink.speed = isTripped ? 2f : 1f;
            nextAnimState = isActive ? ANIM_MINE_ON : ANIM_MINE_OFF;
            if (currentAnimState == nextAnimState) return;
            currentAnimState = nextAnimState;
            animBlink.Play(currentAnimState);
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
            col.enabled = true;
            if (animBlink != null) animBlink.enabled = true;
            if (animPulse != null) animPulse.enabled = true;
            StartCoroutine(IPlayActivateSound());
        }

        void Trip() {
            if (!isAlive || isTripped || isSploded) return;
            isTripped = true;
            trippedSound.Play();
            beepingSound.Stop();
            if (animPulse != null) animPulse.gameObject.SetActive(false);
            SetInvulnerable(true);
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
            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 0.2f));
            if (sprites != null) foreach (SpriteRenderer sr in sprites) if (sr != null) sr.enabled = false;
            if (animBlink != null) animBlink.enabled = false;
            if (animBlink != null) animBlink.speed = 0f;
            if (animPulse != null) animPulse.enabled = false;
            if (animPulse != null) animPulse.gameObject.SetActive(false);
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

        // yes this could def be generalized
        Vector2 GetAttractionVector() {
            if (target == null) return Vector2.zero;
            distanceToTarget = Vector2.Distance(transform.position, target.position);
            if (distanceToTarget > attractRadius) return Vector2.zero;
            if (distanceToTarget < attractMin) return Vector2.zero;
            float force = (9.81f * accelMod) / Mathf.Pow(distanceToTarget, 2f);
            return (target.position - transform.position).normalized * Mathf.Min(force, topSpeed * 2);
        }
    }
}

