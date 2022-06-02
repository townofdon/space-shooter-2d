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

        void OnEnable() {
            if (isAlive && (isBoss || tag == UTag.Boss) && !didNotifyBossSpawn) {
                didNotifyBossSpawn = true;
                eventChannel.OnBossSpawn.Invoke(instanceId);
            }
            if (isAlive && isCountableEnemy && !didNotifyEnemySpawn) {
                didNotifyEnemySpawn = true;
                eventChannel.OnEnemySpawn.Invoke();
            }
        }

        void Awake() {
            instanceId = Utils.GetRootInstanceId(gameObject);
            SetColliders();
        }

        void Start() {
            AppIntegrity.AssertPresent<EventChannelSO>(eventChannel);

            rb = GetComponent<Rigidbody2D>();
            if (rb != null) {
                originalDrag = rb.drag;
                rb.drag = 0f; // we will handle physics calcs manually MWA HA HA!!
            }

            ResetHealth();
            RegisterHealthCallbacks(OnDeath, OnHealthDamaged, Utils.__NOOP__);
            if (ship != null) ship.SetActive(true);
            damageSound.Init(this);
            deathSound.Init(this);
        }

        void Update() {
            TickHealth();
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
            eventChannel.OnEnemyDeath.Invoke(instanceId, points, isCountableEnemy);
            if (rb != null) rb.drag = originalDrag; // to make it seem like it was there all along
            if (damageType != DamageType.InstakillQuiet) {
                deathSound.Play();
                pickups.Spawn(transform.position, rb);
                SpawnPointsToast(points);
            }
            foreach (var actor in killOtherObjects) actor.TakeDamage(1000f, DamageType.Instakill);
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

        IEnumerator DeathAnimation() {
            if (explosion != null) Instantiate(explosion, transform);
            if (explosion2 != null) Instantiate(explosion2, transform);
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
