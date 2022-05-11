using System.Collections;
using UnityEngine;
using Core;
using Damage;
using Game;
using Audio;
using UI;
using Event;
using UnityEngine.InputSystem;

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
        [SerializeField] ParticleSystem shieldLostEffect;
        [SerializeField] ParticleSystem shieldRechargeEffect;
        [SerializeField] GameObject playerExplosion;
        [SerializeField] GameObject ship;
        [SerializeField] GameObject shipFlash;
        [SerializeField] GameObject[] shipParts;

        [Header("Audio")][Space]
        [SerializeField] Sound damageSound;
        [SerializeField] Sound deathSound;
        [SerializeField] LoopableSound shieldSound;
        [SerializeField] Sound shieldLostSound;
        [SerializeField] LoopableSound shieldAlarmSound;
        [SerializeField] Sound shieldRechargeSound;

        [Header("Events")]
        [Space]
        [SerializeField] EventChannelSO eventChannel;
        [SerializeField] GameStateSO gameState;

        [Header("UI")]
        [Space]
        [SerializeField] PlayerUI playerUI;
        [SerializeField] GameObject pointsLostPrefab;

        // components
        CircleCollider2D col;
        Rigidbody2D rb;
        Animator shipFlashEffect;
        PlayerInputHandler inputHandler;
        PlayerMovement movement;
        PlayerInput input;

        // cached
        GameObject splosion;
        Coroutine shakeGamepadCoroutine;
        Coroutine damageCoroutine;
        Coroutine deathCoroutine;
        Timer nukeShockwaveTimer = new Timer(TimerDirection.Decrement, TimerStep.DeltaTime, 1f);

        void OnEnable() {
            eventChannel.OnPlayerTakeHealth.Subscribe(HandleHealthPickup);
        }

        void OnDisable() {
            eventChannel.OnPlayerTakeHealth.Unsubscribe(HandleHealthPickup);
        }

        void Start() {
            AppIntegrity.AssertPresent<ParticleSystem>(shieldEffect);
            AppIntegrity.AssertPresent<GameObject>(playerExplosion);
            AppIntegrity.AssertPresent<GameObject>(ship);
            AppIntegrity.AssertPresent<GameObject>(shipFlash);
            col = Utils.GetRequiredComponent<CircleCollider2D>(gameObject);
            rb = Utils.GetRequiredComponent<Rigidbody2D>(gameObject);
            shipFlashEffect = Utils.GetRequiredComponent<Animator>(shipFlash);

            inputHandler = GetComponent<PlayerInputHandler>();
            movement = GetComponent<PlayerMovement>();
            input = GetComponent<PlayerInput>();

            // init
            ResetHealth();
            SetColliders();
            RegisterHealthCallbacks(OnDeath, OnHealthDamaged, Utils.__NOOP__);
            RegisterShieldCallbacks(OnShieldDepleted, OnShieldDamage, OnShieldDrain, OnShieldRechargeStart, OnShieldRechargeComplete);
            damageSound.Init(this);
            deathSound.Init(this);
            shieldSound.Init(this);
            shieldLostSound.Init(this);
            shieldAlarmSound.Init(this);
            shieldRechargeSound.Init(this);
            GameFeel.ResetGamepadShake();
            if (playerUI != null) Instantiate(playerUI, Vector3.zero, Quaternion.identity, transform);
        }

        void OnDestroy() {
            GameFeel.ResetGamepadShake();    
        }

        void Update() {
            HandleShields();
            TickHealth();
            nukeShockwaveTimer.Tick();

            // move the splosion along the same trajectory as the player's previous heading
            if (splosion != null) splosion.GetComponent<Rigidbody2D>().velocity = rb.velocity * 0.25f;
        }

        void HandleShields() {
            if (!isAlive) return;
            if (shield > 0f && timeHit > 0f) {
                if (!shieldEffect.isPlaying) shieldEffect.Play();
                shieldSound.Play();
            } else {
                shieldEffect.Stop();
                shieldSound.Stop();
            }

            if (shield <= 0f) {
                shieldAlarmSound.Play();
            } else {
                shieldAlarmSound.Stop();
            }

            // if (isRechargingShield) {
            //     shieldRechargeSound.Play();
            // } else {
            //     shieldRechargeSound.Stop();
            // }
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (other.tag == UTag.NukeShockwave) {
                TriggerShockwaveBlast(other.transform.position);
                return;
            }
            if (other.tag == UTag.Asteroid) {
                // ignore - will deal with asteroid in OnCollisionEnter2D
                return;
            }
            if (other.tag == UTag.Mine) {
                // ignore - can go through mines
                return;
            }
            DamageReceiver actor = other.GetComponent<DamageReceiver>();
            if (actor == null) return;
            if (actor.rigidbody == null) return;
            if (!actor.canCollide) return;
            float collisionDamage = GameManager.current.GetDamageClass(DamageType.Collision).baseDamage;
            float collisionMagnitude = (actor.rigidbody.velocity.magnitude + rb.velocity.magnitude);
            actor.TakeDamage(collisionDamage * collisionMagnitude, DamageType.Collision, true);
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

        // void OnCollisionEnter2D(Collision2D other) {
        //     DamageReceiver actor = other.gameObject.GetComponent<DamageReceiver>();
        //     if (actor == null) return;
        //     if (actor.rigidbody == null) return;
        //     float collisionDamage = GameManager.current.GetDamageClass(DamageType.Collision).baseDamage;
        //     float collisionMagnitude = (actor.rigidbody.velocity.magnitude + rb.velocity.magnitude);
        //     float damageToPlayer = collisionDamage * collisionMagnitude * (actor.rigidbody.mass / rb.mass);
        //     float damageToActor = collisionDamage * collisionMagnitude * (rb.mass / actor.rigidbody.mass);
        //     actor.TakeDamage(collisionDamage * collisionMagnitude, DamageType.Collision);
        //     this.TakeDamage(collisionDamage * collisionMagnitude, DamageType.Collision);
        // }

        void TriggerShockwaveBlast(Vector3 shockwaveOrigin) {
            if (nukeShockwaveTimer.active) return;
            nukeShockwaveTimer.Start();
            Vector3 dir = transform.position - shockwaveOrigin;
            float force = blastThrowback * (4f / (dir.magnitude + 4f));
            rb.AddForce(dir.normalized * force, ForceMode2D.Impulse);
            foreach (GameObject part in shipParts) {
                Rigidbody2D rbPart = part.GetComponent<Rigidbody2D>();
                rbPart.AddForce(dir.normalized * force, ForceMode2D.Impulse);
            }
        }

        void OnHealthDamaged(float amount, DamageType damageType, bool isDamageByPlayer) {
            if (shakeGamepadCoroutine != null) StopCoroutine(shakeGamepadCoroutine);
            shakeGamepadCoroutine = StartCoroutine(GameFeel.ShakeGamepad(0.1f, 1f, 1f));
            StartCoroutine(GameFeel.PauseTime(hitPauseDuration, hitPauseTimescale));
            damageCoroutine = StartCoroutine(HullDamageAnimation());
            
            // TODO: PLAY DIFFERENT SOUNDS PER DAMAGE TYPE
            damageSound.Play();
        }

        void OnShieldDepleted() {
            shieldLostSound.Play();
            shieldLostEffect.Stop();
            shieldLostEffect.Play();
        }
        void OnShieldDamage(float amount) {
            shieldRechargeSound.Stop();
            if (shakeGamepadCoroutine != null) StopCoroutine(shakeGamepadCoroutine);
            shakeGamepadCoroutine = StartCoroutine(GameFeel.ShakeGamepad(0.1f, 0.5f, 0.5f));
            StartCoroutine(GameFeel.PauseTime(hitPauseDuration, hitPauseTimescale));
        }
        void OnShieldDrain(float _amount) {
                shieldRechargeSound.Stop();
                shieldRechargeEffect.Stop();
        }
        void OnShieldRechargeStart() {
                shieldRechargeSound.Play();
                shieldRechargeEffect.Stop();
                shieldRechargeEffect.Play();
        }
        void OnShieldRechargeComplete() {
                shieldRechargeSound.Stop();
                shieldRechargeEffect.Stop();
        }

        void HandleHealthPickup(float value) {
            TakeHealth(value);
        }

        void BreakShipApart() {
            GameObject go = new GameObject("BrokenShipParts");
            foreach (GameObject part in shipParts) {
                part.transform.SetParent(go.transform);
                Rigidbody2D rbPart = part.GetComponent<Rigidbody2D>();
                rbPart.simulated = true;
                rbPart.freezeRotation = false;
                // TODO: ADD A LIL'MO RANDOMNESS
                Vector2 direction = (part.transform.position - transform.position).normalized;
                rbPart.velocity = rb.velocity;
                rbPart.AddForce(direction * deathExplosiveForce + Utils.RandomVector2() * 0.5f, ForceMode2D.Impulse);
                rbPart.AddTorque(UnityEngine.Random.Range(-deathPartTorque, deathPartTorque));
            }
        }

        void OnDeath(DamageType damageType, bool isDamageByPlayer) {
            if (shakeGamepadCoroutine != null) StopCoroutine(shakeGamepadCoroutine);
            shakeGamepadCoroutine = StartCoroutine(GameFeel.ShakeGamepad(1.2f, 1f, 1f));
            deathCoroutine = StartCoroutine(DeathAnimation());
            deathSound.Play();
            shieldEffect.Stop();
            shieldRechargeEffect.Stop();
            shieldSound.Stop();
            shieldAlarmSound.Stop();
            shieldRechargeSound.Stop();

            // Destroy the input handler so that the newly-spawned item will gain control (hopefully)
            if (movement != null) { movement.enabled = false; Destroy(movement); }
            if (input != null) { input.enabled = false; Destroy(input); }
            if (inputHandler != null) { inputHandler.enabled = false; Destroy(inputHandler); }

            // set tag to non-player so that PlayerUtils.FindPlayer can find the player once it respawns
            foreach (var obj in GameObject.FindGameObjectsWithTag(UTag.Player)) { obj.tag = UTag.Untagged; }

            SpawnPointsToast();
            eventChannel.OnPlayerDeath.Invoke();
        }

        void SpawnPointsToast() {
            if (pointsLostPrefab == null) return;
            GameObject instance = Instantiate(pointsLostPrefab, transform.position, Quaternion.identity);
            PointsToast toast = instance.GetComponent<PointsToast>();
            toast.SetPoints(gameState.GetPointsToLose());
        }

        IEnumerator HullDamageAnimation() {
            if (damageCoroutine != null) StopCoroutine(damageCoroutine);
            StartCoroutine(GameFeel.ShakeScreen(Utils.GetCamera(), 0.15f, 0.05f));
            shipFlash.SetActive(true);
            yield return new WaitForSeconds(hitRecoveryTime);
            shipFlash.SetActive(false);

            yield return null;
        }

        IEnumerator DeathAnimation() {
            if (damageCoroutine != null) StopCoroutine(damageCoroutine);
            StartCoroutine(GameFeel.ShakeScreen(Utils.GetCamera(), 0.3f, 0.2f));
            ship.SetActive(false);
            yield return HullDamageAnimation();
            BreakShipApart();
            splosion = Object.Instantiate(playerExplosion, transform.position, new Quaternion(0f,0f,0f,0f));
            splosion.GetComponent<Rigidbody2D>().velocity = rb.velocity * 0.25f;
            yield return new WaitForSeconds(6f);
            Destroy(splosion);
            Destroy(gameObject);
        }
    }
}
