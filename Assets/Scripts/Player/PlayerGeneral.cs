using System.Collections;
using UnityEngine;
using Core;
using Damage;
using Game;
using Audio;

namespace Player {

    public class PlayerGeneral : DamageableBehaviour
    {
        [Header("Damage Behavior")][Space]
        [SerializeField] float blastThrowback = 2f;
        [SerializeField] float collideThrowback = 5f;
        [SerializeField] float collideThrowbackFromVelocity = 0.025f;
        [SerializeField] float deathExplosiveForce = 2.5f;
        [SerializeField] float deathPartTorque = 60f;
        [SerializeField] float hitPauseDuration = 0.1f;
        [SerializeField] float hitPauseTimescale = 0.3f;

        [Header("Components")][Space]
        [SerializeField] ParticleSystem shieldEffect;
        [SerializeField] GameObject playerExplosion;
        [SerializeField] GameObject ship;
        [SerializeField] GameObject shipFlash;
        [SerializeField] GameObject[] shipParts;

        [Header("Audio")][Space]
        [SerializeField] Sound damageSound;
        [SerializeField] Sound deathSound;

        // components
        CircleCollider2D col;
        Rigidbody2D rb;
        Animator shipFlashEffect;

        // TODO: REMOVE
        PlayerInputHandler input;

        // cached
        GameObject splosion;
        Coroutine damageCoroutine;
        Coroutine deathCoroutine;

        // state
        // float health;
        // bool isAlive = true;
        // float timeHit = 0f;

        // public bool IsAlive => isAlive;

        void Start() {
            AppIntegrity.AssertPresent<ParticleSystem>(shieldEffect);
            AppIntegrity.AssertPresent<GameObject>(playerExplosion);
            AppIntegrity.AssertPresent<GameObject>(ship);
            AppIntegrity.AssertPresent<GameObject>(shipFlash);
            col = Utils.GetRequiredComponent<CircleCollider2D>(gameObject);
            rb = Utils.GetRequiredComponent<Rigidbody2D>(gameObject);
            shipFlashEffect = Utils.GetRequiredComponent<Animator>(shipFlash);
            // init
            ResetHealth();
            SetColliders();
            RegisterHealthCallbacks(OnDeath, OnHealthDamaged, Utils.__NOOP__);
            RegisterShieldCallbacks(OnShieldDepleted, OnShieldDamage, OnShieldDrain, OnShieldRechargeStart, OnShieldRechargeComplete);
            damageSound.Init(gameObject);
            deathSound.Init(gameObject);

            // TODO: REMOVE
            // StartCoroutine(damageSound.RealtimeEditorInspection());

            // TODO: REMOVE
            input = Utils.GetRequiredComponent<PlayerInputHandler>(gameObject);
        }

        void Update() {
            HandleShields();
            TickHealth();

            // TODO: REMOVE
            if (input.isDeadPressed) TakeDamage(20f);

            // move the splosion along the same trajectory as the player's previous heading
            if (splosion != null) splosion.GetComponent<Rigidbody2D>().velocity = rb.velocity * 0.25f;
        }

        void HandleShields() {
            if (shield > 0f && timeHit > 0f) {
                if (!shieldEffect.isPlaying) shieldEffect.Play();
            } else {
                shieldEffect.Stop();
            }

            if (shield <= 0f) {
                // TODO: PLAY SHIELD WARNING SOUND
            } else {
                // TODO: STOP PLAYING SOUND
            }

            if (isRechargingShield) {
                // TODO: PLAY SHIELD RECHARGE SOUND
            } else {
                // TODO: STOP PLAYING SOUND
            }
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (other.tag == UTag.NukeShockwave) {
                TriggerShockwaveBlast(other.transform.position);
                return;
            }
            DamageReceiver actor = other.GetComponent<DamageReceiver>();
            if (actor == null) return;
            if (actor.rigidbody == null) return;
            if (!actor.canCollide) return;
            float collisionDamage = GameManager.current.GetDamageClass(DamageType.Collision).baseDamage;
            float collisionMagnitude = (actor.rigidbody.velocity.magnitude + rb.velocity.magnitude);
            actor.TakeDamage(collisionDamage * collisionMagnitude, DamageType.Collision);
            this.TakeDamage(collisionDamage * collisionMagnitude, DamageType.Collision);
            float selfMagnitude = rb.velocity.magnitude;
            float otherMagnitude = actor.rigidbody.velocity.magnitude;
            // move the rigidbodies away from each other
            this.rb.velocity         = (transform.position - actor.rigidbody.transform.position).normalized * collideThrowback;
            actor.rigidbody.velocity = -rb.velocity;
            // billiards effect - whatever object is moving faster transfers the velocity to the other rigidbody
            this.rb.velocity         += rb.velocity.normalized * otherMagnitude * collideThrowbackFromVelocity;
            actor.rigidbody.velocity += actor.rigidbody.velocity.normalized * selfMagnitude * collideThrowbackFromVelocity;
        }

