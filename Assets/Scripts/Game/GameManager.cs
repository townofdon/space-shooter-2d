
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

using Core;
using Damage;
using Weapons;
using Event;
using Player;
using Audio;
using Dialogue;
using UI;

namespace Game {

    public class GameManager : MonoBehaviour {
        [Header("General")]
        [Space]
        [SerializeField] float respawnWaitTime = 2f;
        [SerializeField] float winLevelWaitTime = 5f;
        [SerializeField] int levelWonPoints = 10000;
        [SerializeField] EventChannelSO eventChannel;
        [SerializeField] GameStateSO gameState;
        [SerializeField] LevelManager levelManager;
        [SerializeField] ParticleSystem starfieldWarp;
        [SerializeField] GameObject starBG;
        [SerializeField] GameFX gameFX;

        [Header("Xtra Life")]
        [Space]
        [SerializeField] int xtraLifeForEnemiesKilled = 1000;
        [SerializeField] Transform xtraLifeSpawnPoint;
        [SerializeField] GameObject xtraLifeRed;
        [SerializeField] GameObject xtraLifeYellow;
        [SerializeField] GameObject xtraLifeBlue;
        [SerializeField] GameObject xtraLifeGreen;

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
        PlayerInputHandler inputHandler;
        Coroutine ieRespawn;
        Coroutine ieLose;
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

        public void NewGame() {
            timeElapsed = 0f;
            playerState.Init();
            gameState.NewGame();
            ResetWeaponUpgrades();
            foreach (var weapon in _weaponClasses) weapon.SetToStartingAmmo();
        }

        public void SetDifficulty(GameDifficulty difficulty) {
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
            OnUnpause();
            AudioManager.current.StopTrack();
            foreach (var weapon in _weaponClasses) weapon.SetToStartingAmmo();
            HideWarpFX();
            if (levelManager.IsOnTutorialLevel() && gameMode == GameMode.Campaign) ResetWeaponUpgrades();
            gameState.SetHasInfiniteLives(false);
            // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1 + skip);
            levelManager.GotoNextLevel(gameMode == GameMode.Arcade);
        }

        public void GotoUpgradeScene() {
            OnUnpause();
            AudioManager.current.StopTrack();
            HideWarpFX();
            levelManager.GotoUpgradeScene();
        }

        public void GotoMainMenu() {
            OnUnpause();
            NewGame();
            AudioManager.current.StopTrack();
            HideWarpFX();
            // SceneManager.LoadScene("MainMenu");
            levelManager.GotoMainMenu();
        }

        public void GotoLevelOne(bool skipToLevel2 = false) {
            OnUnpause();
            NewGame();
            StartGameTimer();
            AudioManager.current.StopTrack();
            HideWarpFX();
            if (difficulty >= GameDifficulty.Hard || skipToLevel2 || gameMode == GameMode.Arcade) {
                gameState.SetHasInfiniteLives(false);
                levelManager.GotoLevelOne();
            } else {
                gameState.SetHasInfiniteLives(true);
                levelManager.GotoTutorialLevel();
            }
        }

        public void StartGameTimer() {
            timerActive = true;
        }
        public void StopGameTimer() {
            timerActive = false;
        }

        public void DestroyAllEnemies(bool isQuiet = false) {
            ImperativelyDestroyAllEnemies(isQuiet);
            eventChannel.OnDestroyAllEnemies.Invoke();
        }

        public void OnFocusSound() {
            AudioManager.current.PlaySound("MenuFocus");
        }

        void OnEnable() {
            eventChannel.OnSpawnXtraLife.Subscribe(OnSpawnXtraLife);
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
            eventChannel.OnXtraLife.Subscribe(OnXtraLife);
            eventChannel.OnGotoMainMenu.Subscribe(OnGotoMainMenu);
        }

        void OnDisable() {
            eventChannel.OnSpawnXtraLife.Unsubscribe(OnSpawnXtraLife);
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
            eventChannel.OnXtraLife.Unsubscribe(OnXtraLife);
            eventChannel.OnGotoMainMenu.Unsubscribe(OnGotoMainMenu);
        }

        void Awake() {
            _current = Utils.ManageSingleton<GameManager>(_current, this);
            SetupDamageClasses();
            SetupWeaponClasses();
            ULayer.Init();
        }

