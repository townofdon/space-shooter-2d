using System.Collections.Generic;
using UnityEngine;

using Audio;
using Player;
using System.Collections;
using Core;
using Weapons;
using Game;

namespace Enemies {

    public enum BossEyeMovementMode {
        Normal,
        ScreenBounce,
    }

    public class BossBeamLaserShooter : MonoBehaviour {
        [Header("Behaviour")]
        [Space]
        [SerializeField] bool attackOnStart;
        [SerializeField] bool invulnerableAtStart;
        [SerializeField][Range(0f, 10f)] float delayAtStart = 0.5f;
        [SerializeField][Range(0f, 1f)] float agroThreshold = 0.3f;
        [SerializeField][Range(1f, 5f)] float agroFireSpeedUp = 2f;
        [SerializeField][Range(0f, 10f)] float fireInterval = 3f;
        [SerializeField][Range(0f, 5f)] float fireIntervalAgro = 1f;
        [SerializeField][Range(0f, 50f)] float flingForce = 5f;
        [SerializeField][Range(0f, 50f)] float disruptorHoldTime = 0.6f;
        [SerializeField][Range(0f, 50f)] float disruptorHoldTimeAgro = 0.8f;

        [Header("Difficulty Settings")]
        [Space]
        [SerializeField][Range(0f, 5f)] float fireIntervalEasyMod = 1f;
        [SerializeField][Range(0f, 5f)] float fireIntervalMediumMod = 1f;
        [SerializeField][Range(0f, 5f)] float fireIntervalHardMod = 1f;
        [SerializeField][Range(0f, 5f)] float fireIntervalInsaneMod = 1f;
        [Space]
        [SerializeField][Range(0f, 5f)] float maxSpeedEasyMod = 1f;
        [SerializeField][Range(0f, 5f)] float maxSpeedMediumMod = 1f;
        [SerializeField][Range(0f, 5f)] float maxSpeedHardMod = 1f;
        [SerializeField][Range(0f, 5f)] float maxSpeedInsaneMod = 1f;

        [Header("Agro")]
        [Space]
        [SerializeField] bool moveOnAgro;
        [SerializeField] GameObject agroSplosion;
        [SerializeField] List<Transform> agroSplosionLocations = new List<Transform>();
        [SerializeField] float agroDelay = 1f;
        [SerializeField] Sound agroSound;

        [Header("Movement")]
        [Space]
        [SerializeField][Range(0f, 360f)] float moveAngle = 15f;
        [SerializeField][Range(0f, 20f)] float maxMoveSpeed = 10f;
        [SerializeField][Range(0f, 10f)] float throttleUpTime = 3f;
        [SerializeField][Range(0f, 10f)] float throttleDownTime = 3f;
        [SerializeField] AnimationCurve moveCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
        [SerializeField] float screenPadTop = 2f;
        [SerializeField] float screenPadBottom = 4f;
        [SerializeField] float screenPadLeft = 2f;
        [SerializeField] float screenPadRight = 2f;

        [Header("Components")]
        [Space]
        [SerializeField] GameObject disruptorRing;
        [SerializeField] BeamLaser beamLaser;
        [SerializeField] Animator anim;

        [Header("Audio")]
        [Space]
        [SerializeField] LoopableSound disruptorSound;

        // cached
        PlayerGeneral player;
        Rigidbody2D rbOther;
        EnemyShip enemy;
        CircleCollider2D disruptorRingCollider;
        float disruptorRingRadius = 1f;
        Coroutine iDeactivateDisruptorRing;
        Timer disruptorHold = new Timer(TimerDirection.Increment, TimerStep.DeltaTime, 0.6f);
        Vector2 minBounds;
        Vector2 maxBounds;

        // state
        bool didFire = false;
        Coroutine ieShootAtPlayer;

        // move state
        bool isAgroStarting;
        bool isOffscreen;
        Coroutine iAgroMove;
        float moveSpeed = 0f;
        Vector3 moveHeading;
        Timer throttleUp = new Timer(TimerDirection.Increment);
        Timer throttleDown = new Timer(TimerDirection.Increment);

        public void Fire() {
            didFire = true;
            disruptorHold.SetDuration(IsAgro() ? disruptorHoldTimeAgro : disruptorHoldTime);
            disruptorHold.Start();
            ActivateDisruptorRing();
            if (beamLaser != null && beamLaser.isActiveAndEnabled) beamLaser.Fire();
            StartCoroutine(GameFeel.ShakeScreen(Utils.GetCamera(), 0.5f, 0.2f));
        }

