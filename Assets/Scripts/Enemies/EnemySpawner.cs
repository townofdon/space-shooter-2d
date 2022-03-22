using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Game;
using Event;

namespace Enemies
{
    enum BattleEventType {
        Wave,
        Formation,
        Boss,
        WaitForArbitraryTime,
        WaitUntilEnemiesDestroyed,
        WaitUntilWaveSpawnFinished,
        ArbitraryEvent,
        EventLabel,
        // ChangeMusic,
    }

    [System.Serializable]
    class BattleEvent {
        [SerializeField] bool skip;
        [SerializeField] BattleEventType type;
        [SerializeField] string eventLabel;
        // could be a wave, a formation, a boss, incoming asteroids etc.
        [SerializeField] WaveConfigSO wave;
        [SerializeField] GameObject formation;
        [SerializeField] GameObject boss;
        [SerializeField] float arbitraryTime = 0f;
        [SerializeField] int allowableEnemiesLeft = 0;
        [SerializeField] UnityEvent arbitraryEvent;

        // inspector colors
        [SerializeField] Color fieldColorWave;
        [SerializeField] Color fieldColorFormation;
        [SerializeField] Color fieldColorBoss;
        [SerializeField] Color fieldColorWait;
        [SerializeField] Color fieldColorEvent;
        [SerializeField] Color fieldColorLabel;

        public bool Skip => skip;
        public BattleEventType Type => type;
        public WaveConfigSO Wave => wave;
        public float ArbitraryTime => arbitraryTime;
        public int AllowableEnemiesLeft => allowableEnemiesLeft;
        public UnityEvent ArbitraryEvent => arbitraryEvent;
    }

    public class EnemySpawner : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField] List<BattleEvent> battleEvents;
        [SerializeField] bool loopIndefinitely = false;
        [SerializeField] bool debug = false;
        // [SerializeField] GameEvent OnBattleFinished; // old event
        [SerializeField] EventChannelSO eventChannel;

        // all this nonsense just to get colours to show like I want them...
        [Header("Battle Event Colours")][Space]
        [SerializeField] Color _fieldColorWave;
        [SerializeField] Color _fieldColorFormation;
        [SerializeField] Color _fieldColorBoss;
        [SerializeField] Color _fieldColorWait;
        [SerializeField] Color _fieldColorEvent;
        [SerializeField] Color _fieldColorLabel;
        public static Color fieldColorWave;
        public static Color fieldColorFormation;
        public static Color fieldColorBoss;
        public static Color fieldColorWait;
        public static Color fieldColorEvent;
        public static Color fieldColorLabel;
        public void OnBeforeSerialize() {}
        public void OnAfterDeserialize() {
            fieldColorWave = _fieldColorWave;
            fieldColorFormation = _fieldColorFormation;
            fieldColorBoss = _fieldColorBoss;
            fieldColorWait = _fieldColorWait;
            fieldColorEvent = _fieldColorEvent;
            fieldColorLabel = _fieldColorLabel;
        }

        int numEnemiesAlive = 0;
        int battleEventIndex = 0;
        Coroutine currentWaveSpawn;
        Coroutine battle;

        public void OnEnemyDeath(int instanceId, int points) {
            Debug.Log($"ENEMY WAS KILLED - {points} points");
            numEnemiesAlive = Mathf.Max(0, numEnemiesAlive - 1);
        }

        public void BattleFinished() {
            // if (OnBattleFinished != null) OnBattleFinished.Raise();
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
                yield return null;
                foreach (var battleEvent in battleEvents)
                {
                    if (battleEvent.Skip) { battleEventIndex++; continue; }
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
                        // case BattleEventType.ChangeMusic:
                        //     break;
                        default:
                            Debug.LogError("Unsupported BattleEventType: " + battleEvent.Type);
                            break;
                    }
                    battleEventIndex++;
                }
            } while (loopIndefinitely);
            BattleFinished();
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
                    SetEnemyFSM(enemy, wave.initialState);

                    // TODO: REMOVE
                    // switch (wave.mode)
                    // {
                    //     case WaveConfigSO.Mode.FollowPath:
                    //         OnEnemyPathFollow(wave.GetEnemy(i), wave.GetSpawnPosition(), wave.GetWaypoints());
                    //         break;
                    //     case WaveConfigSO.Mode.Spawn:
                    //         OnEnemySpawn(wave.GetEnemy(i), wave.GetSpawnPosition());
                    //         break;
                    //     default:
                    //         break;
                    // }
                    yield return new WaitForSeconds(wave.spawnInterval);
                }
            }
        }

        // GameObject OnEnemySpawn(WaveEnemy waveEnemy, Vector3 spawnPosition) {
        //     GameObject enemy = Instantiate(waveEnemy.enemyPrefab,
        //         spawnPosition + waveEnemy.spawnOffset,
        //         Quaternion.identity,
        //         transform);
        //     return enemy;
        // }

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
