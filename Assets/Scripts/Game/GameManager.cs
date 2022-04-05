
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

using Core;
using Damage;
using Weapons;
using Event;
using Player;
using Audio;
using Dialogue;

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
        public float timeElapsed = 0f;
        bool timerActive = false;

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

        public void ShowUpgradePanel() {
            eventChannel.OnShowUpgradePanel.Invoke();
        }

        public void GotoNextLevel() {
            GameManager.isPaused = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        public void GotoMainMenu() {
            GameManager.isPaused = false;
            Time.timeScale = 1f;
            AudioManager.current.StopTrack();
            SceneManager.LoadScene("MainMenu");
        }

        public void StartGameTimer() {
            timerActive = true;
        }
        public void StopGameTimer() {
            timerActive = false;
        }

        public void DestroyAllEnemies(bool isQuiet = false) {
            ImperativelyDestroyAllEnemies(isQuiet);
        }

        public void OnFocusSound() {
            AudioManager.current.PlaySound("MenuFocus");
        }

        void OnEnable() {
            eventChannel.OnPlayerDeath.Subscribe(OnPlayerDeath);
            eventChannel.OnEnemyDeath.Subscribe(OnEnemyDeath);
            eventChannel.OnWinLevel.Subscribe(OnWinLevel);
            eventChannel.OnPause.Subscribe(OnPause);
            eventChannel.OnUnpause.Subscribe(OnUnpause);
            eventChannel.OnShowDebug.Subscribe(OnShowDebug);
            eventChannel.OnHideDebug.Subscribe(OnHideDebug);
            eventChannel.OnShowDialogue.Subscribe(OnShowDialogue);
            eventChannel.OnDismissDialogue.Subscribe(OnDismissDialogue);
            eventChannel.OnPlayerTakeMoney.Subscribe(OnPlayerTakeMoney);
            eventChannel.OnGotoMainMenu.Subscribe(OnGotoMainMenu);

        }

        void OnDisable() {
            eventChannel.OnPlayerDeath.Unsubscribe(OnPlayerDeath);
            eventChannel.OnEnemyDeath.Unsubscribe(OnEnemyDeath);
            eventChannel.OnWinLevel.Unsubscribe(OnWinLevel);
            eventChannel.OnPause.Unsubscribe(OnPause);
            eventChannel.OnUnpause.Unsubscribe(OnUnpause);
            eventChannel.OnShowDebug.Unsubscribe(OnShowDebug);
            eventChannel.OnHideDebug.Unsubscribe(OnHideDebug);
            eventChannel.OnShowDialogue.Unsubscribe(OnShowDialogue);
            eventChannel.OnDismissDialogue.Unsubscribe(OnDismissDialogue);
            eventChannel.OnPlayerTakeMoney.Unsubscribe(OnPlayerTakeMoney);
            eventChannel.OnGotoMainMenu.Unsubscribe(OnGotoMainMenu);
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

        void Update() {
            if (timerActive) {
                timeElapsed += Time.deltaTime;
            }
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

        void ImperativelyDestroyAllEnemies(bool isQuiet = false) {
            Enemies.EnemyShip[] enemies = FindObjectsOfType<Enemies.EnemyShip>();
            foreach (var enemy in enemies) {
                if (enemy == null || !enemy.isAlive) continue;
                enemy.OnDeathByGuardians(isQuiet);
            }
            GameObject[] asteroids = GameObject.FindGameObjectsWithTag(UTag.Asteroid);
            foreach (var asteroid in asteroids) {
                DamageableBehaviour actor = asteroid.GetComponent<DamageableBehaviour>();
                if (actor != null) actor.OnDeathByGuardians(isQuiet);
                else Destroy(asteroid);
            }
            foreach (var mine in GameObject.FindGameObjectsWithTag(UTag.Mine)) {
                DamageableBehaviour actor = mine.GetComponent<DamageableBehaviour>();
                if (actor != null) actor.OnDeathByGuardians(isQuiet);
                else Destroy(mine);
            }
        }

        void OnPlayerDeath() {
            playerState.IncrementDeaths();
            AudioManager.current.PlaySound("ship-death");
            if (ieRespawn != null) StopCoroutine(ieRespawn);
            if (ieWin != null) StopCoroutine(ieWin);
            ieRespawn = StartCoroutine(IRespawn());
        }

        void OnPlayerTakeMoney(float value) {
            playerState.GainMoney((int)value);
        }

        void OnEnemyDeath(int instanceId, int points) {
            gameState.IncrementEnemiesKilled();
            gameState.GainPoints(points);
        }

        void OnWinLevel(bool showUpgradePanel = true) {
            AudioManager.current.StopTrack();
            AudioManager.current.CueTrack("win-theme");
            StopGameTimer();
            gameState.StorePoints();
            if (ieWin != null) StopCoroutine(ieWin);
            ieWin = StartCoroutine(IWinLevel(showUpgradePanel));
        }

        void OnGotoMainMenu() {
            GotoMainMenu();
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

        void OnShowDialogue(DialogueItemSO item) {
            // playerState.SetInputControlMode(PlayerInputControlMode.Disabled);
        }

        void OnDismissDialogue() {
            // playerState.SetInputControlMode(PlayerInputControlMode.Player);
        }

        void HandleRespawnPlayerShip(Transform respawnTarget) {
            GameObject playerShip = Instantiate(GetPlayerShipPrefab(), playerRespawnPoint.position, Quaternion.identity);
            PlayerInputHandler input = playerShip.GetComponent<PlayerInputHandler>();
            input.SetMode(PlayerInputControlMode.GameBrain);
            input.SetAutoMoveTarget(respawnTarget);
        }

        IEnumerator IRespawn() {
            yield return new WaitForSeconds(respawnWaitTime);
            AudioManager.current.PlaySound("ship-respawn");
            HandleRespawnPlayerShip(playerRespawnTarget);
        }

        IEnumerator IWinLevel(bool showUpgradePanel = true) {
            eventChannel.OnShowVictory.Invoke();
            ImperativelyDestroyAllEnemies();
            yield return new WaitForSeconds(winLevelWaitTime);
            while (player == null) {
                player = PlayerUtils.FindPlayer();
                yield return null;
            }
            player.SetInvulnerable(true);
            eventChannel.OnHideVictory.Invoke();
            AudioManager.current.PlaySound("ship-whoosh");
            PlayerInputHandler inputHandler = player.GetComponent<PlayerInputHandler>();
            while (Utils.IsObjectOnScreen(player.gameObject)) {
                inputHandler.SetMode(PlayerInputControlMode.GameBrain);
                inputHandler.SetAutoMoveTarget(playerExitTarget);
                yield return null;
            }

            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            PlayerInput input = player.GetComponent<PlayerInput>();
            // input.SwitchCurrentActionMap("UI");
            // Destroy the input handler so that the newly-spawned item will gain control (hopefully)
            if (movement != null) { movement.enabled = false; }
            if (input != null) { input.enabled = false; }
            if (inputHandler != null) { inputHandler.enabled = false; }

            player.gameObject.SetActive(false);

            if (showUpgradePanel) {
                ShowUpgradePanel();
            } else {
                GotoNextLevel();
            }
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
