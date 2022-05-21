using System.Collections.Generic;
using Core;
using Game;
using UnityEngine;

namespace Enemies
{

    [System.Serializable]
    public class WaveEnemy
    {
        [SerializeField] GameObject _enemyPrefab;
        [SerializeField] string _spawnLocation;
        [SerializeField] Vector2 _spawnOffset = Vector3.zero;
        [SerializeField][Tooltip("Spawn at or above selected difficulty")] GameDifficulty _spawnDifficulty = GameDifficulty.Easy;

        public GameObject prefab => _enemyPrefab;
        public string spawnLocation => _spawnLocation;
        public Vector3 spawnOffset => _spawnOffset;
        public GameDifficulty spawnDifficulty => _spawnDifficulty;

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
        [SerializeField][Range(0f, 10f)] float _spawnInterval = 1f;
        [SerializeField][Range(0f, 2f)] float _spawnTimeVariance = 0f;
        [SerializeField][Range(0f, 10f)] float _minSpawnInterval = 1f;
        [SerializeField][Range(1, 20)] int _numLoops = 1;
        [SerializeField][Range(0f, 20f)] float _loopInterval = 0f;

        [Header("Spawning")]
        [Space]
        [SerializeField] string _spawnLocation;
        [SerializeField] Vector2 _spawnOffset = Vector3.zero;
        [SerializeField][Tooltip("Spawn at or above selected difficulty")] GameDifficulty _spawnDifficulty = GameDifficulty.Easy;

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
        [SerializeField] int _maxPathLoops = 99;

        [Header("FSM")]
        [Space]
        [SerializeField] FSM.BaseState _initialState;

        [TextArea(minLines: 3, maxLines: 10)]
        public string notes;

        // getters
        public GameDifficulty spawnDifficulty => _spawnDifficulty;
        public int spawnCount => GetSpawnCount();
        public int enemyCount => GetEnemyCount();
        public float spawnInterval => GetSpawnInterval();
        public int numLoops => GetLoopCount();
        public float loopInterval => _loopInterval;
        public Transform path => _path;
        public FSM.BaseState initialState => _initialState;
        public PathfinderLoopMode pathfinderLoopMode => _pathfinderLoopMode;
        public bool flipX => _flipX;
        public bool flipY => _flipY;
        public int maxPathLoops => _maxPathLoops;

        // cached
        string[] locationParts = new string[3];
        Vector2 minBounds;
        Vector2 maxBounds;
        Vector2 tempSpawnLocation;
        int numEnemies = 0;

        // constants
        const float SPAWN_GRID_SIZE = 2f;

        float GetSpawnInterval()
        {
            return Mathf.Max(
                _minSpawnInterval,
                _spawnInterval + UnityEngine.Random.Range(-_spawnTimeVariance, _spawnTimeVariance)
            );
        }

        int GetSpawnCount() {
            if (GameManager.current.difficulty < _spawnDifficulty) return 0;
            return _enemies.Count;
        }

        int GetEnemyCount() {
            if (GameManager.current.difficulty < _spawnDifficulty) return 0;
            if (numEnemies > 0) return numEnemies;
            numEnemies = 0;
            foreach (var enemy in _enemies) {
                if (GameManager.current.difficulty < enemy.spawnDifficulty) continue;
                // evaluating the tag here worked in most cases, but not for enemy groups like turrets or boss stations
                // if (enemy.prefab.tag == UTag.EnemyShip || enemy.prefab.tag == UTag.EnemyTurret || enemy.prefab.tag == UTag.Boss) numEnemies++;
                int count = enemy.prefab.GetComponentsInChildren<EnemyShip>().Length;
                numEnemies += count;
            }
            return numEnemies;
        }

        int GetLoopCount() {
            if (GameManager.current.difficulty < _spawnDifficulty) return 0;
            return Mathf.Max(_numLoops, 1);
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
            return GetLaunchHeading(origin, playerPosition) * Utils.RandomVariance2(launchVelocity, velocityVariance, launchVelocity / 2f);
        }

        Vector3 GetLaunchHeading(Vector3 origin, Vector3 playerPosition) {
            Vector3 headingTowardsPlayer = (playerPosition - origin).normalized;
            return GetLaunchHeadingVariance() * Vector3.Lerp(launchHeading, headingTowardsPlayer, aimTowardsPlayer);
        }

        Quaternion GetLaunchHeadingVariance() {
            return Quaternion.AngleAxis(UnityEngine.Random.Range(-headingVariance / 2f, headingVariance / 2f), Vector3.forward);
        }

        int ToInt(string val) {
            return System.Int16.Parse(val);
        }

        float ToFloat(string val) {
            return float.Parse(val);
        }
    }
}

