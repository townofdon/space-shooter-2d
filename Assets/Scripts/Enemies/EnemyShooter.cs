using System.Collections.Generic;
using System.Collections;
using UnityEngine;

using Core;
using Weapons;
using Damage;
using Player;
using Game;
using Audio;

namespace Enemies
{
    public enum EnemyAimMode {
        AlwaysAimDown,
        AimAtPlayer,
        RotateTowardsPlayer,
    }

    public enum EnemyFiringMode {
        AlwaysFiring,
        OnlyFireWhenPlayerInLineOfSight,
    }

    [RequireComponent(typeof(EnemyShip))]

    public class EnemyShooter : MonoBehaviour
    {
        [SerializeField] bool debug = false;
        [SerializeField] EnemyFiringMode firingMode;
        [SerializeField] EnemyAimMode aimMode;
        [SerializeField] WeaponClass weapon;
        [SerializeField] ParticleSystem muzzleFlashFX;
        [SerializeField] int numUpgrades;
        [SerializeField] List<Transform> guns = new List<Transform>();
        [SerializeField][Range(0f, 3f)] float aimSpeed = 2f;
        [SerializeField][Range(0f, 180f)] float maxAimAngle = 30f;
        [SerializeField][Range(0f, 10f)] float triggerHoldTime = 1f;
        [SerializeField][Range(0f, 10f)] float triggerReleaseTime = 1f;
        [SerializeField][Range(0f, 10f)] float triggerTimeVariance = 0f;
        [SerializeField] Transform rotateTarget;
        [SerializeField] LayerMask targetableLayers;

        [Header("Firing Animation")]
        [Space]
        [SerializeField] bool useFireAnimation = false;
        [SerializeField] Animator anim;

        [Header("Difficulty Settings")]
        [Space]
        [SerializeField][Range(0f, 5f)] float triggerHoldEasyMod = 1f;
        [SerializeField][Range(0f, 5f)] float triggerHoldMediumMod = 1f;
        [SerializeField][Range(0f, 5f)] float triggerHoldHardMod = 1f;
        [SerializeField][Range(0f, 5f)] float triggerHoldInsaneMod = 1f;
        [Space]
        [SerializeField][Range(0f, 5f)] float triggerReleaseEasyMod = 1f;
        [SerializeField][Range(0f, 5f)] float triggerReleaseMediumMod = 1f;
        [SerializeField][Range(0f, 5f)] float triggerReleaseHardMod = 1f;
        [SerializeField][Range(0f, 5f)] float triggerReleaseInsaneMod = 1f;

        [Header("Audio")]
        [SerializeField] Sound chargeUpSound;

        // Components
        Rigidbody2D rb;
        EnemyShip enemy;
        PlayerGeneral player;
        CircleCollider2D circle;

        // state
        Timer triggerHeld = new Timer();
        Timer triggerReleased = new Timer();

        // state - aiming
        Vector2 aimVector = Vector2.down;
        float aimAngle = 0f;
        Quaternion aim = Quaternion.identity;
        Quaternion rot = Quaternion.identity;
        float shipRadius = 1f;

        // state - OnlyFireWhenPlayerInLineOfSight
        bool isPlayerInScopes = false;
        bool isAnotherEnemyInTheWay = false;
        Vector3 vectorToPlayer;
        Collider2D overlapHit = null;
        System.Nullable<RaycastHit2D> lineOfSightHit = null;
        RaycastHit2D[] lineOfSightHits = new RaycastHit2D[2];

        public void UpgradeWeapon() {
            weapon.Upgrade();
        }

        public void Fire() {
            chargeUpSound.Stop();
            ImperativelyFire();
        }

        public void ChargeUp() {
            chargeUpSound.Play();
        }

        void OnEnable() {
            StartCoroutine(PressAndReleaseTrigger());
        }

        void Start() {
            rb = GetComponent<Rigidbody2D>();
            circle = GetComponentInChildren<CircleCollider2D>();
            enemy = Utils.GetRequiredComponent<EnemyShip>(gameObject);
            player = PlayerUtils.FindPlayer();
            chargeUpSound.Init(this);
            InitWeapon();
            // init
            if (circle != null) shipRadius = circle.radius + 1f;
        }

