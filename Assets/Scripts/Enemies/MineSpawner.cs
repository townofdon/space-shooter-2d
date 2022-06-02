using System.Collections;
using UnityEngine;

using Audio;
using Core;
using Player;
using Game;

namespace Enemies {

    public class MineSpawner : MonoBehaviour {
        [Header("Minespawner")]
        [Space]
        [SerializeField] bool debug;
        [SerializeField][Range(0f, 10f)] float initialDelay = 3f;
        [SerializeField][Range(0f, 10f)] float spawnInterval = 3f;
        [SerializeField][Range(0f, 10f)] float spawnVariance = 0.4f;
        [SerializeField] GameObject minePrefab;

        [Header("Spawn Sequences")]
        [Space]
        [SerializeField] float delayBetweenSpawns = 0.1f;
        [SerializeField] AnimationCurve agroNumSpawnsEasy = AnimationCurve.Linear(0f, 2f, 1f, 1f);
        [SerializeField] AnimationCurve agroNumSpawnsMedium = AnimationCurve.Linear(0f, 3f, 1f, 1f);
        [SerializeField] AnimationCurve agroNumSpawnsHard = AnimationCurve.Linear(0f, 4f, 1f, 1f);
        [SerializeField] AnimationCurve agroNumSpawnsInsane = AnimationCurve.Linear(0f, 5f, 1f, 1f);

        [Header("Launch")]
        [Space]
        [SerializeField] Vector2 launchHeading = Vector3.down;
        [SerializeField][Range(0f, 180f)] float headingVariance = 0f;
        [SerializeField][Range(0f, 40f)] float launchVelocity = 0f;
        [SerializeField][Range(0f, 20f)] float velocityVariance = 0f;
        [SerializeField][Range(0f, 1f)] float aimTowardsPlayer = 0f;
        [SerializeField] AnimationCurve predictionCurveEasy = AnimationCurve.EaseInOut(0f, 0.3f, 1f, 0.1f);
        [SerializeField] AnimationCurve predictionCurveMedium = AnimationCurve.EaseInOut(0f, 0.5f, 1f, 0.1f);
        [SerializeField] AnimationCurve predictionCurveHard = AnimationCurve.EaseInOut(0f, 0.7f, 1f, 0.1f);
        [SerializeField] AnimationCurve predictionCurveInsane = AnimationCurve.EaseInOut(0f, 0.9f, 1f, 0.1f);

        [Header("Prediction")]
        [SerializeField] private float _maxDistancePredict = 100;
        [SerializeField] private float _minDistancePredict = 5;
        [SerializeField] private float _maxTimePrediction = 5;

        [Header("Audio")]
        [Space]
        [SerializeField] Sound openSound;
        [SerializeField] Sound spawnSound;

        // cached
        EnemyShip enemy;
        Animator anim;
        GameObject instance;
        PlayerGeneral player;
        AnimationCurve agroNumSpawns;
        int numItemsToSpawn;

        // aim
        Vector3 playerPosition;
        Vector3 playerVelocity;
        Vector3 headingTowardsPlayer;
        Vector3 aimHeading;

        // prediction
        Rigidbody2D rbPlayer;
        AnimationCurve predictionCurve;
        Vector3 prediction;
        float leadTimePercentage;
        float predictionTime;

        public void PlayOpenSound() {
            openSound.Play();
        }

        public void PlaySpawnSound() {
            spawnSound.Play();
        }

        public void SpawnMine() {
            if (!enemy.isAlive) return;
            StartCoroutine(ISpawnMines());
        }

        void Awake() {
            enemy = GetComponent<EnemyShip>();
            anim = GetComponentInChildren<Animator>();
        }

        void Start() {
            openSound.Init(this);
            spawnSound.Init(this);
            StartCoroutine(ISpawnLoop());
        }

        void Update() {
            if (IsPlayerDead()) {
                rbPlayer = null;
                player = PlayerUtils.FindPlayer();
                playerPosition = Vector3.down;
                return;
            }
            if (rbPlayer == null) rbPlayer = player.GetComponent<Rigidbody2D>();
            playerPosition = player.transform.position;
            playerVelocity = rbPlayer == null ? Vector2.zero : rbPlayer.velocity;
        }

        void FixedUpdate() {
            if (debug) GetLaunchVelocity(transform.position, playerPosition, playerVelocity);
        }

        void TriggerSpawn() {
            anim.ResetTrigger("CloseDoors");
            anim.ResetTrigger("SpawnMine");
            anim.Play("MineSpawnerOpen");
            // anim.SetTrigger("SpawnMine");
        }

