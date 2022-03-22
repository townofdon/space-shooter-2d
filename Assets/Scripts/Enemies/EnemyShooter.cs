using System.Collections.Generic;
using System.Collections;
using UnityEngine;

using Core;
using Weapons;
using Damage;
using Player;

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
        [SerializeField] List<Transform> guns = new List<Transform>();
        [SerializeField][Range(0f, 3f)] float aimSpeed = 2f;
        [SerializeField][Range(0f, 180f)] float maxAimAngle = 30f;
        [SerializeField][Range(0f, 10f)] float triggerHoldTime = 1f;
        [SerializeField][Range(0f, 10f)] float triggerReleaseTime = 1f;
        [SerializeField][Range(0f, 10f)] float triggerTimeVariance = 0f;
        [SerializeField] LayerMask targetableLayers;

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
        float shipRadius = 1f;

        // state - OnlyFireWhenPlayerInLineOfSight
        bool isPlayerInScopes = false;
        bool isAnotherEnemyInTheWay = false;
        Vector3 vectorToPlayer;
        Collider2D overlapHit = null;
        System.Nullable<RaycastHit2D> lineOfSightHit = null;
        RaycastHit2D[] lineOfSightHits = new RaycastHit2D[2];

        void Start() {
            rb = GetComponent<Rigidbody2D>();
            circle = GetComponentInChildren<CircleCollider2D>();
            enemy = Utils.GetRequiredComponent<EnemyShip>(gameObject);
            player = FindObjectOfType<PlayerGeneral>();
            InitWeapon();
            StartCoroutine(PressAndReleaseTrigger());
            // init
            if (circle != null) shipRadius = circle.radius + 1f;
        }

        void InitWeapon() {
            AppIntegrity.AssertPresent<WeaponClass>(weapon);
            weapon = weapon.Clone();
            weapon.Init();
            weapon.shotSound.Init(this);
            weapon.effectSound.Init(this);
        }

        void Update() {
            if (Fire()) {
                AfterFire();
            } else {
                AfterNoFire();
            }
            weapon.TickTimers();
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
            if (player == null) return false;
            // if player is close to ship & within maxAngle
            if (vectorToPlayer.magnitude < shipRadius && Vector2.Angle(transform.rotation * aim * -transform.up, vectorToPlayer) <= maxAimAngle) return true;
            // if player is outside of ship's range
            if (vectorToPlayer.magnitude > weapon.effectiveRange) return false;
            isAnotherEnemyInTheWay = false;
            if (CheckRaycastHit(transform.position + aim * Vector2.down * shipRadius)) return true;
            if (CheckRaycastHit(transform.position + transform.right * 0.5f + aim * Vector2.down * shipRadius)) return true;
            if (CheckRaycastHit(transform.position + transform.right + aim * Vector2.down * shipRadius)) return true;
            if (CheckRaycastHit(transform.position - transform.right * 0.5f + aim * Vector2.down * shipRadius)) return true;
            if (CheckRaycastHit(transform.position - transform.right + aim * Vector2.down * shipRadius)) return true;
            return false;
        }

        bool CheckRaycastHit(Vector3 origin) {
            if (isAnotherEnemyInTheWay) return false;
            int hits = Physics2D.RaycastNonAlloc(origin, aim * Vector2.down, lineOfSightHits, vectorToPlayer.magnitude * 1.5f, targetableLayers);
            for (int i = 0; i < hits; i++)
            {
                if (lineOfSightHits[i].transform.tag == UTag.EnemyShip) { isAnotherEnemyInTheWay = true; return false; }
                if (lineOfSightHits[i].transform.tag == UTag.Player) return true;
            }
            return false;
        }

        void HandleAimBehaviour() {
            if (player == null) return;
            vectorToPlayer = player.transform.position - transform.position;
            switch (aimMode)
            {
                case EnemyAimMode.AimAtPlayer:
                    CalculateAim();
                    transform.rotation = Quaternion.identity;
                    aim = Quaternion.AngleAxis(Mathf.Clamp(aimAngle, -maxAimAngle, maxAimAngle), Vector3.forward);
                    break;
                case EnemyAimMode.RotateTowardsPlayer:
                    CalculateAim();
                    transform.rotation = Quaternion.AngleAxis(aimAngle, Vector3.forward);
                    aim = Quaternion.identity;
                    break;
                case EnemyAimMode.AlwaysAimDown:
                default:
                    transform.rotation = Quaternion.identity;
                    aim = Quaternion.identity;
                    break;
            }
        }

        void CalculateAim() {
            aimVector = Vector2.MoveTowards(aimVector, vectorToPlayer.normalized, aimSpeed * Time.deltaTime);
            // TODO: refactor Vector2.down to `orientation` var
            aimAngle = Vector2.SignedAngle(Vector2.down, aimVector);
        }

        bool Fire() {
            if (!enemy.isAlive) return false;
            if (!Utils.IsObjectOnScreen(gameObject)) return false;
            if (!isPlayerInScopes) return false;
            if (!weapon.ShouldFire(triggerHeld.active)) return false;

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
            GameObject instance = Object.Instantiate(prefab, position, rotation * aim);
            DamageDealer damager = instance.GetComponent<DamageDealer>();
            Collider2D collider = instance.GetComponent<Collider2D>();
            if (collider != null) enemy.IgnoreCollider(instance.GetComponent<Collider2D>());
            if (damager != null) damager.SetIgnoreUUID(enemy.uuid);
            if (rb != null) {
                Rigidbody2D rbInstance = instance.GetComponent<Rigidbody2D>();
                rbInstance.velocity += rb.velocity;
            }
        }

        IEnumerator PressAndReleaseTrigger() {
            while (true) {
                triggerHeld.SetDuration(Mathf.Max(triggerHoldTime + UnityEngine.Random.Range(-triggerTimeVariance / 2f, triggerTimeVariance / 2f), 0.1f));
                triggerReleased.SetDuration(Mathf.Max(triggerReleaseTime + UnityEngine.Random.Range(-triggerTimeVariance / 2f, triggerTimeVariance / 2f), 0.1f));
                yield return triggerHeld.StartAndWaitUntilFinished(true);
                yield return triggerReleased.StartAndWaitUntilFinished(true);
                while (!isPlayerInScopes) yield return null;
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