        void Start() {
            timeElapsed = 0f;
            playerState.Init();
            gameState.Init();
            ResetWeaponUpgrades();
            NewGame();
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

        void ResetWeaponUpgrades(bool forceMaxUpgrades = false) {
            if (gameMode == GameMode.Demo || forceMaxUpgrades) {
                playerState.UpgradeGuns();
                foreach (var weapon in _weaponClasses) weapon.ResetWithMaxUpgrade();
            } else {
                playerState.ResetGunsUpgrade();
                foreach (var weapon in _weaponClasses) weapon.Reset();
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
            gameState.LosePoints();
            gameState.LoseLife();
            AudioManager.current.PlaySound("ship-death");
            if (ieRespawn != null) StopCoroutine(ieRespawn);
            if (ieLose != null) StopCoroutine(ieLose);
            if (ieWin != null) StopCoroutine(ieWin);
            if (gameState.lives <= 0) {
                ieLose = StartCoroutine(ILoseLevel());
            } else {
                ieRespawn = StartCoroutine(IRespawn());
            }
        }

        void OnPlayerTakeMoney(float value) {
            playerState.GainMoney((int)value);
        }

        void OnXtraLife() {
            gameState.GainLife();
        }

        void OnEnemyDeath(int instanceId, int points, bool isCountableEnemy = true) {
            gameState.IncrementEnemiesKilled();
            gameState.GainPoints(points);
            // spawn xtra life on each N enemy killed
            if (gameState.numEnemiesKilled > 0 && gameState.numEnemiesKilled % xtraLifeForEnemiesKilled == 0) {
                OnSpawnXtraLife();
            }
        }

        void OnSpawnXtraLife() {
            Instantiate(GetXtraLifePrefab(), xtraLifeSpawnPoint.position, Quaternion.identity);
        }

        void OnWinLevel(bool showUpgradePanel = true) {
            AudioManager.current.StopTrack();
            AudioManager.current.CueTrack("win-theme");
            StopGameTimer();
            gameState.GainPoints(levelWonPoints);
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
            StopGameTimer();
        }

        void OnUnpause() {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            GameManager.isPaused = false;
            StartGameTimer();
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

        void HideWarpFX() {
            starfieldWarp.Stop();
            starBG.SetActive(true);
            foreach (var bg in Object.FindObjectsOfType<BackgroundScroller>()) bg.RestoreScroll();
        }

        void HandleRespawnPlayerShip(Transform respawnTarget) {
            GameObject playerShip = Instantiate(GetPlayerShipPrefab(), playerRespawnPoint.position, Quaternion.identity);
            PlayerInputHandler input = playerShip.GetComponent<PlayerInputHandler>();
            input.SetMode(PlayerInputControlMode.GameBrain);
            input.SetAutoMoveTarget(respawnTarget);
        }

        void DisablePlayerInput() {
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            PlayerInput input = player.GetComponent<PlayerInput>();
            // input.SwitchCurrentActionMap("UI");
            // Destroy the input handler so that the newly-spawned item will gain control (hopefully)
            if (movement != null) { movement.enabled = false; }
            if (input != null) { input.enabled = false; }
            if (inputHandler != null) { inputHandler.enabled = false; }
        }

        IEnumerator SetPlayerModeMoveOffscreen() {
            while (player == null || inputHandler == null || !player.isAlive) {
                player = PlayerUtils.FindPlayer();
                if (player != null) inputHandler = player.GetComponent<PlayerInputHandler>();
                yield return null;
            }

            inputHandler.SetMode(PlayerInputControlMode.GameBrain);
            inputHandler.SetAutoMoveTarget(playerExitTarget);
        }

        IEnumerator IRespawn() {
            yield return new WaitForSeconds(respawnWaitTime);
            AudioManager.current.PlaySound("ship-respawn");
            HandleRespawnPlayerShip(playerRespawnTarget);
        }

        IEnumerator ILoseLevel() {
            yield return new WaitForSeconds(respawnWaitTime);
            levelManager.GotoWinLoseScreen();
        }

        IEnumerator IWinLevel(bool showUpgradePanel = true) {
            eventChannel.OnShowVictory.Invoke();
            ImperativelyDestroyAllEnemies();
            while (player == null) {
                player = PlayerUtils.FindPlayer();
                yield return null;
            }
            player.SetInvulnerable(true);
            yield return new WaitForSeconds(winLevelWaitTime);
            eventChannel.OnHideVictory.Invoke();
            AudioManager.current.PlaySound("ship-whoosh");
            yield return SetPlayerModeMoveOffscreen();
            yield return gameFX.Warp();

            while (player == null || !player.isAlive || Utils.IsObjectOnScreen(player.gameObject)) {
                yield return SetPlayerModeMoveOffscreen();
            }

            DisablePlayerInput();
            player.gameObject.SetActive(false);

            if (showUpgradePanel && gameState.mode == GameMode.Campaign) {
                GotoUpgradeScene();
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

        GameObject GetXtraLifePrefab() {
            switch (playerState.shipColor) {
                case PlayerShipColor.Red:
                    return xtraLifeRed;
                case PlayerShipColor.Yellow:
                    return xtraLifeYellow;
                case PlayerShipColor.Blue:
                    return xtraLifeBlue;
                case PlayerShipColor.Green:
                    return xtraLifeGreen;
                default:
                    return xtraLifeRed;
            }
        }
    }
}
