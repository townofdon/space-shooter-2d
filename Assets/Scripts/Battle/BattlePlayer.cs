using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using Event;
using Player;
using NPCs;
using Game;
using Audio;
using Enemies;

namespace Battle {

    public class BattlePlayer : MonoBehaviour {
        [SerializeField] List<BattleSequenceSO> battleSequences;
        [SerializeField] float timeStartDelay = 3f;
        [SerializeField][Tooltip("show upgrade panel at end of level")] bool showUpgradePanel = true;
        [SerializeField] float enemyCountRefreshRate = 5f;

        [Space]

        [SerializeField] bool loopIndefinitely = false;
        [SerializeField] bool debug = false;

        [Space]
        [SerializeField] EventChannelSO eventChannel;

        // cached
        PlayerGeneral player;
        PlayerInput playerInput;
        AsteroidLauncher asteroidLauncher;

        // state
        int numEnemiesAlive = 0;
        int battleSequenceIndex = 0;
        int battleEventIndex = 0;
        BattleEvent currentBattleEvent;
        bool waitingForDialogue = false;
        bool waitingForBattleTrigger = false;
        WaveConfigSO currentWave;
        Coroutine currentWaveSpawn;
        Coroutine battle;
        Coroutine refreshEnemyCount;

        // state - boss
        List<int> bossesAlive = new List<int>();
        List<int> bossesAliveTemp = new List<int>();

        public void OnEnemySpawn() {
            numEnemiesAlive++;
        }

        public void OnEnemyDeath(int instanceId, int points, bool isCountableEnemy = true) {
            if (!isCountableEnemy) return;
            numEnemiesAlive = Mathf.Max(0, numEnemiesAlive - 1);
            bossesAliveTemp.Clear();
            foreach (var bossId in bossesAlive) {
                if (bossId != instanceId) bossesAliveTemp.Add(bossId);
            }
            bossesAlive = bossesAliveTemp;
        }

        public void OnBossSpawn(int instanceId) {
            bossesAlive.Add(instanceId);
        }

        public void BattleFinished() {
            if (refreshEnemyCount != null) StopCoroutine(refreshEnemyCount);
        }

        public void StopBattle() {
            if (battle != null) StopCoroutine(battle);
            if (refreshEnemyCount != null) StopCoroutine(refreshEnemyCount);
        }

        public void DestroyAllEnemiesPresent(bool isQuiet = false) {
            GameManager.current.DestroyAllEnemies(isQuiet);
            numEnemiesAlive = 0;
        }

        public void DisableAllSpawners() {
            eventChannel.OnDisableAllSpawners.Invoke();
        }

        void OnEnable() {
            eventChannel.OnEnemyDeath.Subscribe(OnEnemyDeath);
            eventChannel.OnEnemySpawn.Subscribe(OnEnemySpawn);
            eventChannel.OnBossSpawn.Subscribe(OnBossSpawn);
            eventChannel.OnDismissDialogue.Subscribe(OnDismissDialogue);
            eventChannel.OnDestroyAllEnemies.Subscribe(OnDestroyAllEnemies);
            eventChannel.OnBattleTriggerCrossed.Subscribe(OnBattleTriggerCrossed);
        }

        void OnDisable() {
            eventChannel.OnEnemyDeath.Unsubscribe(OnEnemyDeath);
            eventChannel.OnEnemySpawn.Unsubscribe(OnEnemySpawn);
            eventChannel.OnBossSpawn.Unsubscribe(OnBossSpawn);
            eventChannel.OnDismissDialogue.Unsubscribe(OnDismissDialogue);
            eventChannel.OnDestroyAllEnemies.Unsubscribe(OnDestroyAllEnemies);
            eventChannel.OnBattleTriggerCrossed.Unsubscribe(OnBattleTriggerCrossed);
        }

        protected void Init() {
            asteroidLauncher = FindObjectOfType<AsteroidLauncher>();
        }

        protected void StartBattle() {
            refreshEnemyCount = StartCoroutine(IRefreshEnemyCount());
            battle = StartCoroutine(PlayBattle());
        }

        protected void AddBattleSequence(BattleSequenceSO sequence) {
            battleSequences.Add(sequence);
        }

        void OnDestroyAllEnemies() {
            numEnemiesAlive = 0;
        }

        IEnumerator PlayBattle() {
            yield return new WaitForSeconds(timeStartDelay);
            do {
                foreach (var battleSequence in battleSequences) {
                    if (GameManager.current.difficulty < battleSequence.spawnDifficulty) { battleSequenceIndex++; continue; }
                    battleEventIndex = 0;
                    foreach (var battleEvent in battleSequence.battleEvents) {
                        if (battleEvent.Skip) { battleEventIndex++; continue; }
                        yield return OnBattleEvent(battleEvent);
                    }
                    battleSequenceIndex++;
                }
            } while (loopIndefinitely);
            BattleFinished();
            battle = null;
        }

