
using System.Collections.Generic;
using UnityEngine;

using Core;
using Damage;
using Weapons;

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

        [SerializeField] List<DamageClass> _damageClasses = new List<DamageClass>();
        Dictionary<DamageType, DamageClass> _damageClassLookup = new Dictionary<DamageType, DamageClass>();

        [SerializeField] List<WeaponClass> _weaponClasses = new List<WeaponClass>();
        Dictionary<WeaponType, WeaponClass> _weaponClassLookup = new Dictionary<WeaponType, WeaponClass>();

        GameState state = new GameState();

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

        void Awake() {
            _current = Utils.ManageSingleton<GameManager>(_current, this);
            SetupDamageClasses();
            SetupWeaponClasses();
        }

        public void SetupDamageClasses() {
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

        public void SetupWeaponClasses() {
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
    }
}
