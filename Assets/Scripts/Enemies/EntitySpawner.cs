using System.Collections;
using UnityEngine;

using Event;
using Game;
using Core;

namespace Enemies {

    public class EntitySpawner : MonoBehaviour {
        [Header("General settings")]
        [SerializeField] EventChannelSO eventChannel;
        [SerializeField][Range(0f, 100f)] float initialSpawnDelay = 0f;
        [SerializeField][Range(0f, 100f)] float spawnInterval = 30f;
        [SerializeField][Range(0f, 100f)] float spawnVariance = 3f;
        [SerializeField] GameObject prefab;

        [Header("Difficulty")]
        [Space]
        [SerializeField][Range(0f, 5f)] float spawnIntEasyMod = 1f;
        [SerializeField][Range(0f, 5f)] float spawnIntMediumMod = 1f;
        [SerializeField][Range(0f, 5f)] float spawnIntHardMod = 1f;
        [SerializeField][Range(0f, 5f)] float spawnIntInsaneMod = 1f;

        // state
        GameObject spawnedObj;
        SpriteRenderer sr;

        public void Disable() {
            OnDisableAllSpawners();
        }

        public void KillCurrent() {
            if (spawnedObj == null) return;
            EnemyShip enemy = spawnedObj.GetComponentInChildren<EnemyShip>();
            if (enemy == null) return;
            enemy.TakeDamage(1000f, Damage.DamageType.Instakill, false);
        }

        void OnEnable() {
            eventChannel.OnDisableAllSpawners.Subscribe(OnDisableAllSpawners);
        }

        void OnDisable() {
            eventChannel.OnDisableAllSpawners.Unsubscribe(OnDisableAllSpawners);
        }

        private void Awake() {
            sr = GetComponent<SpriteRenderer>();
            sr.enabled = false;
        }

        void Start() {
            StartCoroutine(ISpawn());
        }

        void OnDisableAllSpawners() {
            StopAllCoroutines();
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        IEnumerator ISpawn() {
            yield return new WaitForSeconds(initialSpawnDelay);

            SpawnObject();

            while (true) {
                if (prefab != null && spawnedObj == null) {
                    yield return new WaitForSeconds(Utils.RandomVariance(
                        spawnInterval * GetSpawnIntervalMod(),
                        spawnVariance,
                        spawnInterval * GetSpawnIntervalMod() / 2f
                    ));
                    SpawnObject();
                }
                yield return null;
            }
        }

        void SpawnObject() {
            if (spawnedObj == null) spawnedObj = Instantiate(prefab, transform);
        }

        float GetSpawnIntervalMod() {
            switch (GameManager.current.difficulty) {
                case GameDifficulty.Easy:
                    return spawnIntEasyMod;
                case GameDifficulty.Medium:
                    return spawnIntMediumMod;
                case GameDifficulty.Hard:
                    return spawnIntHardMod;
                case GameDifficulty.Insane:
                    return spawnIntInsaneMod;
                default:
                    return 1f;
            }
        }
    }
}

