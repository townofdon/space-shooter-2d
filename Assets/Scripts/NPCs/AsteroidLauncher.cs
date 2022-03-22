using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Player;
using Core;

namespace NPCs {

    public class AsteroidLauncher : MonoBehaviour {
        [Header("Launcher Settings")]
        [Space]
        [SerializeField] List<Transform> spawnPoints = new List<Transform>();
        [SerializeField] Vector3 mainHeading = Vector3.down;
        [SerializeField][Range(0f, 180f)] float headingVariance = 15f;

        [Header("Spawning")]
        [Space]
        [SerializeField] List<GameObject> prefabs;
        [SerializeField] int numSpawns = 1;
        [SerializeField] float spawnInterval = 1f;
        [SerializeField] float spawnVariance = 0.25f;

        [Header("Initial Velocity")]
        [Space]
        [SerializeField] float launchVelocity = 5f;
        [SerializeField] float launchVariance = 2.5f;
        [SerializeField][Range(0f, 1f)] float aimTowardsPlayer = 0f;

        [Header("Randomness")]
        [Space]
        [SerializeField][Range(0, 255)] List<int> seeds = new List<int>();

        // cached
        GameObject player;
        PlayerGeneral playerGeneral;

        // state
        int currentSeedIndex = 0;
        float variance = 0f;
        Transform currentSpawnPoint;
        Rigidbody2D currentRigidBody;
        GameObject lastSpawned;
        Vector3 heading;
        Vector3 headingTowardsPlayer;
        Timer findPlayerInterval = new Timer(TimerDirection.Decrement, TimerStep.FixedDeltaTime);

        public bool isFinishedSpawning => false;

        void Start() {
            FindPlayer();
        }

        void FixedUpdate() {
            FindPlayer();
        }

        void FindPlayer() {
            if (player != null) return;
            if (findPlayerInterval.active) return;
            Debug.Log("searching for player...");
            findPlayerInterval.Start();
            player = GameObject.FindGameObjectWithTag(UTag.Player);
            findPlayerInterval.Tick();
            if (player != null) {
                playerGeneral = player.GetComponent<PlayerGeneral>();
                StartCoroutine(ISpawn());
            }
        }

        void IncrementSeed() {
            currentSeedIndex++;
            if (currentSeedIndex > seeds.Count - 1) currentSeedIndex = 0;
        }

        void Seed() {
            if (seeds.Count <= 0) return;
            UnityEngine.Random.InitState(seeds[currentSeedIndex]);
        }

        Vector3 GetHeading(Vector3 origin) {
            if (player == null || !playerGeneral.isAlive) return GetHeadingVariance() * mainHeading;
            headingTowardsPlayer = (player.transform.position - origin).normalized;
            return GetHeadingVariance() * Vector3.Lerp(mainHeading, headingTowardsPlayer, aimTowardsPlayer);
        }

        Quaternion GetHeadingVariance() {
            Seed();
            return Quaternion.AngleAxis(Utils.RandomVariance(0f, headingVariance), Vector3.forward);
        }

        GameObject GetNextPrefab() {
            Seed();
            return prefabs[UnityEngine.Random.Range(0, prefabs.Count)];
        }

        Transform GetNextSpawnPoint() {
            Seed();
            return spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];
        }

        float GetNextVelocity() {
            Seed();
            return Utils.RandomVariance(launchVelocity, launchVariance, launchVelocity / 2f);
        }

        IEnumerator ISpawn() {
            for (int i = 0; i < numSpawns; i++) {
                // set position via spawnPoint
                currentSpawnPoint = GetNextSpawnPoint();
                lastSpawned = Instantiate(GetNextPrefab(), currentSpawnPoint.position, Quaternion.identity);
                heading = GetHeading(lastSpawned.transform.position);
                currentRigidBody = lastSpawned.GetComponent<Rigidbody2D>();
                if (currentRigidBody != null) {
                    currentRigidBody.velocity = heading * GetNextVelocity();
                }
                Seed();
                yield return new WaitForSeconds(Utils.RandomVariance(spawnInterval, spawnVariance, spawnInterval / 2f));
                IncrementSeed();
            }
        }
    }
}