        IEnumerator ISpawnLoop() {
            yield return new WaitForSeconds(initialDelay);
            while (enemy.isAlive) {
                while (!anim.GetCurrentAnimatorStateInfo(0).IsName("MineSpawnerIdle")) yield return null;
                TriggerSpawn();
                yield return new WaitForSeconds(Utils.RandomVariance2(spawnInterval, spawnVariance, spawnInterval / 2f));
            }
        }

        void HandleSpawnMine() {
            if (enemy == null || !enemy.isAlive) return;
            PlaySpawnSound();
            instance = Instantiate(minePrefab, transform.position, Quaternion.identity);
            var rb = instance.GetComponent<Rigidbody2D>();
            if (rb == null) return;
            rb.velocity = GetLaunchVelocity(instance.transform.position, playerPosition, playerVelocity);
        }

        IEnumerator ISpawnMines() {
            numItemsToSpawn = GetNumItemsToSpawn();
            for (int i = 0; i < numItemsToSpawn; i++) {
                HandleSpawnMine();
                if (i < numItemsToSpawn - 1) yield return new WaitForSeconds(delayBetweenSpawns);
            }
            anim.SetTrigger("CloseDoors");
        }

        bool IsPlayerDead() {
            return player == null || !player.isAlive || !player.isActiveAndEnabled;
        }

        int GetNumItemsToSpawn() {
            if (!enemy.isAlive) return 0;
            agroNumSpawns = GetAgroNumSpawnsCurve();
            return Mathf.FloorToInt(agroNumSpawns.Evaluate(enemy.healthPct));
        }

        AnimationCurve GetAgroNumSpawnsCurve() {
            switch (GameManager.current.difficulty) {
                case GameDifficulty.Easy:
                    return agroNumSpawnsEasy;
                case GameDifficulty.Medium:
                    return agroNumSpawnsMedium;
                case GameDifficulty.Hard:
                    return agroNumSpawnsHard;
                case GameDifficulty.Insane:
                    return agroNumSpawnsInsane;
                default:
                    return agroNumSpawnsMedium;
            }
        }

        // copied from WaveConfigSO because I'm lazy and this is very end-of-project

        public Vector3 GetLaunchVelocity(Vector3 origin, Vector3 targetPosition, Vector3 targetVelocity) {
            if (!enemy.isAlive) return launchHeading * launchVelocity;
            predictionCurve = GetPredictionCurve();
            aimHeading = Vector3.Lerp(
                GetLaunchHeading(origin, targetPosition),
                GetFuturePredictionHeading(origin, targetPosition, targetVelocity),
                predictionCurve.Evaluate(enemy.healthPct)
            ).normalized;
            return aimHeading * Utils.RandomVariance2(launchVelocity, velocityVariance, launchVelocity / 2f);
        }

        Vector3 GetLaunchHeading(Vector3 origin, Vector3 targetPosition) {
            if (IsPlayerDead()) return launchHeading;
            headingTowardsPlayer = (targetPosition - origin).normalized;
            return GetLaunchHeadingVariance() * Vector3.Lerp(launchHeading, headingTowardsPlayer, aimTowardsPlayer);
        }

        Quaternion GetLaunchHeadingVariance() {
            return Quaternion.AngleAxis(UnityEngine.Random.Range(-headingVariance / 2f, headingVariance / 2f), Vector3.forward);
        }

        Vector3 GetFuturePredictionHeading(Vector3 origin, Vector3 targetPosition, Vector3 targetVelocity) {
            if (IsPlayerDead()) return launchHeading;
            leadTimePercentage = Mathf.InverseLerp(_minDistancePredict, _maxDistancePredict, Vector3.Distance(origin, targetPosition));
            predictionTime = Mathf.Lerp(0, _maxTimePrediction, leadTimePercentage);
            prediction = ((targetPosition + targetVelocity * predictionTime) - origin).normalized;
            return prediction;
        }

        AnimationCurve GetPredictionCurve() {
            switch (GameManager.current.difficulty) {
                case GameDifficulty.Easy:
                    return predictionCurveEasy;
                case GameDifficulty.Medium:
                    return predictionCurveMedium;
                case GameDifficulty.Hard:
                    return predictionCurveHard;
                case GameDifficulty.Insane:
                    return predictionCurveInsane;
                default:
                    return predictionCurveMedium;
            }
        }

        void OnGUI() {
            if (!debug) return;
            GUILayout.TextField($"playerPosition={playerPosition}");
            GUILayout.TextField($"playerVelocity={playerVelocity}");
        }

        void OnDrawGizmos() {
            if (!debug) return;
            float distanceToPlayer = Vector3.Distance(transform.position, playerPosition);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + prediction * distanceToPlayer);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + headingTowardsPlayer * distanceToPlayer);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + aimHeading * distanceToPlayer);
        }
    }
}

