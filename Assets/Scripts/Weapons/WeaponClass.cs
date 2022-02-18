using System.Collections.Generic;
using UnityEngine;

using Audio;
using Core;

namespace Weapons
{

    public enum WeaponType {
        Laser,
        LaserRed,
        MachineGun,
        DisruptorRing,
        Nuke,
        Missile,
    }

    // public struct WeaponState {
    //     // hold all weapon state here - firing rate, ammo, cooldown time, reload, deployment, etc.
    //     public WeaponState(
    //         int startingAmmo,
    //         float overheatTime,
    //         float cooldownTime,
    //         float firingRate,
    //         float burstMax,
    //         float burstInterval,
    //         float deploymentTime,
    //         float teardownTime,
    //         bool reloads,
    //         float magazineCapacity,
    //         float reloadTime
    //     ) {
    //         _deploying = new Timer();
    //         _teardown = new Timer();
    //         _firing = new Timer();
    //         _burst = new Timer();
            
    //         _deploying.SetDuration(deploymentTime);
    //         _teardown.SetDuration(teardownTime);
    //         _firing.SetDuration(firingRate);
    //         _burst.SetDuration(burstInterval);
    //     }

    //     // STATE
    //     bool _equipped = false; // does the actor have this weapon?
    //     int _ammo = 0;
    //     int _burstStep = 0;
    //     int _firingCycle = 0;
    //     Timer _deploying = new Timer();
    //     Timer _teardown = new Timer();
    //     Timer _firing = new Timer();
    //     Timer _burst = new Timer();

    //     public void FireStep() {
    //         _ammo--;
    //         _firingCycle++;
    //     }
    // }

    [System.Serializable]
    class WeaponSettings {
        public WeaponSettings() {
            _damageMultiplier = 1f;
            _startingAmmo = 0;
            _infiniteAmmo = true;
            _reloads = false;
            _reloadTime = 0f;
            _magazineCapacity = 0;
            _firingRate = 0f;
            _burstMax = 0;
            _burstInterval = 0f;
            _overheats = false;
            _overheatTime = 0f;
            _cooldownTime = 0f;
            _deploymentTime = .5f;
            _shieldDrain = 0;
        }

        [Header("Damage")][Space]
        [SerializeField] string _assetClass;

        [Header("Damage")][Space]
        [SerializeField][Range(0f, 10f)] float _damageMultiplier = 1f;

        [Header("Ammo / Firing")][Space]
        [SerializeField] int _startingAmmo = 0;
        [SerializeField] bool _infiniteAmmo = true;
        [SerializeField] bool _reloads = false;
        [SerializeField][Range(0f, 10f)] float _reloadTime = 0f;
        [SerializeField] int _magazineCapacity = 0;
        [SerializeField][Range(0f, 10f)] float _firingRate = 0f;
        [SerializeField][Range(0, 10)] int _burstMax = 0;
        [SerializeField][Range(0f, 1f)] float _burstInterval = 0f;

        [Header("Cooldown / Deployment")][Space]
        [SerializeField] bool _overheats = false;
        [SerializeField][Range(0f, 10f)] float _overheatTime = 0f;
        [SerializeField][Range(0f, 10f)] float _cooldownTime = 0f;
        [SerializeField][Range(0f, 10f)] float _deploymentTime = .5f;
        
        [Header("Physics")][Space]
        [SerializeField] float _launchForce = 0f;
        [SerializeField] float _recoil = 0f;

        [Header("Prefab Settings")][Space]
        [SerializeField] GameObject _prefab;

        [Header("Side Effects")][Space]
        [SerializeField] float _shieldDrain = 0f;

        [Header("Cost")][Space]
        [SerializeField] int _cost = 0;
        
        [Header("Override Audio")][Space]
        [SerializeField] Sound _reloadSound;

        public string assetClass => _assetClass;
        public float damageMultiplier => _damageMultiplier;
        public int startingAmmo => _startingAmmo;
        public bool infiniteAmmo => _infiniteAmmo;
        public bool overheats => _overheats;
        public float overheatTime => _overheatTime;
        public float cooldownTime => _cooldownTime;
        public float firingRate => _firingRate;
        public int burstMax => _burstMax;
        public float burstInterval => _burstInterval;
        public float deploymentTime => _deploymentTime;
        public bool reloads => _reloads;
        public int magazineCapacity => _magazineCapacity;
        public float reloadTime => _reloadTime;
        public float recoil => _recoil;
        public float launchForce => _launchForce;
        public GameObject prefab => _prefab;
        public float shieldDrain => _shieldDrain;
        public Sound reloadSound => _reloadSound;
    }

