using System.Collections;
using UnityEngine;

using Audio;
using Core;
using Player;

namespace Enemies {

    public class MineSpawner : MonoBehaviour {
        [Header("Minespawner")]
        [Space]
        [SerializeField][Range(0f, 10f)] float initialDelay = 3f;
        [SerializeField][Range(0f, 10f)] float spawnInterval = 3f;
        [SerializeField][Range(0f, 10f)] float spawnVariance = 0.4f;
        [SerializeField] GameObject minePrefab;

        [Header("Launch")]
        [Space]
        [SerializeField] Vector2 launchHeading = Vector3.down;
        [SerializeField][Range(0f, 180f)] float headingVariance = 0f;
        [SerializeField][Range(0f, 40f)] float launchVelocity = 0f;
        [SerializeField][Range(0f, 20f)] float velocityVariance = 0f;
        [SerializeField][Range(0f, 1f)] float aimTowardsPlayer = 0f;

        [Header("Audio")]
        [Space]
        [SerializeField] Sound openSound;
        [SerializeField] Sound spawnSound;

        // cached
        EnemyShip enemy;
        Animator anim;
        GameObject instance;
        PlayerGeneral player;
        Vector3 playerPosition;

        public void PlayOpenSound() {
            openSound.Play();
        }

        public void PlaySpawnSound() {
            spawnSound.Play();
        }

        public void SpawnMine() {
            HandleSpawnMine();
        }

        void Awake() {
            enemy = GetComponent<EnemyShip>();
            anim = GetComponentInChildren<Animator>();
        }

        void Start() {
            openSound.Init(this);
            spawnSound.Init(this);
            StartCoroutine(ISpawn());
        }

        void Update() {
            if (IsPlayerDead()) {
                player = PlayerUtils.FindPlayer();
                playerPosition = Vector3.down;
                return;
            }
            playerPosition = player.transform.position;
        }

        void TriggerSpawn() {
            anim.SetTrigger("SpawnMine");
        }

        IEnumerator ISpawn() {
            yield return new WaitForSeconds(initialDelay);
            while (enemy.isAlive) {
                TriggerSpawn();
                yield return new WaitForSeconds(Utils.RandomVariance(spawnInterval, spawnVariance, spawnInterval / 2f));
            }
        }

        void HandleSpawnMine() {
            PlaySpawnSound();
            instance = Instantiate(minePrefab, transform.position, Quaternion.identity);
            var rb = instance.GetComponent<Rigidbody2D>();
            if (rb == null) return;
            rb.velocity = GetLaunchVelocity(instance.transform.position, playerPosition);
        }

        bool IsPlayerDead() {
            return player == null || !player.isAlive || !player.isActiveAndEnabled;
        }

        // copied from WaveConfigSO because I'm lazy and this is very end-of-project

        public Vector3 GetLaunchVelocity(Vector3 origin, Vector3 playerPosition) {
            return GetLaunchHeading(origin, playerPosition) * Utils.RandomVariance2(launchVelocity, velocityVariance, launchVelocity / 2f);
        }

        Vector3 GetLaunchHeading(Vector3 origin, Vector3 playerPosition) {
            if (IsPlayerDead()) return launchHeading;
            Vector3 headingTowardsPlayer = (playerPosition - origin).normalized;
            return GetLaunchHeadingVariance() * Vector3.Lerp(launchHeading, headingTowardsPlayer, aimTowardsPlayer);
        }

        Quaternion GetLaunchHeadingVariance() {
            return Quaternion.AngleAxis(UnityEngine.Random.Range(-headingVariance / 2f, headingVariance / 2f), Vector3.forward);
        }
    }
}

