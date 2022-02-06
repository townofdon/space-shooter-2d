
using System.Collections.Generic;
using UnityEngine;

using Core;
using Damage;

namespace Game {


    public class GameManager : MonoBehaviour
    {

        [SerializeField] List<DamageClass> _damageClasses = new List<DamageClass>();
        Dictionary<DamageType, DamageClass> _damageClassLookup = new Dictionary<DamageType, DamageClass>();

        public DamageClass GetDamageClass(DamageType damageType) {
            return _damageClassLookup[damageType];
        }

        // singleton
        static GameManager _current;
        public static GameManager current => _current;

        void Awake() {
            _current = Utils.ManageSingleton<GameManager>(_current, this);
            SetupDamageClasses();
        }

        public void SetupDamageClasses() {
            foreach (var damageClass in _damageClasses) {
                _damageClassLookup.Add(damageClass.damageType, damageClass);
            }

            // assert all damage classes present
            foreach(DamageType damageType in System.Enum.GetValues(typeof(DamageType))) {
                try {
                    // note - the dictionary lookup itself will fail, which is fine
                    if (_damageClassLookup[damageType] == null) {
                        string name = System.Enum.GetName(typeof(DamageType), damageType);
                        Debug.LogError("ERR: damage type \"" + name + "\" was not loaded");
                    }
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