        void InitWeapon() {
            AppIntegrity.AssertPresent<WeaponClass>(weapon);
            weapon = weapon.Clone();
            weapon.Init();
            weapon.shotSound.Init(this);
            weapon.effectSound.Init(this);
            weapon.SetInfiniteAmmo(true);
            for (int i = 0; i < numUpgrades; i++) weapon.Upgrade();
        }

        void Update() {
            if (player == null || !player.isAlive) player = PlayerUtils.FindPlayer();
            if (DidFire()) {
                AfterFire();
            } else {
                AfterNoFire();
            }
            weapon.TickTimers();
            triggerHeld.Tick();
            triggerReleased.Tick();
            HandleFiringBehaviour();
            HandleAimBehaviour();
        }

        void HandleFiringBehaviour() {
            switch (firingMode)
            {
                case EnemyFiringMode.OnlyFireWhenPlayerInLineOfSight:
                    isPlayerInScopes = CheckPlayerWithinScopes();
                    break;
                case EnemyFiringMode.AlwaysFiring:
                default:
                    // this enemy is just plain dumb, or maybe really upset
                    isPlayerInScopes = true;
                    break;
            }
        }

        bool CheckPlayerWithinScopes() {
            if (player == null || !player.isAlive) return false;
            // if player is close to ship & within maxAngle
            if (vectorToPlayer.magnitude < shipRadius && Vector2.Angle(transform.rotation * rot * aim * -transform.up, vectorToPlayer) <= maxAimAngle) return true;
            // if player is outside of ship's range
            if (vectorToPlayer.magnitude > weapon.effectiveRange) return false;
            isAnotherEnemyInTheWay = false;
            if (CheckRaycastHit(transform.position + rot * aim * Vector2.down * shipRadius)) return true;
            if (CheckRaycastHit(transform.position + transform.right * 0.5f + rot * aim * Vector2.down * shipRadius)) return true;
            if (CheckRaycastHit(transform.position + transform.right + rot * aim * Vector2.down * shipRadius)) return true;
            if (CheckRaycastHit(transform.position - transform.right * 0.5f + rot * aim * Vector2.down * shipRadius)) return true;
            if (CheckRaycastHit(transform.position - transform.right + rot * aim * Vector2.down * shipRadius)) return true;
            return false;
        }

        bool CheckRaycastHit(Vector3 origin) {
            if (isAnotherEnemyInTheWay) return false;
            int hits = Physics2D.RaycastNonAlloc(origin, rot * aim * Vector2.down, lineOfSightHits, vectorToPlayer.magnitude * 1.5f, targetableLayers);
            for (int i = 0; i < hits; i++)
            {
                if (lineOfSightHits[i].transform.tag == UTag.EnemyShip) { isAnotherEnemyInTheWay = true; return false; }
                if (lineOfSightHits[i].transform.tag == UTag.Player) return true;
            }
            return false;
        }

        void HandleAimBehaviour() {
            if (player == null || !player.isAlive) return;
            vectorToPlayer = player.transform.position - transform.position;
            switch (aimMode)
            {
                case EnemyAimMode.AimAtPlayer:
                    CalculateAim();
                    rot = Quaternion.identity;
                    aim = Quaternion.AngleAxis(Mathf.Clamp(aimAngle, -maxAimAngle, maxAimAngle), Vector3.forward);
                    HandleRotation();
                    break;
                case EnemyAimMode.RotateTowardsPlayer:
                    CalculateAim();
                    rot = Quaternion.AngleAxis(aimAngle, Vector3.forward);
                    aim = Quaternion.identity;
                    HandleRotation();
                    break;
                case EnemyAimMode.AlwaysAimDown:
                default:
                    rot = Quaternion.identity;
                    aim = Quaternion.identity;
                    HandleRotation();
                    break;
            }
        }

        void HandleRotation() {
            if (rotateTarget != null) {
                rotateTarget.rotation = rot;
            } else {
                transform.rotation = rot;
            }
        }

        void CalculateAim() {
            aimVector = Vector2.MoveTowards(aimVector, vectorToPlayer.normalized, aimSpeed * Time.deltaTime);
            // TODO: refactor Vector2.down to `orientation` var
            aimAngle = Vector2.SignedAngle(Vector2.down, aimVector);
        }

