using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Event;
using Battle;
using Player;

namespace Enemies {

    public class EnemySpawner : MonoBehaviour {
        [SerializeField] List<BattleSequenceSO> battleSequences;

        [SerializeField] bool loopIndefinitely = false;
        [SerializeField] bool debug = false;
        [SerializeField] EventChannelSO eventChannel;

        // cached
        PlayerGeneral player;

        // state
        int numEnemiesAlive = 0;
        int battleSequenceIndex = 0;
        int battleEventIndex = 0;
        bool waitingForDialogue = false;
        WaveConfigSO currentWave;
        Coroutine currentWaveSpawn;
        Coroutine battle;

        // state - boss
        List<int> bossesAlive = new List<int>();
        List<int> bossesAliveTemp = new List<int>();

        public void OnEnemyDeath(int instanceId, int points) {
            numEnemiesAlive = Mathf.Max(0, numEnemiesAlive - 1);
            bossesAliveTemp.Clear();
            foreach (var bossId in bossesAlive) {
                if (bossId != instanceId) bossesAliveTemp.Add(bossId);
            }
            bossesAlive = bossesAliveTemp;
        }

        public void OnBossSpawn(int instanceId) {
            bossesAlive.Add(instanceId);
            numEnemiesAlive++;
        }

        public void BattleFinished() {
            eventChannel.OnWinLevel.Invoke();
        }

        public void StopBattle() {
            if (battle != null) StopCoroutine(battle);
        }

        void OnEnable() {
            eventChannel.OnEnemyDeath.Subscribe(OnEnemyDeath);
            eventChannel.OnBossSpawn.Subscribe(OnBossSpawn);
            eventChannel.OnDismissDialogue.Subscribe(OnDismissDialogue);
        }

        void OnDisable() {
            eventChannel.OnEnemyDeath.Unsubscribe(OnEnemyDeath);
            eventChannel.OnBossSpawn.Unsubscribe(OnBossSpawn);
            eventChannel.OnDismissDialogue.Unsubscribe(OnDismissDialogue);
        }

        void Start() {
            battle = StartCoroutine(PlayBattle());
        }

        IEnumerator PlayBattle() {
            do {
                foreach (var battleSequence in battleSequences) {
                    battleEventIndex = 0;
                    foreach (var battleEvent in battleSequence.battleEvents) {
                        if (battleEvent.Skip) { battleEventIndex++; continue; }
                        yield return OnBattleEvent(battleEvent);
                    }
                    battleSequenceIndex++;
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
                        currentWave = battleEvent.Wave;
                        currentWaveSpawn = StartCoroutine(SpawnEnemies(battleEvent.Wave));
                        numEnemiesAlive += battleEvent.Wave.enemyCount;
                    }
                    break;
                case BattleEventType.Boss:
                    // TODO: SPAWN BOSS
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
                case BattleEventType.WaitUntilBossDestroyed:
                    if (bossesAlive.Count > 0) yield return null;
                    break;
                case BattleEventType.ArbitraryEvent:
                    if (battleEvent.ArbitraryEvent != null) {
                        battleEvent.ArbitraryEvent.Invoke();
                    }
                    break;
                case BattleEventType.DestroyAllEnemiesPresent:
                    yield return DestroyAllEnemiesPresent();
                    break;
                case BattleEventType.PlayMusic:
                    // TODO: PLAY MUSIC
                    break;
                case BattleEventType.StopMusic:
                    // TODO: STOP MUSIC
                    break;
                case BattleEventType.ShowDialogue:
                    eventChannel.OnShowDialogue.Invoke(battleEvent.dialogueItem);
                    waitingForDialogue = true;
                    while (waitingForDialogue) yield return null;
                    break;
                default:
                    Debug.LogError("Unsupported BattleEventType: " + battleEvent.Type);
                    break;
            }
            battleEventIndex++;
        }

        IEnumerator SpawnEnemies(WaveConfigSO wave) {
            if (wave != null) {
                for (int i = 0; i < wave.spawnCount; i++) {
                    player = PlayerUtils.FindPlayer();
                    GameObject enemy = SpawnObject(wave.GetEnemy(i), wave);
                    if (wave.HasPath()) {
                        SetEnemyPathfollow(enemy, wave.GetWaypoints(), wave.pathfinderLoopMode, wave.flipX, wave.flipY);
                    }
                    LaunchEnemy(enemy, wave);
                    // TODO: REPLACE WITH B-TREE
                    SetEnemyFSM(enemy, wave.initialState);
                    yield return new WaitForSeconds(wave.spawnInterval);
                }
            }
        }

        GameObject SpawnObject(WaveEnemy enemy, WaveConfigSO wave) {
            return Instantiate(
                enemy.prefab,
                (enemy.hasSpawnLocation
                    ? wave.ParseSpawnLocation(enemy.spawnLocation) + (Vector2)enemy.spawnOffset
                    : wave.GetSpawnPosition()
                ),
                Quaternion.identity,
                transform
            );
        }

        void SetEnemyPathfollow(GameObject enemy, List<Transform> waypoints, PathfinderLoopMode loopMode, bool flipX, bool flipY) {
            var pathFollower = enemy.GetComponent<Pathfollower>();
            if (pathFollower == null) return;
            pathFollower.SetWaypoints(waypoints, flipX, flipY);
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

        void LaunchEnemy(GameObject enemy, WaveConfigSO wave) {
            var rb = enemy.GetComponent<Rigidbody2D>();
            if (rb == null) return;
            rb.velocity = wave.GetLaunchVelocity(enemy.transform.position, player != null ? player.transform.position : Vector2.zero);
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

        void OnDismissDialogue() {
            waitingForDialogue = false;
        }

        void OnGUI() {
            if (!debug) return;
            GUILayout.TextField("Seq " + battleSequenceIndex);
            GUILayout.TextField("Event " + battleEventIndex);
            GUILayout.TextField(numEnemiesAlive.ToString());
        }

        void OnDrawGizmos() {
            if (!debug) return;
            if (currentWave != null) {
                float radius = 0.4f;

                string[] cardinals = new string[] {
                    "N",
                    "S",
                    "E",
                    "W",
                };
                string[] indices = new string[] {
                    "0",
                    "1",
                    "2",
                    "3",
                    "4",
                    "5",
                    "6",
                    "7",
                    "8",
                    "9",
                    "10",
                    "11",
                    "12",
                    "13",
                    "14",
                    "15",
                    "16",
                    "17",
                    "18",
                    "19",
                    "20",
                    "21",
                    "22",
                    "23",
                    "24",
                };
                string[] offsets = new string[] {
                    "0",
                    "1",
                    "2",
                    "3",
                    "4",
                    "5",
                };

                foreach (var cardinal in cardinals) {
                    Gizmos.color = Color.magenta;
                    if (cardinal == "N") Gizmos.color = Color.cyan;
                    if (cardinal == "W") Gizmos.color = Color.yellow;
                    if (cardinal == "E") Gizmos.color = Color.red;
                    if (cardinal == "S") Gizmos.color = Color.blue;
                    foreach (var index in indices) {
                        foreach (var offset in offsets) {
                            Gizmos.DrawSphere(currentWave.ParseSpawnLocation(
                                $"{cardinal}-{index}-{offset}"
                            ), radius);
                        }
                    }
                }
            }
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
