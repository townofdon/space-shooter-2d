using UnityEngine;

using Audio;

namespace Weapons
{

    public enum WeaponType {
        Laser,
        LaserRed,
        MachineGun,
        DisruptorRing,
        Nuke,
    }

    [CreateAssetMenu(fileName = "WeaponClass", menuName = "ScriptableObjects/WeaponClass", order = 0)]
    public class WeaponClass : ScriptableObject {
        [Header("Weapon Settings")][Space]
        [SerializeField] WeaponType _weaponType = WeaponType.Laser;
        [SerializeField] int _startingAmmo = 100;
        [SerializeField] bool _infiniteAmmo = false;
        [SerializeField] bool _overheats = false;
        [SerializeField][Range(0f, 10f)] float _overheatTime = 1f;
        [SerializeField][Range(0f, 10f)] float _cooldownTime = 1f;
        [SerializeField][Range(0f, 10f)] float _firingRate = 1f;
        [SerializeField][Range(0, 10)] int _burst = 3;
        [SerializeField][Range(0f, 1f)] float _burstInterval = 1f;
        [SerializeField][Range(0f, 10f)] float _deploymentTime = 1f;
        [SerializeField][Range(0f, 10f)] float _teardownTime = 0f;

        public WeaponType type => _weaponType;
        public int startingAmmo => _startingAmmo;
        public bool infiniteAmmo => _infiniteAmmo;
        public bool overheats => _overheats;
        public float overheatTime => _overheatTime;
        public float cooldownTime => _cooldownTime;
        public float firingRate => _firingRate;
        public int burst => _burst;
        public float burstInterval => _burstInterval;
        public float deploymentTime => _deploymentTime;
        public float teardownTime => _teardownTime;

        [Header("Prefab Settings")][Space]
        [SerializeField] GameObject _prefab;
        [SerializeField][Range(0f, 20f)] float _lifetime = 10f;

        public GameObject prefab => _prefab;
        public float lifetime => _lifetime;

        [Header("Audio Settings")][Space]
        [SerializeField] Sound _shotSound;
        [SerializeField] LoopableSound _effectSound;

        public Sound shotSound => _shotSound;
        public LoopableSound effectSound => _effectSound;

        [Header("Side Effects")][Space]
        [SerializeField] float _shieldDrain = 0;

        public float shieldDrain => _shieldDrain;

        // TODO: ADD SOUND EFFECTS HERE
    }
}