        bool DidFire() {
            if (!enemy.isAlive) return false;
            if (!Utils.IsObjectOnScreen(gameObject)) return false;
            if (!isPlayerInScopes) return false;
            if (!weapon.ShouldFire(triggerHeld.active)) return false;
            if (useFireAnimation && anim != null) {
                if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !anim.GetCurrentAnimatorStateInfo(0).IsTag("Idle")) return false;
                anim.SetTrigger("Fire");
                return true;
            }

            return ImperativelyFire();
        }

        bool ImperativelyFire() {
            if (!enemy.isAlive) return false;
            foreach (var gun in guns) {    
                FireProjectile(weapon.prefab, gun.position, gun.rotation);
                weapon.shotSound.Play();
                weapon.effectSound.Play();
            }
            return true;
        }

        void AfterFire() {
            weapon.AfterFire();
        }

        void AfterNoFire() {
            weapon.AfterNoFire();
            weapon.effectSound.Stop();
        }

        void FireProjectile(GameObject prefab, Vector3 position, Quaternion rotation) {
            if (muzzleFlashFX != null) muzzleFlashFX.Play();
            GameObject instance = Object.Instantiate(prefab, position, rotation * aim);
            Projectile[] projectiles = instance.GetComponentsInChildren<Projectile>();
            foreach (var projectile in projectiles) {
                DamageDealer damager = projectile.GetComponent<DamageDealer>();
                Collider2D collider = projectile.GetComponent<Collider2D>();
                if (collider != null) enemy.IgnoreCollider(projectile.GetComponent<Collider2D>());
                if (damager != null) damager.SetIgnoreUUID(enemy.uuid);
                if (rb != null) {
                    Rigidbody2D rbInstance = projectile.GetComponent<Rigidbody2D>();
                    rbInstance.velocity += rb.velocity;
                }
            }
            Nuke nuke = instance.GetComponent<Nuke>();
            if (nuke != null && player != null && player.isAlive) {
                nuke.SetMoveSpeed(Mathf.Clamp(Vector2.Distance(transform.position, player.transform.position) * 0.75f, 4f, 10f));
            }
        }

        IEnumerator PressAndReleaseTrigger() {
            // simulate human-like response time
            yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(0.2f, 0.6f));
            while (true) {
                while (!isPlayerInScopes) yield return null;
                triggerHeld.SetDuration(Mathf.Max(triggerHoldTime * GetTriggerHoldMod() + GetTriggerVariance(), 0.1f));
                triggerReleased.SetDuration(Mathf.Max(triggerReleaseTime * GetTriggerReleaseMod() + GetTriggerVariance(), triggerReleaseTime / 2f));
                // NOTE!!! - if weapon burst is set, the weapon will keep firing for full burst even after the trigger is released
                yield return triggerHeld.StartAndWaitUntilFinished(true);
                yield return triggerReleased.StartAndWaitUntilFinished(true);
            }
        }

        float GetTriggerVariance() {
            return UnityEngine.Random.Range(-triggerTimeVariance / 2f, triggerTimeVariance / 2f);
        }

        float GetTriggerHoldMod() {
            switch (GameManager.current.difficulty) {
                case GameDifficulty.Easy:
                    return triggerHoldEasyMod;
                case GameDifficulty.Medium:
                    return triggerHoldMediumMod;
                case GameDifficulty.Hard:
                    return triggerHoldHardMod;
                case GameDifficulty.Insane:
                    return triggerHoldInsaneMod;
                default:
                    return 1f;
            }
        }

        float GetTriggerReleaseMod() {
            switch (GameManager.current.difficulty) {
                case GameDifficulty.Easy:
                    return triggerReleaseEasyMod;
                case GameDifficulty.Medium:
                    return triggerReleaseMediumMod;
                case GameDifficulty.Hard:
                    return triggerReleaseHardMod;
                case GameDifficulty.Insane:
                    return triggerReleaseInsaneMod;
                default:
                    return 1f;
            }
        }

        void OnDrawGizmos() {
            if (!debug) return;
            // Gizmos.color = Color.cyan;
            // Gizmos.DrawRay(transform.position, vectorToPlayer);
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(transform.position, transform.rotation * Vector2.down);
            Gizmos.color = weapon.firing ? Color.red : Color.yellow;
            Gizmos.DrawRay(transform.position, aim * Vector2.down);
        }
    }
}
