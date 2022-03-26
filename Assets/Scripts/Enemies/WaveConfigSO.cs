using System.Collections.Generic;
using Core;
using UnityEngine;

namespace Enemies
{

    [System.Serializable]
    public class WaveEnemy
    {
        [SerializeField] GameObject _enemyPrefab;
        [SerializeField] string _spawnLocation;
        [SerializeField] Vector2 _spawnOffset = Vector3.zero;

        public GameObject prefab => _enemyPrefab;
        public string spawnLocation => _spawnLocation;
        public Vector3 spawnOffset => _spawnOffset;

        public bool hasSpawnLocation => _spawnLocation != null && _spawnLocation != "";
    }

    [CreateAssetMenu(menuName = "WaveConfig", fileName = "ScriptableObjects/WaveConfig", order = 0)]
    public class WaveConfigSO : ScriptableObject
    {
        public enum Mode
        {
            Spawn,
            FollowPath,
        }

        [Header("General Settings")]
        [Space]
        [SerializeField] List<WaveEnemy> _enemies;
        [SerializeField] float _spawnInterval = 1f;
        [SerializeField] [Range(0f, 2f)] float _spawnTimeVariance = 0f;
        [SerializeField] float _minSpawnInterval = 1f;

        [Header("Spawning")]
        [Space]
        [SerializeField] string _spawnLocation;
        [SerializeField] Vector2 _spawnOffset = Vector3.zero;

        [Header("Launch")]
        [Space]
        [SerializeField] Vector2 launchHeading = Vector3.down;
        [SerializeField][Range(0f, 180f)] float headingVariance = 0f;
        [SerializeField][Range(0f, 40f)] float launchVelocity = 0f;
        [SerializeField][Range(0f, 20f)] float velocityVariance = 0f;
        [SerializeField][Range(0f, 1f)] float aimTowardsPlayer = 0f;

        [Header("Path")]
        [Space]
        [SerializeField] Transform _path;
        [SerializeField] PathfinderLoopMode _pathfinderLoopMode;
        [SerializeField] bool _flipX;
        [SerializeField] bool _flipY;

        [Header("FSM")]
        [Space]
        [SerializeField] FSM.BaseState _initialState;

        // getters
        public int spawnCount => _enemies.Count;
        public int enemyCount => GetEnemyCount();
        public float spawnInterval => GetSpawnInterval();
        public Transform path => _path;
        public FSM.BaseState initialState => _initialState;
        public PathfinderLoopMode pathfinderLoopMode => _pathfinderLoopMode;
        public bool flipX => _flipX;
        public bool flipY => _flipY;

        // cached
        string[] locationParts = new string[3];
        Vector2 minBounds;
        Vector2 maxBounds;
        Vector2 tempSpawnLocation;

        // constants
        const float SPAWN_GRID_SIZE = 2f;

        float GetSpawnInterval()
        {
            return Mathf.Max(
                _minSpawnInterval,
                _spawnInterval + UnityEngine.Random.Range(-_spawnTimeVariance, _spawnTimeVariance)
            );
        }

        int GetEnemyCount() {
            int count = 0;
            foreach (var enemy in _enemies) {
                if (enemy.prefab.tag == UTag.EnemyShip) count++;
            }
            return count;
        }

        public WaveEnemy GetEnemy(int index)
        {
            return _enemies[index];
        }

        public Vector3 GetSpawnPosition() {
            if (_spawnLocation == null || _spawnLocation == "") return Vector3.zero;
            return ParseSpawnLocation(_spawnLocation) + (Vector2)_spawnOffset;

            // if (mode == Mode.Spawn)
            // {
            //     if (_spawnLocation == null) return Vector3.zero;
            //     return _spawnLocation.position + _spawnOffset;
            // }
            // if (mode == Mode.FollowPath)
            // {
            //     Transform waypoint = GetStartingWaypoint();
            //     if (waypoint == null) return Vector3.zero;
            //     return waypoint.position + _spawnOffset;
            // }

            // return Vector3.zero;
        }

        public bool HasPath() {
            return _path != null;
        }

        public List<Transform> GetWaypoints()
        {
            if (_path == null) return new List<Transform>();
            var waypoints = new List<Transform>();
            foreach (Transform child in _path)
            {
                waypoints.Add(child);
            }
            return waypoints;
        }

        Transform GetStartingWaypoint()
        {
            return _path.GetChild(0);
        }

        public Vector2 ParseSpawnLocation(string spawnLocation) {
            (minBounds, maxBounds) = Utils.GetScreenBounds(null, 0f);
            tempSpawnLocation = Vector2.zero;
            if (spawnLocation == null || spawnLocation == "") return tempSpawnLocation;
            locationParts = spawnLocation.Split("-", 3);

            if (locationParts[0] == "N") {
                // place obj at top-left corner of screen
                tempSpawnLocation = new Vector2(minBounds.x, maxBounds.y);
                tempSpawnLocation += Vector2.right * (maxBounds.x * 2f * ToFloat(locationParts[1]) / 24f);
                tempSpawnLocation += Vector2.up * ToFloat(locationParts[2]) * SPAWN_GRID_SIZE;
            } else if (locationParts[0] == "W") {
                // place obj at top-left corner of screen
                tempSpawnLocation = new Vector2(minBounds.x, maxBounds.y);
                tempSpawnLocation += Vector2.down * (maxBounds.y * 2f * ToFloat(locationParts[1]) / 24f);
                tempSpawnLocation += Vector2.left * ToFloat(locationParts[2]) * SPAWN_GRID_SIZE;
            } else if (locationParts[0] == "E") {
                // place obj at top-right corner of screen
                tempSpawnLocation = new Vector2(maxBounds.x, maxBounds.y);
                tempSpawnLocation += Vector2.down * (maxBounds.y * 2f * ToFloat(locationParts[1]) / 24f);
                tempSpawnLocation += Vector2.right * ToFloat(locationParts[2]) * SPAWN_GRID_SIZE;
            } else if (locationParts[0] == "S") {
                // place obj at bottom-left corner of screen
                tempSpawnLocation = new Vector2(minBounds.x, minBounds.y);
                tempSpawnLocation += Vector2.right * (maxBounds.x * 2f * ToFloat(locationParts[1]) / 24f);
                tempSpawnLocation += Vector2.down * ToFloat(locationParts[2]) * SPAWN_GRID_SIZE;
            }
            return tempSpawnLocation;
        }

        public Vector3 GetLaunchVelocity(Vector3 origin, Vector3 playerPosition) {
            return GetLaunchHeading(origin, playerPosition) * Utils.RandomVariance(launchVelocity, velocityVariance, launchVelocity / 2f);
        }

        Vector3 GetLaunchHeading(Vector3 origin, Vector3 playerPosition) {
            Vector3 headingTowardsPlayer = (playerPosition - origin).normalized;
            return GetLaunchHeadingVariance() * Vector3.Lerp(launchHeading, headingTowardsPlayer, aimTowardsPlayer);
        }

        Quaternion GetLaunchHeadingVariance() {
            return Quaternion.AngleAxis(Utils.RandomVariance(0f, headingVariance), Vector3.forward);
        }

        int ToInt(string val) {
            return System.Int16.Parse(val);
        }

        float ToFloat(string val) {
            return float.Parse(val);
        }
    }
}

