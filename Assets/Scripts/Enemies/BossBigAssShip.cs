using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Event;
using Game;

namespace Enemies {

    [System.Serializable]
    public class BigAssFiringBehaviour {
        [SerializeField][Range(0f, 10f)] float _triggerHoldTime = 1f;
        [SerializeField][Range(0f, 10f)] float _triggerReleaseTime = 1f;
        [SerializeField][Range(0f, 10f)] float _triggerTimeVariance = 0f;
        [Space]
        [SerializeField][Range(0f, 5f)] float _triggerHoldEasyMod = 1f;
        [SerializeField][Range(0f, 5f)] float _triggerHoldMediumMod = 1f;
        [SerializeField][Range(0f, 5f)] float _triggerHoldHardMod = 1f;
        [SerializeField][Range(0f, 5f)] float _triggerHoldInsaneMod = 1f;
        [Space]
        [SerializeField][Range(0f, 5f)] float _triggerReleaseEasyMod = 1f;
        [SerializeField][Range(0f, 5f)] float _triggerReleaseMediumMod = 1f;
        [SerializeField][Range(0f, 5f)] float _triggerReleaseHardMod = 1f;
        [SerializeField][Range(0f, 5f)] float _triggerReleaseInsaneMod = 1f;
        [Space]
        [SerializeField] AnimationCurve _firingAgroMod = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        public float triggerHoldTime { get => _triggerHoldTime; }
        public float triggerReleaseTime { get => _triggerReleaseTime; }
        public float triggerTimeVariance { get => _triggerTimeVariance; }
        public float triggerHoldEasyMod { get => _triggerHoldEasyMod; }
        public float triggerHoldMediumMod { get => _triggerHoldMediumMod; }
        public float triggerHoldHardMod { get => _triggerHoldHardMod; }
        public float triggerHoldInsaneMod { get => _triggerHoldInsaneMod; }
        public float triggerReleaseEasyMod { get => _triggerReleaseEasyMod; }
        public float triggerReleaseMediumMod { get => _triggerReleaseMediumMod; }
        public float triggerReleaseHardMod { get => _triggerReleaseHardMod; }
        public float triggerReleaseInsaneMod { get => _triggerReleaseInsaneMod; }
        public AnimationCurve firingAgroMod { get => _firingAgroMod; }
    }

    public class BossBigAssShip : MonoBehaviour {
        [SerializeField] bool debug = false;
        [SerializeField] EventChannelSO eventChannel;
        [Space]
        [SerializeField] EnemyShip bigAssShip;
        [SerializeField] EnemyShooterController shooterController;
        [Space]
        [SerializeField] List<GameObject> enemyWaves = new List<GameObject>(5);
        [SerializeField] List<BigAssFiringBehaviour> firingBehaviours = new List<BigAssFiringBehaviour>(5);



        int currentEnemyWaveIndex = 0;
        int numEnemiesAlive = 0;
        int maxEnemies = 0;

        void OnEnable() {
            eventChannel.OnEnemyDeath.Subscribe(OnEnemyDeath);
            eventChannel.OnEnemySpawn.Subscribe(OnEnemySpawn);
        }

        void OnDisable() {
            eventChannel.OnEnemyDeath.Unsubscribe(OnEnemyDeath);
            eventChannel.OnEnemySpawn.Unsubscribe(OnEnemySpawn);
        }

        void Awake() {
            shooterController.enabled = false;
            if (enemyWaves.Count != firingBehaviours.Count) {
                Debug.LogError($"Enemy waves ({enemyWaves.Count}) need to have same num as firingBehaviours ({firingBehaviours.Count})");
                return;
            }
        }

        void Start() {
            StartCoroutine(IManageEnemies());
        }

        void OnEnemySpawn() {
            numEnemiesAlive++;
            maxEnemies++;
        }

        void OnEnemyDeath(int instanceId, int points, bool isCountableEnemy = true) {
            if (!isCountableEnemy) return;
            numEnemiesAlive = Mathf.Max(0, numEnemiesAlive - 1);
        }

        IEnumerator IManageEnemies() {
            yield return null;
            while (bigAssShip != null && bigAssShip.isAlive && currentEnemyWaveIndex < enemyWaves.Count) {
                ActivateEnemies(enemyWaves[currentEnemyWaveIndex]);
                UpdateShooterSettings();
                shooterController.enabled = true;
                yield return new WaitForSeconds(1f);
                while (numEnemiesAlive > 0) yield return null;
                shooterController.enabled = false;
                currentEnemyWaveIndex++;
                yield return null;
            }
        }

        void ActivateEnemies(GameObject obj) {
            maxEnemies = 0;
            numEnemiesAlive = 0;
            var enemyShips = obj.GetComponentsInChildren<EnemyShip>(true);
            foreach (var enemyShip in enemyShips) {
                enemyShip.enabled = true;
                enemyShip.SetInvulnerable(false);
                enemyShip.gameObject.SetActive(true);
                var mineSpawner = enemyShip.GetComponent<MineSpawner>();
                if (mineSpawner != null) mineSpawner.enabled = true;
            }
            obj.SetActive(true);
        }

        void UpdateShooterSettings() {
            BigAssFiringBehaviour firingBehaviour = firingBehaviours[currentEnemyWaveIndex];
            shooterController.SetTriggerHoldTime(firingBehaviour.triggerHoldTime * GetTriggerHoldMod(firingBehaviour));
            shooterController.SetTriggerReleaseTime(firingBehaviour.triggerReleaseTime * GetTriggerReleaseMod(firingBehaviour));
            shooterController.SetTriggerTimeVariance(firingBehaviour.triggerTimeVariance);
        }

        float GetTriggerHoldMod(BigAssFiringBehaviour firingBehaviour) {
            switch (GameManager.current.difficulty) {
                case GameDifficulty.Easy:
                    return firingBehaviour.triggerHoldEasyMod;
                case GameDifficulty.Medium:
                    return firingBehaviour.triggerHoldMediumMod;
                case GameDifficulty.Hard:
                    return firingBehaviour.triggerHoldHardMod;
                case GameDifficulty.Insane:
                    return firingBehaviour.triggerHoldInsaneMod;
                default:
                    return 1f;
            }
        }

        float GetTriggerReleaseMod(BigAssFiringBehaviour firingBehaviour) {
            switch (GameManager.current.difficulty) {
                case GameDifficulty.Easy:
                    return firingBehaviour.triggerReleaseEasyMod;
                case GameDifficulty.Medium:
                    return firingBehaviour.triggerReleaseMediumMod;
                case GameDifficulty.Hard:
                    return firingBehaviour.triggerReleaseHardMod;
                case GameDifficulty.Insane:
                    return firingBehaviour.triggerReleaseInsaneMod;
                default:
                    return 1f;
            }
        }

        void OnGUI() {
            if (!debug) return;
            GUILayout.TextField("Seq " + currentEnemyWaveIndex);
            GUILayout.TextField("#e=" + numEnemiesAlive.ToString());
        }
    }
}
