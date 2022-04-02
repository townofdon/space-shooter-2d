
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Core;
using Damage;
using Weapons;
using Event;
using Player;
using Audio;

namespace Game {

    public class GameManager : MonoBehaviour

    {
        [Header("General")]
        [Space]
        [SerializeField] float respawnWaitTime = 2f;
        [SerializeField] float winLevelWaitTime = 5f;
        [SerializeField] EventChannelSO eventChannel;
        [SerializeField] GameStateSO gameState;

        [Header("Player")]
        [Space]
        [SerializeField] PlayerStateSO playerState;
        [SerializeField] GameObject redShipPrefab;
        [SerializeField] GameObject yellowShipPrefab;
        [SerializeField] GameObject blueShipPrefab;
        [SerializeField] GameObject greenShipPrefab;
        [SerializeField] Transform playerRespawnPoint;
        [SerializeField] Transform playerRespawnTarget;
        [SerializeField] Transform playerExitTarget;

        [Header("Global Classes")]
        [Space]
        [SerializeField] List<DamageClass> _damageClasses = new List<DamageClass>();
        Dictionary<DamageType, DamageClass> _damageClassLookup = new Dictionary<DamageType, DamageClass>();
        [SerializeField] List<WeaponClass> _weaponClasses = new List<WeaponClass>();
        Dictionary<WeaponType, WeaponClass> _weaponClassLookup = new Dictionary<WeaponType, WeaponClass>();

        // cached
        PlayerGeneral player;
        Coroutine ieRespawn;
        Coroutine ieWin;

        // state
        public static bool isPaused = false;

        // singleton
        static GameManager _current;
        public static GameManager current => _current;

        // public
        public GameMode gameMode => gameState.mode;
        public GameDifficulty difficulty => gameState.difficulty;
        public DamageClass GetDamageClass(DamageType damageType) {
            return _damageClassLookup[damageType];
        }
        public WeaponClass GetWeaponClass(WeaponType weaponType) {
            return _weaponClassLookup[weaponType];
        }

        public static void Cleanup() {
            Utils.CleanupSingleton<GameManager>(_current);
        }

        public void SetDifficulty(GameDifficulty difficulty) {
            Debug.Log("Setting difficulty to: " + difficulty);
            gameState.SetDifficulty(difficulty);
        }

        public void SetPlayerShipColor(PlayerShipColor value) {
            playerState.SetShipColor(value);
        }

        public void RespawnPlayerShip(Transform respawnTarget) {
            HandleRespawnPlayerShip(respawnTarget);
        }

        public void RespawnPlayerShip() {
            HandleRespawnPlayerShip(playerRespawnTarget);
        }

