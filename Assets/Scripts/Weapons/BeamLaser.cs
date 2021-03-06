using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Audio;
using Core;
using Player;
using Damage;
using Game;

namespace Weapons {

    public class BeamLaser : MonoBehaviour {
        [Header("BeamLaser")]
        [Space]
        [SerializeField][Range(0, 10f)] float aimSpeed = 1f;
        [SerializeField] LineRenderer laserLine;
        [SerializeField] LineRenderer aimLine;
        [SerializeField] ParticleSystem chargeFX;
        [SerializeField] bool debug = false;

        [Header("Ignore")]
        [Space]
        [SerializeField] Collider2D[] ignoredColliders = new Collider2D[0];

        [Header("Damage")]
        [Space]
        [SerializeField][Range(0f, 1f)] float chargingLineWidth = 0.1f;
        [SerializeField][Range(0f, 0.5f)] float chargingLineVariance = 0.1f;
        [SerializeField][Range(0.01f, 1f)] float chargeTime = 0.5f;
        [SerializeField][Range(0.01f, 1f)] float fireTime = 0.2f;
        [SerializeField][Range(0.01f, 1f)] float holdTime = 0.2f;
        [SerializeField][Range(0.01f, 1f)] float damageStartThreshold = 0.8f;
        [SerializeField][Range(0f, 1000f)] float damageAmount = 100f;
        [SerializeField][Range(0f, 1000f)] float damageVariance = 20f;

        [Header("Difficulty Settings")]
        [Space]
        [SerializeField][Range(0f, 5f)] float aimSpeedEasyMod = 1f;
        [SerializeField][Range(0f, 5f)] float aimSpeedMediumMod = 1f;
        [SerializeField][Range(0f, 5f)] float aimSpeedHardMod = 1f;
        [SerializeField][Range(0f, 5f)] float aimSpeedInsaneMod = 1f;

        [Header("Audio")]
        [Space]
        [SerializeField] Sound chargeUpSound;
        [SerializeField] Sound fireSound;

        // components
        BoxCollider2D box;

        // cached
        PlayerGeneral player;
        DamageableBehaviour parentActor;
        DamageReceiver actor;
        ParticleSystem.MainModule chargeFXModule;

        // state
        Timer charging = new Timer(TimerDirection.Increment);
        Timer firing = new Timer(TimerDirection.Increment);
        Timer holding = new Timer(TimerDirection.Increment);
        Timer closing = new Timer(TimerDirection.Decrement);
        bool isFiring = false;
        bool isLocked = false;
        Coroutine ieFire;
        Coroutine ieChargeUp;

        // state - aiming
        Vector3 vectorToPlayer;
        Vector2 aimVector = Vector2.down;
        float aimAngle = 0f;
        Quaternion aim = Quaternion.identity;
        RaycastHit2D[] raycastHits = new RaycastHit2D[10];
        Vector2 aimEndpoint = new Vector2(0, -100f);

        public void SetLocked(bool value) {
            isLocked = value;
        }

        public void Fire() {
            if (ieFire != null) StopCoroutine(ieFire);
            ieFire = StartCoroutine(IFire());
        }

        public void ChargeUp() {
            if (ieChargeUp != null) StopCoroutine(ieChargeUp);
            ieChargeUp = StartCoroutine(IChargeUp());
        }

        void Start() {
            box = GetComponentInChildren<BoxCollider2D>();
            parentActor = GetComponentInParent<DamageableBehaviour>();
            charging.SetDuration(chargeTime);
            firing.SetDuration(fireTime);
            holding.SetDuration(holdTime);
            closing.SetDuration(fireTime);
            chargeUpSound.Init(this);
            fireSound.Init(this);
            player = PlayerUtils.FindPlayer();

            AppIntegrity.AssertPresent(box);
            AppIntegrity.AssertPresent(laserLine);
            AppIntegrity.AssertPresent(parentActor);

            laserLine.widthMultiplier = 0f;
            laserLine.enabled = true;
            box.enabled = false;
            aimVector = -transform.up;
            if (chargeFX != null) {
                chargeFX.Stop();
                chargeFXModule = chargeFX.main;
            }
        }

        void Update() {
            if (player == null || !player.isAlive) player = PlayerUtils.FindPlayer();
            HandleLookAtPlayer();
        }

        void FixedUpdate() {
            // the aim laser always points towards local down
            aimEndpoint.y = -GetAimIntersectDistance();
            aimLine.SetPosition(1, aimEndpoint);
        }