    [CreateAssetMenu(fileName = "WeaponClass", menuName = "ScriptableObjects/WeaponClass", order = 0)]
    public class WeaponClass : ScriptableObject {
        [Header("Weapon Settings")][Space]
        [SerializeField] WeaponType _weaponType = WeaponType.Laser;
        [SerializeField] string _weaponName;

        [Header("Audio Settings")][Space]
        [SerializeField] Sound _shotSound;
        [SerializeField] LoopableSound _effectSound;
        [SerializeField] Sound _reloadSound;

        [SerializeField] WeaponSettings baseSettings = new WeaponSettings();
        [SerializeField] List<WeaponSettings> upgrades = new List<WeaponSettings>();

        WeaponSettings current => (_upgradeLevel > -1 && _upgradeLevel < upgrades.Count) ? upgrades[_upgradeLevel] : baseSettings;
        bool CanUpgrade => _upgradeLevel < upgrades.Count - 1;
        int CurrentUpgradeLevel => _upgradeLevel + 1;
        int MaxUpgradeLevel => upgrades.Count;
        List<Sound> _upgradeSounds = new List<Sound>();
        
        // PUBLIC SETTINGS
        public WeaponType type => _weaponType;
        public string weaponName => _weaponName;
        public int upgradeLevel => _upgradeLevel;
        public string assetClass => current.assetClass;
        public Sound shotSound => _shotSound;
        public LoopableSound effectSound => _effectSound;
        public Sound reloadSound => current.reloadSound.hasSource ? current.reloadSound : _reloadSound;
        public float damageMultiplier => current.damageMultiplier;
        public int startingAmmo => current.startingAmmo;
        public bool infiniteAmmo => current.infiniteAmmo;
        public bool overheats => current.overheats;
        public int magazineCapacity => current.magazineCapacity;
        public float recoil => current.recoil;
        public float launchForce => current.launchForce;
        public GameObject prefab => current.prefab;
        public float shieldDrain => current.shieldDrain;
        public List<Sound> upgradeSounds => _upgradeSounds;

        // PRIVATE SETTINGS

        int burstMax => current.burstMax;
        float reloadTime => current.reloadTime;
        float overheatTime => current.overheatTime;
        float cooldownTime => current.cooldownTime;
        float firingRate => current.firingRate;
        float burstInterval => current.burstInterval;
        float deploymentTime => current.deploymentTime;
        bool reloads => current.reloads;

        // STATE GETTERS
        public bool equipped => _equipped;
        public int ammo => _ammo;
        public int ammoInClip => GetAmmoLeftInClip();
        public int ammoInClipDisplayed => _ammoInClipDisplayed;
        public bool hasAmmo => infiniteAmmo || (HasAmmo() && HasAmmoLeftInClip());
        public int firingCycle => _firingCycle;
        public bool deploying => _deploying.active;
        public bool firing => _firing.active;
        public bool reloading => _reloading.active;
        public bool cooldownActive => _cooldown.active;
        public bool overheated => _overheated.active;
        public bool burstCooldownActive => _burstCooldown.active;

        // STATE
        [System.NonSerialized]
        bool initialized;
        int _upgradeLevel = -1;
        bool _equipped = false; // does the actor have this weapon?
        int _ammo = 0;
        int _ammoInClipDisplayed = 0;
        int _burstStep = 0;
        int _firingCycle = 0;
        Timer _deploying = new Timer();
        Timer _firing = new Timer();
        Timer _reloading = new Timer();
        Timer _cooldown = new Timer();
        Timer _overheated = new Timer();
        Timer _burstCooldown = new Timer();
        Timer _outOfAmmoNotifyTimer = new Timer();
        bool _backpackReloading = false;
        bool _didNotifyOutOfAmmo = false;

        // CALLBACKS
        System.Action<WeaponType> _onReload;
        System.Action<WeaponType> _onOutOfAmmo;

        public void Init(bool equipped = true) {
            if (!initialized) {
                _upgradeLevel = -1;
                initialized = true;
                PopulateUpgradeSounds();
            }
            _equipped = equipped;
            _ammo = startingAmmo;
            _burstStep = 0;
            _firingCycle = 0;
            _ammoInClipDisplayed = GetAmmoLeftInClip();
            _firing.SetDuration(firingRate);
            _reloading.SetDuration(reloadTime);
            _deploying.SetDuration(deploymentTime);
            _cooldown.SetDuration(cooldownTime);
            _overheated.SetDuration(overheatTime);
            _burstCooldown.SetDuration(burstInterval);
            _outOfAmmoNotifyTimer.SetDuration(1f);
            _firing.End();
            _reloading.End();
            _deploying.End();
            _cooldown.End();
            _burstCooldown.End();
            if (overheats) _overheated.Start(); // overheated gets ticked manually
        }

