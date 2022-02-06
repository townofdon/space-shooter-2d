using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Enemies
{

    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] List<WaveConfigSO> waves;
        [SerializeField] List<float> waveIntervals = new List<float>(DefaultWaveIntervals());
        [SerializeField] UnityEvent onWaveEnd;
        [SerializeField] bool spawnInfiniteWaves = false;

        int currentWaveIndex = 0;
        IEnumerator<float> waveInterval;

        void Start() {
            waveInterval = WaveIntervalEnumerator().GetEnumerator();
            StartCoroutine(SpawnWaves());
        }

        void OnDestroy() {
            waveInterval.Dispose();
        }

        IEnumerator SpawnWaves() {
            do
            {
                currentWaveIndex = 0;
                while (currentWaveIndex < waves.Count) {
                    WaveConfigSO wave = waves[currentWaveIndex];
                    yield return new WaitForSeconds(wave.delayBeforeSpawn);
                    Coroutine waveSpawn = StartCoroutine(SpawnEnemies(wave));
                    if (wave.waitUntilFinished) {
                        yield return waveSpawn;
                    } else {
                        yield return new WaitForSeconds(GetNextWaveInterval());
                    }
                    currentWaveIndex++;
                }
                if (onWaveEnd != null && !spawnInfiniteWaves) onWaveEnd.Invoke();
            } while (spawnInfiniteWaves);
        }

        IEnumerator SpawnEnemies(WaveConfigSO wave) {
            for (int i = 0; i < wave.enemyCount; i++) {
                GameObject enemy = Instantiate(wave.GetEnemy(i),
                    wave.GetStartingWaypoint().position,
                    Quaternion.identity,
                    transform);
                enemy.GetComponent<Pathfinder>().SetWave(wave);
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }

        float GetNextWaveInterval() {
            waveInterval.MoveNext();
            return waveInterval.Current;
        }

        IEnumerable<float> WaveIntervalEnumerator() {
            for (int i = 0; i < waveIntervals.Count; i++)
                yield return waveIntervals[i];
            while (true)
                yield return waveIntervals[waveIntervals.Count - 1];
        }

        static IEnumerable<float> DefaultWaveIntervals() {
            float waveInterval = 3f;
            while (waveInterval >= 1f) {
                yield return waveInterval;
                waveInterval -= 0.5f;
            }
        }
    }
}
