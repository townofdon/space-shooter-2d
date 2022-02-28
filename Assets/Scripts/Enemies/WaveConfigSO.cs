using System.Collections.Generic;
using UnityEngine;

namespace Enemies {

    [System.Serializable]
    public class WaveEnemy {
        [SerializeField] GameObject _enemyPrefab;
        [SerializeField] Vector3 _spawnOffset = Vector3.zero;
        public GameObject prefab { get => _enemyPrefab; }
        public Vector3 spawnOffset { get => _spawnOffset; }
    }

    [CreateAssetMenu(menuName = "WaveConfig", fileName = "ScriptableObjects/WaveConfig", order = 0)]
    public class WaveConfigSO : ScriptableObject
    {
        public enum Mode {
            Spawn,
            FollowPath,
        }

        [Header("General Settings")][Space]
        [SerializeField] List<WaveEnemy> _enemies;
        [SerializeField] float _spawnInterval = 1f;
        [SerializeField][Range(0f, 2f)] float _spawnTimeVariance = 0f;
        [SerializeField] float _minSpawnInterval = 1f;

        [Header("Mode")][Space]
        [SerializeField] Mode _mode;

        [Header("Spawn Mode")][Space]
        [SerializeField] Transform _spawnLocation;

        [Header("Spawn Offset (applies to all modes)")][Space]
        [SerializeField] Vector3 _offset = Vector3.zero;

        [Header("PathFollow Mode")][Space]
        [SerializeField] Transform _path;
        [SerializeField] PathfinderLoopMode _pathfinderLoopMode;

        [Header("FSM")][Space]
        [SerializeField] FSM.BaseState _initialState;

        // getters
        public Mode mode => _mode;
        public int enemyCount => _enemies.Count;
        public float spawnInterval => GetSpawnInterval();
        public Transform path => _path;
        public FSM.BaseState initialState => _initialState;
        public PathfinderLoopMode pathfinderLoopMode => _pathfinderLoopMode;

        float GetSpawnInterval() {
            return Mathf.Max(
                _minSpawnInterval,
                _spawnInterval + UnityEngine.Random.Range(-_spawnTimeVariance, _spawnTimeVariance)
            );
        }

        public WaveEnemy GetEnemy(int index) {
            return _enemies[index];
        }

        public Vector3 GetSpawnPosition() {
            if (mode == Mode.Spawn) {
                if (_spawnLocation == null) return Vector3.zero;
                return _spawnLocation.position + _offset;
            }
            if (mode == Mode.FollowPath) {
                Transform waypoint = GetStartingWaypoint();
                if (waypoint == null) return Vector3.zero;
                return waypoint.position + _offset;
            }

            return Vector3.zero;
        }

        public List<Transform> GetWaypoints() {
            if (mode != Mode.FollowPath) return new List<Transform>();
            var waypoints = new List<Transform>();
            foreach (Transform child in _path)
            {
                waypoints.Add(child);
            }
            return waypoints;
        }

        Transform GetStartingWaypoint() {
            return _path.GetChild(0);
        }
    }
}