        void HandleLookAtPlayer() {
            if (isLocked) return;
            if (player == null || !player.isAlive) return;
            if (Vector2.Distance(transform.position, player.transform.position) > 50f || !Utils.IsObjectOnScreen(gameObject)) {
                aimLine.enabled = false;
                return;
            }
            aimLine.enabled = true;
            vectorToPlayer = player.transform.position - transform.position;
            aimVector = Vector2.MoveTowards(aimVector, vectorToPlayer.normalized, aimSpeed * GetAimSpeedMod() * Time.deltaTime);
            aimAngle = Vector2.SignedAngle(Vector2.down, aimVector);
            transform.rotation = Quaternion.AngleAxis(aimAngle, Vector3.forward);
        }

        float GetAimIntersectDistance() {
            if (firing.active) return 100f;
            if (closing.active) return 100f;
            if (player == null || !player.isAlive) return 100f;
            int numResults = Physics2D.RaycastNonAlloc(transform.position, aimVector.normalized, raycastHits, 100f);
            for (int i = 0; i < numResults; i++) {
                if (IsIgnoredCollider(raycastHits[i].collider)) continue;
                if (raycastHits[i].collider.tag == UTag.Bullet || raycastHits[i].collider.tag == UTag.Laser) continue;
                return Vector2.Distance(raycastHits[i].collider.transform.position, transform.position);
            }
            return 100f;
        }

        bool IsIgnoredCollider(Collider2D collider) {
            foreach (var ignore in ignoredColliders) {
                if (collider == ignore) return true;
            }
            return false;
        }

        void OnTriggerEnter2D(Collider2D other) {
            HandleHit(other);
        }

        void OnTriggerStay2D(Collider2D other) {
            HandleHit(other);
        }

        void HandleHit(Collider2D other) {
            actor = other.GetComponent<DamageReceiver>();
            if (actor == null) return;
            // don't harm yourself with da beam!!
            if (parentActor != null && actor.uuid == parentActor.uuid) return;
            actor.TakeDamage(Utils.RandomVariance(damageAmount, damageVariance, damageAmount / 2f), Damage.DamageType.Disruptor, false);
        }

        IEnumerator IFire() {
            isLocked = true;
            box.enabled = false;
            chargeUpSound.Stop();
            fireSound.Stop();
            fireSound.Play();
            firing.Start();
            while (firing.active) {
                if (firing.value > damageStartThreshold) box.enabled = true;
                laserLine.widthMultiplier = Mathf.Max(firing.value, chargingLineWidth);
                firing.Tick();
                yield return null;
            }

            holding.Start();
            while (holding.active) {
                laserLine.widthMultiplier = UnityEngine.Random.Range(0.8f, 1f);
                holding.Tick();
                yield return null;
            }

            closing.Start();
            while (closing.active) {
                if (firing.value < damageStartThreshold) box.enabled = false;
                laserLine.widthMultiplier = closing.value;
                closing.Tick();
                yield return null;
            }

            if (chargeFX != null) chargeFX.Stop();
            laserLine.widthMultiplier = 0f;
            box.enabled = false;
            fireSound.Stop();
            ieFire = null;
            isLocked = false;
        }

        IEnumerator IChargeUp() {
            laserLine.widthMultiplier = GetChargingLineWidth();
            if (chargeFX != null) chargeFX.Play();
            chargeUpSound.Play();
            charging.Start();
            while (charging.active) {
                laserLine.widthMultiplier = GetChargingLineWidth();
                if (chargeFX != null) chargeFXModule.simulationSpeed = Mathf.Lerp(0.5f, 2f, charging.value);
                charging.Tick();
                yield return null;
            }
            ieChargeUp = null;
        }

        float GetChargingLineWidth() {
            return Utils.RandomVariance2(chargingLineWidth, chargingLineVariance, 0.08f, 0.5f);
        }

        float GetAimSpeedMod() {
            switch (GameManager.current.difficulty) {
                case GameDifficulty.Easy:
                    return aimSpeedEasyMod;
                case GameDifficulty.Medium:
                    return aimSpeedMediumMod;
                case GameDifficulty.Hard:
                    return aimSpeedHardMod;
                case GameDifficulty.Insane:
                    return aimSpeedInsaneMod;
                default:
                    return 1f;
            }
        }

        void OnGUI() {
            if (!debug) return;
            if (GUILayout.Button("Fire")) {
                Fire();
            }
        }
    }
}