        public void GotoNextLevel() {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        void OnEnable() {
            eventChannel.OnPlayerDeath.Subscribe(OnPlayerDeath);
            eventChannel.OnEnemyDeath.Subscribe(OnEnemyDeath);
            eventChannel.OnWinLevel.Subscribe(OnWinLevel);
            eventChannel.OnPause.Subscribe(OnPause);
            eventChannel.OnUnpause.Subscribe(OnUnpause);
            eventChannel.OnShowDebug.Subscribe(OnShowDebug);
            eventChannel.OnHideDebug.Subscribe(OnHideDebug);
        }

        void OnDisable() {
            eventChannel.OnPlayerDeath.Unsubscribe(OnPlayerDeath);
            eventChannel.OnEnemyDeath.Unsubscribe(OnEnemyDeath);
            eventChannel.OnWinLevel.Unsubscribe(OnWinLevel);
            eventChannel.OnPause.Unsubscribe(OnPause);
            eventChannel.OnUnpause.Unsubscribe(OnUnpause);
            eventChannel.OnShowDebug.Unsubscribe(OnShowDebug);
            eventChannel.OnHideDebug.Unsubscribe(OnHideDebug);
        }

        void Awake() {
            _current = Utils.ManageSingleton<GameManager>(_current, this);
            SetupDamageClasses();
            SetupWeaponClasses();
            ULayer.Init();
        }

        void Start() {
            playerState.Init();
            gameState.Init();
        }

        void SetupDamageClasses() {
            foreach (var damageClass in _damageClasses) {
                _damageClassLookup.Add(damageClass.damageType, damageClass);
            }
            foreach(DamageType damageType in System.Enum.GetValues(typeof(DamageType))) {
                try {
                    // note - the dictionary lookup will fail if missing
                    _damageClassLookup[damageType] = _damageClassLookup[damageType];
                } catch (System.Exception e) {
                    Debug.LogException(e);
                }
            }
        }

        void SetupWeaponClasses() {
            foreach (var weaponClass in _weaponClasses) {
                _weaponClassLookup.Add(weaponClass.type, weaponClass);
            }
            foreach(WeaponType weaponType in System.Enum.GetValues(typeof(WeaponType))) {
                try {
                    _weaponClassLookup[weaponType] = _weaponClassLookup[weaponType];
                } catch (System.Exception e) {
                    Debug.LogException(e);
                }
            }
        }

        void OnPlayerDeath() {
            playerState.IncrementDeaths();
            AudioManager.current.PlaySound("ship-death");
            if (ieRespawn != null) StopCoroutine(ieRespawn);
            if (ieWin != null) StopCoroutine(ieWin);
            ieRespawn = StartCoroutine(IRespawn());
        }

        void OnEnemyDeath(int instanceId, int points) {
            gameState.GainPoints(points);
        }

        void OnWinLevel() {
            // TODO: SHOW VICTORY UI
            Debug.Log("VICTORY!!");
            gameState.StorePoints();
            if (ieWin != null) StopCoroutine(ieWin);
            ieWin = StartCoroutine(IWinLevel());
        }

        void OnPause() {
            Time.timeScale = 0f;
            AudioListener.pause = true; // see: https://gamedevbeginner.com/10-unity-audio-tips-that-you-wont-find-in-the-tutorials/#audiolistener_pause
            GameManager.isPaused = true;
        }

        void OnUnpause() {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            GameManager.isPaused = false;
        }

        void OnShowDebug() {
            OnPause();
        }

        void OnHideDebug() {
            OnUnpause();
        }

        void HandleRespawnPlayerShip(Transform respawnTarget) {
            GameObject playerShip = Instantiate(GetPlayerShipPrefab(), playerRespawnPoint.position, Quaternion.identity);
            PlayerInputHandler input = playerShip.GetComponent<PlayerInputHandler>();
            input.SetMode(PlayerControlMode.ByGame);
            input.SetAutoMoveTarget(respawnTarget);
        }

        IEnumerator IRespawn() {
            yield return new WaitForSeconds(respawnWaitTime);
            AudioManager.current.PlaySound("ship-respawn");
            HandleRespawnPlayerShip(playerRespawnTarget);
        }

        IEnumerator IWinLevel() {
            yield return new WaitForSeconds(winLevelWaitTime);
            while (player == null) {
                player = PlayerUtils.FindPlayer();
                yield return null;
            }
            AudioManager.current.PlaySound("ship-whoosh");
            PlayerInputHandler input = player.GetComponent<PlayerInputHandler>();
            input.SetMode(PlayerControlMode.ByGame);
            input.SetAutoMoveTarget(playerExitTarget);
            while (Utils.IsObjectOnScreen(player.gameObject)) yield return null;
            GotoNextLevel();
        }

        GameObject GetPlayerShipPrefab() {
            switch (playerState.shipColor) {
                case PlayerShipColor.Red:
                    return redShipPrefab;
                case PlayerShipColor.Yellow:
                    return yellowShipPrefab;
                case PlayerShipColor.Blue:
                    return blueShipPrefab;
                case PlayerShipColor.Green:
                    return greenShipPrefab;
                default:
                    return redShipPrefab;
            }
        }
    }
}
