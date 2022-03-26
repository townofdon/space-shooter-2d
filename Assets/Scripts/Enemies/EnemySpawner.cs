using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Event;
using Battle;

namespace Enemies
{

    public class EnemySpawner : MonoBehaviour {
        [SerializeField] List<BattleSequence> battleSequences;

        [SerializeField] bool loopIndefinitely = false;
        [SerializeField] bool debug = false;
        [SerializeField] EventChannelSO eventChannel;

        int numEnemiesAlive = 0;
        int battleEventIndex = 0;
        Coroutine currentWaveSpawn;
        Coroutine battle;

        public void OnEnemyDeath(int instanceId, int points) {
            numEnemiesAlive = Mathf.Max(0, numEnemiesAlive - 1);
        }

        public void BattleFinished() {
            eventChannel.OnWinLevel.Invoke();
        }

        public void StopBattle() {
            if (battle != null) StopCoroutine(battle);
        }

        void OnEnable() {
            eventChannel.OnEnemyDeath.Subscribe(OnEnemyDeath);
        }

        void OnDisable() {
            eventChannel.OnEnemyDeath.Unsubscribe(OnEnemyDeath);
        }

        void Start() {
            battle = StartCoroutine(PlayBattle());
        }

        IEnumerator PlayBattle() {
            do
            {
                foreach (var battleSequence in battleSequences) {
                    foreach (var battleEvent in battleSequence.battleEvents) {
                        if (battleEvent.Skip) { battleEventIndex++; continue; }
                        yield return OnBattleEvent(battleEvent);
                    }
                }
            } while (loopIndefinitely);
            BattleFinished();
        }

        IEnumerator OnBattleEvent(BattleEvent battleEvent) {
            switch (battleEvent.Type) {
                case BattleEventType.EventLabel:
                    // inspector only
                    break;
                case BattleEventType.Wave:
                    if (battleEvent.Wave != null) {
                        currentWaveSpawn = StartCoroutine(SpawnEnemies(battleEvent.Wave));
                        numEnemiesAlive += battleEvent.Wave.enemyCount;
                    }
                    break;
                case BattleEventType.Boss:
                    // TODO: SPAWN BOSS
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
                case BattleEventType.WaitUntilWaveSpawnFinished:
                    if (currentWaveSpawn != null) yield return currentWaveSpawn;
                    break;
                case BattleEventType.ArbitraryEvent:
                    if (battleEvent.ArbitraryEvent != null) {
                        battleEvent.ArbitraryEvent.Invoke();
                    }
                    break;
                case BattleEventType.DestroyAllEnemiesPresent:
                    yield return DestroyAllEnemiesPresent();
                    break;
                // case BattleEventType.ChangeMusic:
                //     break;
                default:
                    Debug.LogError("Unsupported BattleEventType: " + battleEvent.Type);
                    break;
            }
            battleEventIndex++;
        }


        IEnumerator SpawnEnemies(WaveConfigSO wave) {
            if (wave != null) {
                for (int i = 0; i < wave.enemyCount; i++) {
                    // spawn enemy
                    GameObject enemy = Instantiate(wave.GetEnemy(i).prefab,
                        wave.GetSpawnPosition() + wave.GetEnemy(i).spawnOffset,
                        Quaternion.identity,
                        transform);
                    if (wave.mode == WaveConfigSO.Mode.FollowPath) {
                        SetEnemyPathfollow(enemy, wave.GetWaypoints(), wave.pathfinderLoopMode);
                    }
                    // TODO: REPLACE WITH B-TREE
                    SetEnemyFSM(enemy, wave.initialState);
                    yield return new WaitForSeconds(wave.spawnInterval);
                }
            }
        }

        void SetEnemyPathfollow(GameObject enemy, List<Transform> waypoints, PathfinderLoopMode loopMode) {
            var pathFollower = enemy.GetComponent<Pathfollower>();
            if (pathFollower == null) return;
            pathFollower.SetWaypoints(waypoints);
            pathFollower.SetLoopMode(loopMode);
            pathFollower.Begin();
            var enemyMovement = enemy.GetComponent<EnemyMovement>();
            if (enemyMovement == null) return;
            enemyMovement.SetMode(MovementMode.Default);
        }

        void SetEnemyFSM(GameObject enemy, FSM.BaseState initialState) {
            var machine = enemy.GetComponent<FSM.FiniteStateMachine>();
            if (machine == null) return;
            machine.SetState(initialState);
        }

        IEnumerator DestroyAllEnemiesPresent() {
            EnemyShip[] enemies = FindObjectsOfType<EnemyShip>();
            foreach (var enemy in enemies) {
                if (enemy == null || !enemy.isAlive) continue;
                yield return new WaitForSecondsRealtime(0.04f);
                enemy.OnDeathByGuardians();
            }
            numEnemiesAlive = 0;
        }

        void OnGUI() {
            if (!debug) return;
            GUILayout.TextField("Event " + battleEventIndex);
            GUILayout.TextField(numEnemiesAlive.ToString());
        }

        // handy enumerator code below

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
    }
}
