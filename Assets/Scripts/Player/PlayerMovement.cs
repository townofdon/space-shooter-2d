using UnityEngine;

using Core;
using Audio;
using Game;
using Event;

namespace Player {

    [RequireComponent(typeof(PlayerInputHandler))]

    public class PlayerMovement : MonoBehaviour
    {
        [Header("General")]
        [Space]
        [SerializeField] PlayerStateSO playerState;

        [Header("Main Movement Control")][Space]
        [SerializeField] float maxSpeed = 10f;
        [SerializeField] float thrust = 100f;
        [SerializeField] float decelMultiplier = 2f;
        [SerializeField] float throttleUpTime = 0.75f;
        [SerializeField] float throttleDownTime = 2f;

        [Header("Aiming / Rotation")][Space]
        [SerializeField] float aimMaxAngle = 30f;
        [SerializeField] float aimSpeed = 1f;

        [Header("Boost")][Space]
        [SerializeField] float boostThrust = 100f;
        [SerializeField] float boostCooldownTime = 5f;
        [SerializeField] float boostDragMultiplier = 5f;
        [SerializeField] GameObject boostWaves;

        [Header("Bounds")][Space]
        [SerializeField] float screenPadLeft = 0f;
        [SerializeField] float screenPadRight = 0f;
        [SerializeField] float screenPadTop = 0f;
        [SerializeField] float screenPadBottom = 0f;

        [Header("Audio")][Space]
        [SerializeField] LoopableSound thrustSound;
        [SerializeField] Sound boostSound;

        // components
        Rigidbody2D rb;
        PlayerInputHandler input;
        PlayerGeneral player;

        // cached
        float initialDrag;

        // state - bounds
        Vector2 minBounds;
        Vector2 maxBounds;

        // state - thrust
        bool canMove = true;
        Vector2 currentThrust;
        float throttle = 0f;

        // state - aim
        float currentAngle = 0f;
        Quaternion aim = Quaternion.identity;

        // state - boost
        bool canBoost = true;
        Vector2 currentBoost;
        float boostAvailable = 1f;

        // state - bounds
        Vector2 screenPosition;

        void Start() {
            AppIntegrity.AssertPresent<GameObject>(boostWaves);
            rb = Utils.GetRequiredComponent<Rigidbody2D>(gameObject);
            input = Utils.GetRequiredComponent<PlayerInputHandler>(gameObject);
            player = Utils.GetRequiredComponent<PlayerGeneral>(gameObject);
            initialDrag = rb.drag;
            minBounds = Utils.GetCamera().ViewportToWorldPoint(new Vector2(0f, 0f));
            maxBounds = Utils.GetCamera().ViewportToWorldPoint(new Vector2(1f, 1f));
            thrustSound.Init(this);
            boostSound.Init(this);
        }

        void Update() {
            SetThrottle();
            HandleMoveByGame();
        }

        void FixedUpdate() {
            HandleMove();
            HandleRotate();
            HandleBoost();
            HandleBounds();
        }

        void SetThrottle() {
            bool hasMoveInput = Mathf.Abs(input.move.x) > Mathf.Epsilon || Mathf.Abs(input.move.y) > Mathf.Epsilon;
            if (player.isAlive && hasMoveInput) {
                throttle += Time.deltaTime / throttleUpTime;
                if (playerState.controlMode == PlayerInputControlMode.Player) thrustSound.Play();
            } else {
                throttle -= Time.deltaTime / throttleDownTime;
                thrustSound.Stop();
            }
            throttle = Mathf.Clamp(throttle, 0f, 1f);
        }

        void HandleMoveByGame() {
            if (playerState.controlMode == PlayerInputControlMode.Player) return;
            if (GameManager.isPaused) return;
            if (!player.isAlive || !canMove) return;
            rb.simulated = false;
            rb.isKinematic = true;
            rb.velocity = Vector2.zero;
            transform.position += (Vector3)input.move * throttle * maxSpeed * Time.deltaTime;
        }

        void HandleMove() {
            if (playerState.controlMode == PlayerInputControlMode.GameBrain) return;
            if (GameManager.isPaused) return;
            if (!player.isAlive || !canMove) return;
            rb.simulated = true;
            rb.isKinematic = false;
            currentThrust.x = GetThrustComponent(input.move.x, rb.velocity.x);
            currentThrust.y = GetThrustComponent(input.move.y, rb.velocity.y);
            rb.AddForce(currentThrust);
        }

        void HandleRotate() {
            if (GameManager.isPaused) return;
            if (currentAngle < input.look.x * aimMaxAngle) currentAngle = Mathf.Round(Mathf.Clamp(currentAngle + aimSpeed * Time.deltaTime, -aimMaxAngle, aimMaxAngle));
            if (currentAngle > input.look.x * aimMaxAngle) currentAngle = Mathf.Round(Mathf.Clamp(currentAngle - aimSpeed * Time.deltaTime, -aimMaxAngle, aimMaxAngle));
            aim = Quaternion.AngleAxis(-currentAngle, Vector3.forward);
            transform.rotation = aim;
        }

        void HandleBounds() {
            if (playerState.controlMode == PlayerInputControlMode.GameBrain) return;
            transform.position = new Vector2(
                Mathf.Clamp(transform.position.x, minBounds.x + screenPadLeft, maxBounds.x - screenPadRight),
                Mathf.Clamp(transform.position.y, minBounds.y + screenPadBottom, maxBounds.y - screenPadTop)
            );
        }

        void HandleBoost() {
            if (GameManager.isPaused) return;
            if (player.isAlive && input.isBoostPressed && input.move.magnitude > 0.1f && canBoost) {
                currentBoost = (Vector2.up * 0.01f + input.move).normalized * boostThrust * boostAvailable;
                rb.AddForce(currentBoost, ForceMode2D.Impulse);
                if (rb.velocity.magnitude > maxSpeed && boostAvailable >= 0.7f) {
                    boostSound.Play();
                    CreateBoostWaves();
                }
                boostAvailable = 0f;
            } else {
                boostAvailable += Time.deltaTime / boostCooldownTime;
            }
            boostAvailable = Mathf.Clamp(boostAvailable, 0f, 1f);
            rb.drag = Mathf.Lerp(boostDragMultiplier * initialDrag, initialDrag, boostAvailable);
        }

        void CreateBoostWaves() {
            // Quaternion rotation = Quaternion.LookRotation(-rb.velocity.normalized, Vector3.up);
            Quaternion rotation = Quaternion.FromToRotation(Vector3.right, -rb.velocity.normalized);
            // float angle = Vector2.Angle(Vector2.up, rb.velocity.normalized);
            // Quaternion rotation = new Quaternion(0f,0f,0f,0f);
            // rotation.eulerAngles = new Vector3(0f, 0f, angle);
            GameObject obj = Object.Instantiate(boostWaves, transform.position, rotation);
            obj.transform.position += (Vector3)rb.velocity.normalized * 0.5f;
            obj.GetComponent<Rigidbody2D>().velocity = -rb.velocity * 0.01f;
            Destroy(obj, 5f);
        }

        float GetThrustComponent(float value, float componentVelocity) {
            return value
                * throttle
                * thrust
                * Easing.easeOutQuart(
                    // get diff of value vs. current velocity
                    Mathf.Clamp(Mathf.Abs(value * maxSpeed - Mathf.Clamp(componentVelocity, -maxSpeed, maxSpeed)) / maxSpeed, 0f, decelMultiplier)
                );
        }
    }
}

