
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Core;
using Damage;
using Weapons;
using Event;
using System.Collections;

namespace Game {

    public enum GameMode {
        Battle,
        FreeRoam,
        Docked,
    }

    struct GameState {
        GameMode _mode;

        public GameState(GameMode mode = GameMode.Battle) : this() {
            _mode = mode;
        }

        public GameMode mode { get; set; }
    }

    public class GameManager : MonoBehaviour

    {
        [Header("General")]
        [Space]
        [SerializeField] float pauseSlowdownDuration = 0.4f;
        [SerializeField] float respawnWaitTime = 0.4f;
        [SerializeField] EventChannelSO eventChannel;

        [Header("Global Classes")]
        [Space]
        [SerializeField] List<DamageClass> _damageClasses = new List<DamageClass>();
        Dictionary<DamageType, DamageClass> _damageClassLookup = new Dictionary<DamageType, DamageClass>();
        [SerializeField] List<WeaponClass> _weaponClasses = new List<WeaponClass>();
        Dictionary<WeaponType, WeaponClass> _weaponClassLookup = new Dictionary<WeaponType, WeaponClass>();

        // state
        GameState state = new GameState();
        public static bool isPaused = false;

        public GameMode gameMode => state.mode;
        public DamageClass GetDamageClass(DamageType damageType) {
            return _damageClassLookup[damageType];
        }
        public WeaponClass GetWeaponClass(WeaponType weaponType) {
            return _weaponClassLookup[weaponType];
        }

        // singleton
        static GameManager _current;
        public static GameManager current => _current;

        void OnEnable() {
            eventChannel.OnPlayerDeath.Subscribe(OnPlayerDeath);
            eventChannel.OnEnemyDeath.Subscribe(OnEnemyDeath);
            eventChannel.OnWinLevel.Subscribe(OnWinLevel);
            eventChannel.OnPause.Subscribe(OnPause);
            eventChannel.OnUnpause.Subscribe(OnUnpause);
        }

        void OnDisable() {
            eventChannel.OnPlayerDeath.Unsubscribe(OnPlayerDeath);
            eventChannel.OnEnemyDeath.Unsubscribe(OnEnemyDeath);
            eventChannel.OnWinLevel.Unsubscribe(OnWinLevel);
            eventChannel.OnPause.Unsubscribe(OnPause);
            eventChannel.OnUnpause.Unsubscribe(OnUnpause);
        }

        void Awake() {
            _current = Utils.ManageSingleton<GameManager>(_current, this);
            SetupDamageClasses();
            SetupWeaponClasses();
            ULayer.Init();
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

        public static void Cleanup() {
            Utils.CleanupSingleton<GameManager>(_current);
        }

        void OnPlayerDeath() {
            StartCoroutine(IRespawn());
        }

        void OnEnemyDeath(int instanceId, int points) {
            // TODO: LOG SCORE DUE TO ENEMIES KILLED
        }

        void OnWinLevel() {
            Debug.Log("VICTORY!!");
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

        IEnumerator IRespawn() {
            yield return new WaitForSeconds(respawnWaitTime);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