        public void RegisterCallbacks(System.Action<WeaponType> OnReload, System.Action<WeaponType> OnOutOfAmmo) {
            _onReload = OnReload;
            _onOutOfAmmo = OnOutOfAmmo;
        }

        public void Upgrade() {
            if (!CanUpgrade) return;
            Debug.Log("upgrading_" + _weaponType + " >> " + assetClass);
            _upgradeLevel += 1;
            Init();
        }

        public void UpgradeTo(int levelZeroIndexed) {
            Init();
            _upgradeLevel = Mathf.Clamp(levelZeroIndexed, -1, upgrades.Count - 1);
        }

        public void Deploy() {
            _firing.End();
            _burstStep = 0;
            if (_backpackReloading) {
                _reloading.End();
                _backpackReloading = false;
            } else {
                _deploying.Start();
            }
        }

        public void PickupAmmo(int amount) {
            int prevAmmo = _ammo;
            _ammo += amount;
            if (prevAmmo <= 0) Reload();
        }

        public bool ShouldFire(bool isButtonPressed) {
            if (!isButtonPressed && _burstStep <= 0) {
                _didNotifyOutOfAmmo = false;
                return false;
            }
            if (_firing.active) return false;
            if (_deploying.active) return false;
            if (reloads && _reloading.active) return false;
            if (overheats && _cooldown.active) return false;
            if (burstMax > 0 && _burstCooldown.active) return false;
            if (reloads && !HasAmmoLeftInClip()) return false;
            if (!HasAmmo()) {
                _burstStep = 0;
                if (!_didNotifyOutOfAmmo) {
                    _didNotifyOutOfAmmo = true;
                    InvokeCallback(_onOutOfAmmo);
                }
                return false;
            }
            return true;
        }

        public void AfterFire() {
            _firingCycle++;
            if (!infiniteAmmo) {
                _ammo = Mathf.Max(_ammo - 1, 0);
            }
            // handle burst
            if (burstMax > 0) {
                _burstStep++;
                if (_burstStep >= burstMax) {
                    _burstCooldown.Start();
                    _burstStep = 0;
                }
            }
            if (reloads && _firingCycle >= magazineCapacity) {
                _ammoInClipDisplayed = GetAmmoLeftInClip();
                Reload();
            }
            if (!reloads && _firingCycle > 99) {
                _firingCycle = 0;
            }
            if (overheats) {
                _overheated.Tick();
                if (_overheated.tEnd) {
                    _burstStep = 0;
                    _cooldown.Start();
                    _overheated.Start();
                }
            }
            _firing.Start();
        }

        public void Reload() {
            if (!reloads) return;
            if (GetAmmoLeftInClip() >= magazineCapacity) return;
            if (_reloading.active) {
                _backpackReloading = true;
                return;
            }
            _firingCycle = 0;
            _burstStep = 0;
            _reloading.Start();

            InvokeCallback(_onReload);
        }

        public void AfterNoFire() {
            if (overheats && !_firing.active && !_burstCooldown.active) {
                _overheated.TickReversed();
            }
        }

        public void TickTimers() {
            _firing.Tick();
            _reloading.Tick();
            _deploying.Tick();
            _cooldown.Tick();
            _burstCooldown.Tick();
            _outOfAmmoNotifyTimer.Tick();
            if (!_reloading.active) {
                _ammoInClipDisplayed = GetAmmoLeftInClip();
            }
        }

        public WeaponClass Clone() {
            string s = this.name;
            WeaponClass newInstance = Instantiate(this);
            newInstance.name = s;
            return newInstance;
        }

        int GetAmmo() {
            if (infiniteAmmo) return int.MaxValue;
            return _ammo;
        }

        int GetAmmoLeftInClip() {
            if (!reloads) return GetAmmo();
            return Mathf.Clamp(magazineCapacity - _firingCycle, 0, GetAmmo());
        }

        bool HasAmmoLeftInClip() {
            return GetAmmoLeftInClip() > 0;
        }

        bool HasAmmo() {
            if (infiniteAmmo) return true;
            if (_ammo > 0) return true;
            return false;
        }

        void InvokeCallback(System.Action<WeaponType> action) {
            if (action != null) {
                action.Invoke(_weaponType);
            }
        }

        void PopulateUpgradeSounds() {
            _upgradeSounds.Clear();
            foreach (var upgrade in upgrades) {
                if (upgrade.reloadSound.hasClip) {
                    _upgradeSounds.Add(upgrade.reloadSound);
                }
            }
        }
    }
}

