using System.Collections.Generic;
using UnityEngine;

namespace Enemies {

    [CreateAssetMenu(menuName = "WaveConfig", fileName = "ScriptableObjects/WaveConfig", order = 0)]
    public class WaveConfigSO : ScriptableObject
    {
        [SerializeField] List<GameObject> enemies;
        [SerializeField] Transform path;
        [SerializeField] float _spawnInterval = 1f;
        [SerializeField][Range(0f, 2f)] float spawnTimeVariance = 0f;
        [SerializeField] float _minSpawnInterval = 1f;

        // getters
        public int enemyCount => enemies.Count;
        public float spawnInterval => GetSpawnInterval();

        float GetSpawnInterval() {
            return Mathf.Max(
                _minSpawnInterval,
                _spawnInterval + UnityEngine.Random.Range(-spawnTimeVariance, spawnTimeVariance)
            );
        }

        public GameObject GetEnemy(int index) {
            return enemies[index];
        }

        public Transform GetStartingWaypoint() {
            return path.GetChild(0);
        }

        public List<Transform> GetWaypoints() {
            var waypoints = new List<Transform>();
            foreach (Transform child in path)
            {
                waypoints.Add(child);
            }
            return waypoints;
        }
    }
}

