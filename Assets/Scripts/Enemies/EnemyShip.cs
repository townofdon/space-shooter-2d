using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Core;
using Damage;
using Audio;
using UI;
using Pickups;
using Event;
using Game;

namespace Enemies {

    public class EnemyShip : DamageableBehaviour
    {
        [Header("Enemy Type")]
        [Space]
        [SerializeField] bool isBoss = false;
        [SerializeField] bool isCountableEnemy = true;

        [Header("Components")][Space]
        [SerializeField] GameObject ship;
        [SerializeField] GameObject explosion;
        [SerializeField] GameObject explosion2;
        [SerializeField] GameObject otherDeathFX;

        [Header("Deeeeeath!")]
        [Space]
        [SerializeField] GameObject deathContainer;
        [SerializeField] GameObject destroyedShip;
        [SerializeField] List<DamageableBehaviour> killOtherObjects = new List<DamageableBehaviour>();

        [Header("Movement")][Space]
        [Header("Audio")][Space]
        [SerializeField] Sound spawnSound;
        [SerializeField] Sound damageSound;
        [SerializeField] Sound deathSound;

        [Header("Pickups")]
        [Space]
        [SerializeField] PickupsSpawnConfig pickups;

        [Header("Points")]
        [Space]
        [SerializeField] int pointsWhenKilledByPlayer = 50;
        [SerializeField] int pointsWhenWoundedByPlayer = 10;
        [SerializeField] GameObject pointsToastPrefab;

        [Header("Events")][Space]
        // [SerializeField] GameEvent OnEnemyDeath;
        [SerializeField] EventChannelSO eventChannel;

        // components
        Rigidbody2D rb;

        // cached
        float originalDrag;

        // props
        int instanceId;

        // state
        bool everDamagedByPlayer = false;
        bool didNotifyBossSpawn = false;
        bool didNotifyEnemySpawn = false;
        bool didNotifyEnemyDeath = false;

        public bool isCountable => isCountableEnemy;

        void OnEnable() {
            OnNotifyEnemySpawn();
            OnNotifyBossSpawn();
            spawnSound.Play();
        }

        void OnDisable() {
            if (isCountableEnemy) OnNotifyEnemyDeath();
        }

        void Awake() {
            instanceId = Utils.GetRootInstanceId(gameObject);
            SetColliders();
        }

        void Start() {
            if (eventChannel == null) Debug.LogError($"eventChannel missing in \"{gameObject.name}\" ({tag})");

            rb = GetComponent<Rigidbody2D>();
            if (rb != null) {
                originalDrag = rb.drag;
                rb.drag = 0f; // we will handle physics calcs manually MWA HA HA!!
            }

            ResetHealth();
            RegisterHealthCallbacks(OnDeath, OnHealthDamaged, Utils.__NOOP__);
            if (ship != null) ship.SetActive(true);
            spawnSound.Init(this);
            damageSound.Init(this);
            deathSound.Init(this);
            OnNotifyEnemySpawn();
            OnNotifyBossSpawn();
        }

        void Update() {
            TickHealth();
        }

        void OnNotifyEnemySpawn() {
            if (!isAlive) return;
            if (!isCountableEnemy) return;
            if (didNotifyEnemySpawn) return;
            didNotifyEnemySpawn = true;
            if (eventChannel != null) eventChannel.OnEnemySpawn.Invoke();
        }

        void OnNotifyBossSpawn() {
            if (!isAlive) return;
            if (!isBoss && tag != UTag.Boss) return;
            if (didNotifyBossSpawn) return;
            didNotifyBossSpawn = true;
            if (eventChannel != null) eventChannel.OnBossSpawn.Invoke(instanceId);
        }

        void OnNotifyEnemyDeath(bool isDamageByPlayer = false) {
            if (didNotifyEnemyDeath) return;
            didNotifyEnemyDeath = true;
            int points = (int)(GetDeathPoints(isDamageByPlayer) * GameUtils.GetPointsMod());
            if (eventChannel != null) eventChannel.OnEnemyDeath.Invoke(instanceId, points, isCountableEnemy);
        }

        void OnHealthDamaged(float amount, DamageType damageType, bool isDamageByPlayer) {
            if (isDamageByPlayer) everDamagedByPlayer = true;
            // Debug.Log("enemy_damage=" + amount + " health=" + health);
            // TODO: FLASH ENEMY SPRITE
            // TODO: PLAY DAMAGE SOUND
            damageSound.Play();
        }

        void OnDeath(DamageType damageType, bool isDamageByPlayer) {
            RemoveMarker();
            int points = (int)(GetDeathPoints(isDamageByPlayer) * GameUtils.GetPointsMod());
            OnNotifyEnemyDeath(isDamageByPlayer);
            if (rb != null) rb.drag = originalDrag; // to make it seem like it was there all along
            if (damageType != DamageType.InstakillQuiet) {
                deathSound.Play();
                pickups.Spawn(transform.position, rb);
                SpawnPointsToast(points);
                SpawnExplosions();
            }
            foreach (var actor in killOtherObjects) if (actor != null) actor.TakeDamage(1000f, damageType == DamageType.InstakillQuiet ? DamageType.InstakillQuiet : DamageType.Instakill);
            StartCoroutine(DeathAnimation());
        }

        void SpawnPointsToast(int points = 0) {
            if (pointsToastPrefab == null) return;
            if (points <= 0) return;
            GameObject instance = Instantiate(pointsToastPrefab, transform.position, Quaternion.identity);
            PointsToast pointsToast = instance.GetComponent<PointsToast>();
            pointsToast.SetPoints(points);
        }

        int GetDeathPoints(bool isDamageByPlayer) {
            if (isDamageByPlayer) return pointsWhenKilledByPlayer;
            if (everDamagedByPlayer) return pointsWhenWoundedByPlayer;
            return 0;
        }

        void RemoveMarker() {
            OffscreenMarker marker = GetComponentInChildren<OffscreenMarker>();
            if (marker != null) marker.Disable();
        }

        void SpawnExplosions() {
            if (explosion != null) Instantiate(explosion, transform);
            if (explosion2 != null) Instantiate(explosion2, transform);
        }

        IEnumerator DeathAnimation() {
            if (otherDeathFX != null) otherDeathFX.SetActive(true);
            if (ship != null) ship.SetActive(false);
            if (destroyedShip != null) {
                GameObject container = deathContainer != null ? deathContainer : new GameObject("DestroyedShip");
                destroyedShip.transform.SetParent(container.transform);
                destroyedShip.SetActive(true);
            }
            yield return new WaitForSeconds(3f);
            while (deathSound.isPlaying) yield return null;

            Destroy(gameObject);

            yield return null;
        }
    }
}
