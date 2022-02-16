using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Game;

namespace Enemies
{
    enum BattleEventType {
        Wave,
        Formation,
        Boss,
        WaitForArbitraryTime,
        WaitUntilEnemiesDestroyed,
        WaitUntilLatestWaveSpawnFinished,
        ArbitraryEvent,
        // ChangeMusic,
    }

    [System.Serializable]
    class BattleEvent {
        [SerializeField] string eventName;
        [SerializeField] BattleEventType type;
        // could be a wave, a formation, a boss, incoming asteroids etc.
        [SerializeField] WaveConfigSO wave;
        // [SerializeField] GameObject formation;
        // [SerializeField] GameObject boss;
        [SerializeField] float arbitraryTime = 0f;
        [SerializeField] int allowableEnemiesLeft = 0;

        [HideInInspector]
        [SerializeField] UnityEvent arbitraryEvent;

        public BattleEventType Type => type;
        public WaveConfigSO Wave => wave;
        public float ArbitraryTime => arbitraryTime;
        public int AllowableEnemiesLeft => allowableEnemiesLeft;
        public UnityEvent ArbitraryEvent => arbitraryEvent;
    }

    public class EnemySpawner : MonoBehaviour
    {
        // [SerializeField] List<WaveConfigSO> waves;
        // [SerializeField] List<float> waveIntervals = new List<float>(DefaultWaveIntervals());
        // [SerializeField] UnityEvent onWaveEnd;
        // [SerializeField] bool spawnInfiniteWaves = false;

        [SerializeField] List<BattleEvent> battleEvents;
        [SerializeField] bool loopIndefinitely = false;
        [SerializeField] bool debug = false;
        [SerializeField] GameEvent OnBattleFinished;

        // int currentWaveIndex = 0;

        int numEnemiesAlive = 0;
        Coroutine currentWaveSpawn;
        Coroutine battle;

        public void OnEnemyDeath() {
            numEnemiesAlive = Mathf.Max(0, numEnemiesAlive - 1);
        }

        public void BattleFinished() {
            if (OnBattleFinished != null) OnBattleFinished.Raise();
        }

        public void StopBattle() {
            if (battle != null) StopCoroutine(battle);
        }

        void Start() {
            battle = StartCoroutine(PlayBattle());
        }

        IEnumerator PlayBattle() {
            do
            {
                yield return null;
                foreach (var battleEvent in battleEvents)
                {
                    switch (battleEvent.Type) {
                        case BattleEventType.Wave:
                            if (battleEvent.Wave != null) {
                                currentWaveSpawn = StartCoroutine(SpawnEnemies(battleEvent.Wave));
                                numEnemiesAlive += battleEvent.Wave.enemyCount;
                            }
                            break;
                        case BattleEventType.Boss:
                            // TODO: SPAWN FORMATION
                            break;
                        case BattleEventType.Formation:
                            // TODO: SPAWN FORMATION
                            break;
                        case BattleEventType.WaitForArbitraryTime:
                            yield return new WaitForSeconds(battleEvent.ArbitraryTime);
                            break;
                        case BattleEventType.WaitUntilEnemiesDestroyed:
                            while (numEnemiesAlive > battleEvent.AllowableEnemiesLeft) yield return null;
                            break;
                        case BattleEventType.WaitUntilLatestWaveSpawnFinished:
                            if (currentWaveSpawn != null) yield return currentWaveSpawn;
                            break;
                        case BattleEventType.ArbitraryEvent:
                            if (battleEvent.ArbitraryEvent != null) {
                                battleEvent.ArbitraryEvent.Invoke();
                            }
                            break;
                        // case BattleEventType.ChangeMusic:
                        //     break;
                        default:
                            Debug.LogError("Unsupported BattleEventType: " + battleEvent.Type);
                            break;
                    }
                }
                BattleFinished();

                // currentWaveIndex = 0;
                // while (currentWaveIndex < battleEvents.Count) {
                //     BattleEvent battleEvent = waves[currentWaveIndex];
                //     yield return new WaitForSeconds(wave.delayBeforeSpawn);
                //     Coroutine waveSpawn = StartCoroutine(SpawnEnemies(wave));
                //     if (wave.waitUntilFinished) {
                //         yield return waveSpawn;
                //     } else {
                //         yield return new WaitForSeconds(GetNextWaveInterval());
                //     }
                //     currentWaveIndex++;
                // }
                // if (onWaveEnd != null && !spawnInfiniteWaves) onWaveEnd.Invoke();
            } while (loopIndefinitely);
        }

        IEnumerator SpawnEnemies(WaveConfigSO wave) {
            if (wave != null) {
                for (int i = 0; i < wave.enemyCount; i++) {
                    GameObject enemy = Instantiate(wave.GetEnemy(i),
                        wave.GetStartingWaypoint().position,
                        Quaternion.identity,
                        transform);
                    enemy.GetComponent<Pathfinder>().SetWave(wave);
                    yield return new WaitForSeconds(wave.spawnInterval);
                }
            }
        }


        // int currentWaveIndex = 0;
        // IEnumerator<float> waveInterval;

        // void Start() {
        //     waveInterval = WaveIntervalEnumerator().GetEnumerator();
        //     StartCoroutine(SpawnWaves());

        //     // determine number of enemies for all waves
        //     int enemyCount = 0;
        //     foreach (var wave in waves)
        //     {
        //         enemyCount += wave.enemyCount;
        //     }
        // }

        // void OnDestroy() {
        //     waveInterval.Dispose();
        // }

        // IEnumerator SpawnWaves() {
        //     do
        //     {
        //         currentWaveIndex = 0;
        //         while (currentWaveIndex < waves.Count) {
        //             WaveConfigSO wave = waves[currentWaveIndex];
        //             yield return new WaitForSeconds(wave.delayBeforeSpawn);
        //             Coroutine waveSpawn = StartCoroutine(SpawnEnemies(wave));
        //             if (wave.waitUntilFinished) {
        //                 yield return waveSpawn;
        //             } else {
        //                 yield return new WaitForSeconds(GetNextWaveInterval());
        //             }
        //             currentWaveIndex++;
        //         }
        //         if (onWaveEnd != null && !spawnInfiniteWaves) onWaveEnd.Invoke();
        //     } while (spawnInfiniteWaves);
        // }

        // IEnumerator SpawnEnemies(WaveConfigSO wave) {
        //     for (int i = 0; i < wave.enemyCount; i++) {
        //         GameObject enemy = Instantiate(wave.GetEnemy(i),
        //             wave.GetStartingWaypoint().position,
        //             Quaternion.identity,
        //             transform);
        //         enemy.GetComponent<Pathfinder>().SetWave(wave);
        //         yield return new WaitForSeconds(wave.spawnInterval);
        //     }
        // }

        // float GetNextWaveInterval() {
        //     waveInterval.MoveNext();
        //     return waveInterval.Current;
        // }

        // IEnumerable<float> WaveIntervalEnumerator() {
        //     for (int i = 0; i < waveIntervals.Count; i++)
        //         yield return waveIntervals[i];
        //     while (true)
        //         yield return waveIntervals[waveIntervals.Count - 1];
        // }

        // static IEnumerable<float> DefaultWaveIntervals() {
        //     float waveInterval = 3f;
        //     while (waveInterval >= 1f) {
        //         yield return waveInterval;
        //         waveInterval -= 0.5f;
        //     }
        // }

        void OnGUI() {
            if (!debug) return;
            GUILayout.TextField(numEnemiesAlive.ToString());    
        }
    }
}