        IEnumerator OnBattleEvent(BattleEvent battleEvent) {
            currentBattleEvent = battleEvent;
            switch (battleEvent.Type) {
                case BattleEventType.EventLabel:
                    // inspector only
                    break;
                case BattleEventType.Wave:
                    if (battleEvent.Wave != null) {
                        currentWave = battleEvent.Wave;
                        currentWaveSpawn = StartCoroutine(SpawnEnemies(battleEvent.Wave));
                        // numEnemiesAlive += battleEvent.Wave.enemyCount;
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
                case BattleEventType.WaitUntilTrigger:
                    waitingForBattleTrigger = true;
                    while (waitingForBattleTrigger) yield return null;
                    break;
                case BattleEventType.ArbitraryEvent:
                    if (battleEvent.ArbitraryEvent != null) {
                        battleEvent.ArbitraryEvent.Invoke();
                    }
                    break;
                case BattleEventType.DestroyAllEnemiesPresent:
                    DisableAllSpawners();
                    DestroyAllEnemiesPresent();
                    break;
                case BattleEventType.PlayMusic:
                    AudioManager.current.CueTrack(battleEvent.track);
                    break;
                case BattleEventType.StopMusic:
                    AudioManager.current.StopTrack();
                    break;
                case BattleEventType.ShowDialogue:
                    DisableAllSpawners();
                    waitingForDialogue = true;
                    eventChannel.OnShowDialogue.Invoke(battleEvent.dialogueItem);
                    while (waitingForDialogue) yield return null;
                    break;
                case BattleEventType.ShowHint:
                    playerInput = GetPlayerInput();
                    eventChannel.OnShowHint.Invoke(battleEvent.hint, playerInput != null ? playerInput.currentControlScheme : "Keyboard&Mouse");
                    break;
                case BattleEventType.ActivateAsteroidLauncher:
                    if (asteroidLauncher != null) {
                        asteroidLauncher.gameObject.SetActive(true);
                        asteroidLauncher.enabled = true;
                    }
                    break;
                case BattleEventType.DeactivateAsteroidLauncher:
                    if (asteroidLauncher != null) {
                        asteroidLauncher.enabled = false;
                        asteroidLauncher.gameObject.SetActive(false);
                    }
                    break;
                case BattleEventType.WinLevel:
                    DisableAllSpawners();
                    eventChannel.OnWinLevel.Invoke(showUpgradePanel);
                    break;
                case BattleEventType.XtraLife:
                    eventChannel.OnSpawnXtraLife.Invoke();
                    break;
                default:
                    Debug.LogError("Unsupported BattleEventType: " + battleEvent.Type);
                    break;
            }
            battleEventIndex++;
        }

        PlayerInput GetPlayerInput() {
            if (playerInput != null) return playerInput;
            player = PlayerUtils.FindPlayer();
            if (player == null) return null;
            return player.GetComponent<PlayerInput>();
        }

        IEnumerator SpawnEnemies(WaveConfigSO wave) {
            if (wave != null) {
                for (int i = 0; i < wave.numLoops; i++) {
                    for (int j = 0; j < wave.spawnCount; j++) {
                        player = PlayerUtils.FindPlayer();
                        GameObject enemy = SpawnObject(wave.GetEnemy(j), wave);
                        if (wave.HasPath()) {
                            SetEnemyPathfollow(enemy, wave.GetWaypoints(), wave.pathfinderLoopMode, wave.flipX, wave.flipY, wave.maxPathLoops);
                        }
                        LaunchEnemy(enemy, wave);
                        // TODO: REPLACE WITH B-TREE
                        SetEnemyFSM(enemy, wave.initialState);
                        yield return new WaitForSeconds(wave.spawnInterval);
                    }
                    if (i < wave.numLoops - 1) yield return new WaitForSeconds(wave.loopInterval);
                }
            }
        }

        GameObject SpawnObject(WaveEnemy enemy, WaveConfigSO wave) {
            return Instantiate(
                enemy.prefab,
                (enemy.hasSpawnLocation
                    ? wave.ParseSpawnLocation(enemy.spawnLocation) + (Vector2)enemy.spawnOffset
                    : wave.GetSpawnPosition() + enemy.spawnOffset
                ),
                Quaternion.identity
            );
        }

        void SetEnemyPathfollow(GameObject enemy, List<Transform> waypoints, PathfinderLoopMode loopMode, bool flipX, bool flipY, int maxPathLoops) {
            var pathFollower = enemy.GetComponent<Pathfollower>();
            if (pathFollower == null) return;
            pathFollower.SetWaypoints(waypoints, flipX, flipY);
            pathFollower.SetLoopMode(loopMode);
            pathFollower.SetMaxLoops(maxPathLoops);
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

        void OnDismissDialogue() {
            waitingForDialogue = false;
        }

        void OnBattleTriggerCrossed() {
            waitingForBattleTrigger = false;
        }

        IEnumerator IRefreshEnemyCount() {
            while (true) {
                yield return new WaitForSeconds(enemyCountRefreshRate);
                RefreshEnemyCount();
            }
        }

        int refreshCount;
        void RefreshEnemyCount() {
            if (battle == null) return;
            refreshCount = 0;
            foreach (EnemyShip enemy in FindObjectsOfType<EnemyShip>()) {
                if (enemy.isAlive && enemy.isActiveAndEnabled && enemy.isCountable) refreshCount++;
            }
            numEnemiesAlive = refreshCount;
        }

        void OnGUI() {
            if (!debug) return;
            GUILayout.TextField("Seq " + battleSequenceIndex);
            GUILayout.TextField("Event " + battleEventIndex);
            GUILayout.TextField((currentBattleEvent != null) ? System.Enum.GetName(typeof(BattleEventType), currentBattleEvent.Type) : "null");
            GUILayout.TextField("#e=" + numEnemiesAlive.ToString());
            GUILayout.TextField("#b=" + bossesAlive.Count.ToString());
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