        public void LockTarget() {
            if (beamLaser == null || !beamLaser.isActiveAndEnabled) return;
            beamLaser.SetLocked(true);
            beamLaser.ChargeUp();
        }

        public void StartAttacking() {
            if (ieShootAtPlayer == null) ieShootAtPlayer = StartCoroutine(IShootAtPlayer());
        }

        void OnEnable() {
            enemy.OnDeathEvent += OnDeath;
        }

        void OnDisable() {
            enemy.OnDeathEvent -= OnDeath;
        }

        void Awake() {
            enemy = GetComponent<EnemyShip>();
        }

        void Start() {
            if (disruptorRing != null) {
                disruptorRingCollider = disruptorRing.GetComponent<CircleCollider2D>();
                if (disruptorRingCollider != null) disruptorRingRadius = disruptorRingCollider.radius;
            }
            player = PlayerUtils.FindPlayer();
            disruptorSound.Init(this);
            if (invulnerableAtStart && enemy != null) enemy.SetInvulnerable(true);
            if (attackOnStart && ieShootAtPlayer == null) ieShootAtPlayer = StartCoroutine(IShootAtPlayer());
            (minBounds, maxBounds) = Utils.GetScreenBounds(Utils.GetCamera(), 0, true);
            SetInitialHeading();
            agroSound.Init(this);
        }

        void Update() {
            if (player == null || !player.isAlive) player = PlayerUtils.FindPlayer();
            HandleDisruptorRing();
            HandleOffscreen();
            disruptorHold.Tick();
            if (moveOnAgro && iAgroMove == null && IsAgro()) iAgroMove = StartCoroutine(IAgro());
        }

        void OnDeath() {
            if (transform.parent != null) {
                EntitySpawner[] spawners = transform.parent.GetComponentsInChildren<EntitySpawner>();
                foreach (var spawner in spawners) {
                    spawner.KillCurrent();
                    spawner.Disable();
                }
            }
        }

        void FixedUpdate() {
            HandleMove();
            HandleScreenReflect();
            HandleCirclecast();
        }

        void HandleOffscreen() {
            isOffscreen = GetIsOffscreen();
            if (isOffscreen) didFire = true;
        }

        void HandleCirclecast() {
            if (enemy == null || !enemy.isAlive) return;
            Collider2D[] otherColliders = Physics2D.OverlapCircleAll(transform.position, disruptorRingRadius);
            foreach (var other in otherColliders) {
                if (other.tag != UTag.Mine && other.tag != UTag.Missile && other.tag != UTag.Nuke && other.tag != UTag.Asteroid) return;
                rbOther = other.GetComponent<Rigidbody2D>();
                if (rbOther != null) {
                    rbOther.AddForce((transform.position - other.transform.position) * flingForce, ForceMode2D.Impulse);
                }
                disruptorHold.SetDuration(IsAgro() ? disruptorHoldTimeAgro : disruptorHoldTime);
                disruptorHold.Start();
                StartCoroutine(IActivateDisruptorRingLate());
            }
        }

        void HandleDisruptorRing() {
            if (enemy == null || !enemy.isAlive) {
                DeactivateDisruptorRing();
                return;
            }
            if (isAgroStarting) {
                ActivateDisruptorRing();
                return;
            }
            if (player == null || !player.isAlive || Vector2.Distance(player.transform.position, transform.position) > disruptorRingRadius) {
                DeactivateDisruptorRing();
                return;
            }

            ActivateDisruptorRing();
        }

        void ActivateDisruptorRing() {
            if (iDeactivateDisruptorRing != null) { StopCoroutine(iDeactivateDisruptorRing); iDeactivateDisruptorRing = null; }
            if (disruptorRing != null && !disruptorRing.activeSelf) disruptorRing.SetActive(true);
            if (disruptorRing != null && disruptorRing.activeInHierarchy) disruptorSound.Play();
        }

        void DeactivateDisruptorRing() {
            if (iDeactivateDisruptorRing != null) return;
            if (disruptorRing == null || !disruptorRing.activeSelf) return;
            iDeactivateDisruptorRing = StartCoroutine(IDeactivateDisruptorRing());
        }

        IEnumerator IActivateDisruptorRingLate() {
            yield return new WaitForSeconds(0.1f);
            ActivateDisruptorRing();
        }

        IEnumerator IDeactivateDisruptorRing() {
            yield return new WaitForSeconds(0.4f);
            while (disruptorHold.active) yield return null;
            if (disruptorRing != null) disruptorRing.SetActive(false);
            disruptorSound.Stop();
            iDeactivateDisruptorRing = null;
        }

