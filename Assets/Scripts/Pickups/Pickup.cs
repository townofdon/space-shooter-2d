using System.Collections;
using Audio;
using Core;
using Event;
using UnityEngine;
using Weapons;

namespace Pickups {

    public enum PickupType {
        Health,
        Money,
        Ammo,
    }

    public class Pickup : MonoBehaviour {
        [SerializeField] bool debug = false;

        [Header("Pickup Settings")]
        [Space]
        [SerializeField] PickupType pickupType;
        [SerializeField] WeaponType weaponType;
        [SerializeField] float value;
        [SerializeField][Range(0f, 1080f)] float startRotation = 250f;
        [SerializeField][Range(0f, 40f)][Tooltip("Set 0 for infinite lifetime")] float lifetime = 0f;
        [SerializeField][Range(1f, 10f)] float nearRemoveTime = 3f;
        [SerializeField][Range(0.05f, 1f)] float blinkRate = 0.4f;

        [Header("Scroll Down Screen")]
        [Space]
        [SerializeField] bool scrollDownScreen;
        [SerializeField][Range(0f, 20f)] float scrollSpeed = 1f;
        [SerializeField][Range(0f, 20f)] float scrollAccel = 1f;

        [Header("Disperse")]
        [Space]
        [SerializeField] bool disperseAtStart;
        [SerializeField][Range(0f, 20f)] float disperseMin = 2f;
        [SerializeField][Range(0f, 20f)] float disperseMax = 10f;

        [Header("Audio")]
        [Space]
        [SerializeField] Sound pickupSound;

        [Header("Events")]
        [Space]
        [SerializeField] EventChannelSO eventChannel;

        // components
        SpriteRenderer sr;
        TrailRenderer tr;
        Rigidbody2D rb;
        Animator anim;

        // state
        bool isRemoved = false;
        bool isPickedUp = false;
        Timer countdown = new Timer();
        Coroutine blinking;
        Vector2 velocity;

        void Start() {
            anim = GetComponentInChildren<Animator>();
            sr = GetComponentInChildren<SpriteRenderer>();
            tr = GetComponentInChildren<TrailRenderer>();
            rb = GetComponent<Rigidbody2D>();
            rb.angularVelocity = (Utils.RandomBool() ? 1 : -1) * UnityEngine.Random.Range(startRotation / 2f, startRotation);
            pickupSound.Init(this);
            countdown.SetDuration(lifetime);
            countdown.Start();
            HandleDisperse();
        }

        void Update() {
            HandleCountdown();
        }

        void FixedUpdate() {
            HandleScrollDownScreen();
        }

        void HandleCountdown() {
            if (lifetime <= 0f) return;
            if (isRemoved) return;
            if (isPickedUp) return;
            countdown.Tick();
            Debug.Log(countdown.timeLeft);
            if (countdown.timeLeft < nearRemoveTime && blinking == null) {
                blinking = StartCoroutine(IBlink());
            }
            if (countdown.tEnd) {
                Remove();
            }
        }

        void HandleDisperse() {
            if (!disperseAtStart) return;
            if (isRemoved) return;
            if (isPickedUp) return;
            if (rb == null) return;
            rb.velocity = rb.velocity + UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(disperseMin, disperseMax);
        }

        void HandleScrollDownScreen() {
            if (!scrollDownScreen) return;
            if (isRemoved) return;
            if (isPickedUp) return;
            if (rb == null) return;
            if (rb.velocity.magnitude < scrollSpeed) {
                rb.velocity += Vector2.down * scrollAccel * Time.fixedDeltaTime;
            }
            if (debug) Debug.Log(rb.velocity.magnitude);
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (isRemoved) return;
            if (isPickedUp) return;
            if (other.tag == UTag.Player) {
                OnPickup();
            }
        }

        void Remove() {
            if (isRemoved) return;
            if (isPickedUp) return;
            isRemoved = true;
            Destroy(gameObject);
        }

        void OnPickup() {
            if (isRemoved) return;
            if (isPickedUp) return;
            isPickedUp = true;
            StartCoroutine(IOnPickup());
            switch (pickupType) {
                case PickupType.Health:
                    eventChannel.OnPlayerTakeHealth.Invoke(value);
                    break;
                case PickupType.Ammo:
                    eventChannel.OnTakeAmmo.Invoke(weaponType, (int)value);
                    break;
                case PickupType.Money:
                    eventChannel.OnPlayerTakeMoney.Invoke(value);
                    break;
                default:
                    break;
            }
        }

        IEnumerator IOnPickup() {
            if (sr != null) sr.enabled = false;
            if (tr != null) tr.enabled = false;
            pickupSound.Play();
            while (pickupSound.isPlaying) yield return null;
            // if (onItemPickup != null) onItemPickup.Raise(type, value);
            Destroy(gameObject);
            // disable sprite
            // disable trail
            // invoke OnItemPickup event
            // play sound
            yield return null;
        }

        IEnumerator IBlink() {
            if (anim != null) anim.speed = 0.1f;
            while (true && sr != null && !isRemoved && !isPickedUp) {
                sr.enabled = false;
                yield return new WaitForSecondsRealtime(blinkRate);
                sr.enabled = true;
                yield return new WaitForSeconds(blinkRate);
            }
        }
    }
}

