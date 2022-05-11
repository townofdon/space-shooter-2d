using UnityEngine;

using Audio;
using Player;
using System.Collections;
using Core;
using Weapons;

namespace Enemies {

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
        EnemyShip enemy;
        CircleCollider2D disruptorRingCollider;
        float disruptorRingRadius = 1f;
        Coroutine iDeactivateDisruptorRing;
        Timer disruptorHold = new Timer(TimerDirection.Increment, TimerStep.DeltaTime, 0.6f);

        // state
        bool didFire = false;
        Coroutine ieShootAtPlayer;

        public void Fire() {
            didFire = true;
            disruptorHold.Start();
            ActivateDisruptorRing();
            if (beamLaser != null) beamLaser.Fire();
        }

        public void LockTarget() {
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
        }

        void Update() {
            if (player == null || !player.isAlive) player = PlayerUtils.FindPlayer();
            HandleDisruptorRing();
            disruptorHold.Tick();
        }

        void OnDeath() {
            EntitySpawner[] spawners = transform.parent.GetComponentsInChildren<EntitySpawner>();
            foreach (var spawner in spawners) {
                spawner.KillCurrent();
                spawner.Disable();
            }
        }

        void FixedUpdate() {
            HandleCirclecast();
        }

        void HandleCirclecast() {
            if (enemy == null || !enemy.isAlive) return;
            Collider2D[] otherColliders = Physics2D.OverlapCircleAll(transform.position, disruptorRingRadius);
            foreach (var other in otherColliders) {
                if (other.tag != UTag.Mine && other.tag != UTag.Missile && other.tag != UTag.Nuke && other.tag != UTag.Asteroid) return;
                disruptorHold.Start();
                StartCoroutine(IActivateDisruptorRingLate());
            }
        }

        void HandleDisruptorRing() {
            if (enemy == null || !enemy.isAlive) {
                DeactivateDisruptorRing();
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
            disruptorSound.Play();
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
            while (true) {
                didFire = false;
                yield return new WaitForSeconds(isAgro() ? fireIntervalAgro : fireInterval);
                enemy.SetInvulnerable(false);
                if (anim != null) {
                    anim.SetTrigger("ChargeUp");
                    anim.SetFloat("TimeMultiplier", isAgro() ? agroFireSpeedUp : 1f);
                }
                while (!didFire) yield return null;
            }
        }

        bool isAgro() {
            if (enemy == null || !enemy.isAlive) return false;
            return enemy.healthPct < agroThreshold;
        }
    }
}