        IEnumerator IShootAtPlayer() {
            yield return new WaitForSeconds(delayAtStart);
            while (IsAlive()) {
                didFire = false;
                yield return new WaitForSeconds(GetFireInterval());
                while (isAgroStarting || isOffscreen) yield return null;
                enemy.SetInvulnerable(false);
                if (anim != null && anim.enabled) {
                    anim.SetTrigger("ChargeUp");
                    anim.SetFloat("TimeMultiplier", IsAgro() ? agroFireSpeedUp : 1f);
                }
                while (!didFire) yield return null;
            }
        }

        void HandleMove() {
            if (!IsAlive()) return;
            if (!IsAgro()) return;
            if (!moveOnAgro) return;
            transform.position = transform.position + moveHeading * moveSpeed * Time.fixedDeltaTime;
        }

        void HandleScreenReflect() {
            if (!IsAlive()) return;
            if (!IsAgro()) return;
            if (!moveOnAgro) return;
            if (transform.position.x < minBounds.x + screenPadLeft) {
                moveHeading = Vector2.Reflect(moveHeading, Vector2.right);
            } else if (transform.position.x > maxBounds.x - screenPadRight) {
                moveHeading = Vector2.Reflect(moveHeading, Vector2.left);
            }
            if (transform.position.y < minBounds.y + screenPadBottom) {
                moveHeading = Vector2.Reflect(moveHeading, Vector2.up);
            } else if (transform.position.y > maxBounds.y - screenPadTop) {
                moveHeading = Vector2.Reflect(moveHeading, Vector2.down);
            }
            transform.position = new Vector2(
                Mathf.Clamp(transform.position.x, minBounds.x + screenPadLeft, maxBounds.x - screenPadRight),
                Mathf.Clamp(transform.position.y, minBounds.y + screenPadBottom, maxBounds.y - screenPadTop)
            );
        }

        IEnumerator IAgro() {
            enemy.SetInvulnerable(true);
            isAgroStarting = true;
            yield return new WaitForSeconds(agroDelay);
            enemy.SetInvulnerable(false);
            isAgroStarting = false;
            foreach (Transform location in agroSplosionLocations) {
                Instantiate(agroSplosion, location.position, Quaternion.identity);
            }
            while (IsAlive()) {
                throttleUp.SetDuration(throttleUpTime);
                throttleUp.Start();
                while (throttleUp.active) {
                    SetMoveSpeed(throttleUp.value / 2f);
                    throttleUp.Tick();
                    yield return null;
                }

                throttleDown.SetDuration(throttleDownTime);
                throttleDown.Start();
                while (throttleDown.active) {
                    SetMoveSpeed(0.5f + throttleDown.value / 2f);
                    throttleDown.Tick();
                    yield return null;
                }

                yield return null;
            }
        }

        bool IsAlive() {
            return enemy != null && enemy.isAlive;
        }

        bool IsAgro() {
            if (enemy == null || !enemy.isAlive) return false;
            return enemy.healthPct < agroThreshold;
        }

        float GetFireInterval() {
            return (IsAgro() ? fireIntervalAgro : fireInterval) * GetFireIntervalMod();
        }

        float GetFireIntervalMod() {
            switch (GameManager.current.difficulty) {
                case GameDifficulty.Easy:
                    return fireIntervalEasyMod;
                case GameDifficulty.Medium:
                    return fireIntervalMediumMod;
                case GameDifficulty.Hard:
                    return fireIntervalHardMod;
                case GameDifficulty.Insane:
                    return fireIntervalInsaneMod;
                default:
                    return 1f;
            }
        }

        float GetMaxMoveSpeedMod() {
            switch (GameManager.current.difficulty) {
                case GameDifficulty.Easy:
                    return maxSpeedEasyMod;
                case GameDifficulty.Medium:
                    return maxSpeedMediumMod;
                case GameDifficulty.Hard:
                    return maxSpeedHardMod;
                case GameDifficulty.Insane:
                    return maxSpeedInsaneMod;
                default:
                    return 1f;
            }
        }

        void SetInitialHeading() {
            moveHeading = Utils.RandomBool() ? Quaternion.AngleAxis(moveAngle, Vector3.forward) * Vector3.left : Quaternion.AngleAxis(moveAngle, -Vector3.forward) * Vector3.right;
        }

        void SetMoveSpeed(float t) {
            moveSpeed = maxMoveSpeed * moveCurve.Evaluate(t) * GetMaxMoveSpeedMod();
        }

        bool GetIsOffscreen() {
            if (transform.position.x < minBounds.x) return true;
            if (transform.position.x > maxBounds.x) return true;
            if (transform.position.y < minBounds.y) return true;
            if (transform.position.y > maxBounds.y) return true;
            return false;
        }
    }
}