        void TriggerShockwaveBlast(Vector3 shockwaveOrigin) {
            Vector3 dir = transform.position - shockwaveOrigin;
            float force = blastThrowback * (4f / (dir.magnitude + 4f));
            rb.AddForce(dir.normalized * force, ForceMode2D.Impulse);
            foreach (GameObject part in shipParts) {
                Rigidbody2D rbPart = part.GetComponent<Rigidbody2D>();
                rbPart.AddForce(dir.normalized * force, ForceMode2D.Impulse);
            }
        }

        void OnHealthDamaged(float amount, DamageType damageType) {
            Debug.Log("player_damage=" + amount + " HP=" + health + " SHIELD=" + shield);
            StartCoroutine(GameFeel.PauseTime(hitPauseDuration, hitPauseTimescale));
            damageCoroutine = StartCoroutine(HullDamageAnimation());
            
            // TODO: PLAY DIFFERENT SOUNDS PER DAMAGE TYPE
            damageSound.Play();
        }

        void OnShieldDepleted() {}
        void OnShieldDamage(float _amount) {
            StartCoroutine(GameFeel.PauseTime(hitPauseDuration, hitPauseTimescale));
            // TODO: REPLACE SOUND
            damageSound.Play();
        }
        void OnShieldDrain(float _amount) {}
        void OnShieldRechargeStart() {}
        void OnShieldRechargeComplete() {}

        void BreakShipApart() {
            GameObject go = new GameObject("BrokenShipParts");
            foreach (GameObject part in shipParts) {
                part.transform.SetParent(go.transform);
                Rigidbody2D rbPart = part.GetComponent<Rigidbody2D>();
                rbPart.simulated = true;
                rbPart.freezeRotation = false;
                // TODO: ADD A LIL'MO RANDOMNESS
                Vector2 direction = (part.transform.position - transform.position).normalized;
                rbPart.AddForce(direction * deathExplosiveForce + Utils.RandomVector2() * 0.5f, ForceMode2D.Impulse);
                rbPart.AddTorque(UnityEngine.Random.Range(-deathPartTorque, deathPartTorque));
            }
        }

        void OnDeath() {
            deathCoroutine = StartCoroutine(DeathAnimation());
            deathSound.Play();
        }

        IEnumerator HullDamageAnimation() {
            if (damageCoroutine != null) StopCoroutine(damageCoroutine);
            StartCoroutine(GameFeel.ShakeScreen(Camera.main, 0.15f, 0.05f));
            shipFlash.SetActive(true);
            yield return new WaitForSeconds(hitRecoveryTime);
            shipFlash.SetActive(false);

            yield return null;
        }

        IEnumerator DeathAnimation() {
            if (damageCoroutine != null) StopCoroutine(damageCoroutine);
            StartCoroutine(GameFeel.ShakeScreen(Camera.main, 0.3f, 0.2f));
            // TODO: PLAY PRELIMINARY DEATH SOUND
            ship.SetActive(false);
            yield return HullDamageAnimation();
            BreakShipApart();
            // TODO: PLAY DEATH SOUND
            // TODO: INSTANTIATE PARTICLE EFFECT
            splosion = Object.Instantiate(playerExplosion, transform.position, new Quaternion(0f,0f,0f,0f));
            splosion.GetComponent<Rigidbody2D>().velocity = rb.velocity * 0.25f;
            yield return new WaitForSeconds(6f);
            Destroy(splosion);
            Destroy(gameObject);
        }
    }
}


