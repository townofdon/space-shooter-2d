using UnityEngine;
using Core;
using Weapons;
using Damage;

namespace Player
{

    public class PlayerWeapons : MonoBehaviour
    {
        [Header("Weapon Classes")][Space]
        [SerializeField] WeaponClass laser;
        [SerializeField] WeaponClass machineGun;
        [SerializeField] WeaponClass disruptorRing;
        [SerializeField] WeaponClass missile;
        [SerializeField] WeaponClass nuke;

        [Header("Gun Objects")][Space]
        [SerializeField] GameObject mainGuns;
        [SerializeField] GameObject sideGuns;
        [SerializeField] GameObject rearGuns;

        [Header("Gun Locations")][Space]
        [SerializeField] Transform mainGunL;
        [SerializeField] Transform mainGunR;
        [SerializeField] Transform sideGunL;
        [SerializeField] Transform sideGunR;
        [SerializeField] Transform rearGunL;
        [SerializeField] Transform rearGunR;

        [Header("DisruptorRing")][Space]
        [SerializeField] GameObject disruptorRingEffect;

        // components
        PlayerGeneral player;
        PlayerInputHandler input;
        Rigidbody2D rb;

        // state
        WeaponClass primaryWeapon;
        WeaponClass secondaryWeapon;
        bool didSwitchPrimaryWeapon;
        bool didSwitchSecondaryWeapon;
        bool didReloadPrimary;

        void Start()
        {
            AppIntegrity.AssertPresent<GameObject>(disruptorRingEffect);
            AppIntegrity.AssertPresent<GameObject>(mainGunL);
            AppIntegrity.AssertPresent<GameObject>(mainGunR);
            AppIntegrity.AssertPresent<GameObject>(sideGunL);
            AppIntegrity.AssertPresent<GameObject>(sideGunR);
            AppIntegrity.AssertPresent<GameObject>(rearGunL);
            AppIntegrity.AssertPresent<GameObject>(rearGunR);
            input = Utils.GetRequiredComponent<PlayerInputHandler>(gameObject);
            player = Utils.GetRequiredComponent<PlayerGeneral>(gameObject);
            rb = GetComponent<Rigidbody2D>();
            InitWeapon(laser);
            InitWeapon(machineGun);
            InitWeapon(disruptorRing);
            InitWeapon(missile);
            InitWeapon(nuke);
            primaryWeapon = laser;
            secondaryWeapon = nuke;
        }

        void InitWeapon(WeaponClass weapon) {
            AppIntegrity.AssertPresent<WeaponClass>(weapon);
            weapon.Init();
            weapon.shotSound.Init(this);
            weapon.effectSound.Init(this);
            weapon.RegisterCallbacks(OnReloadWeapon, OnOutOfAmmo);
        }

        void Update()
        {
            if (FirePrimary()) {
                AfterPrimaryFire();
            } else {
                AfterPrimaryNoFire();
            }
            if (FireSecondary()) {
                AfterSecondaryFire();
            } else {
                AfterSecondaryNoFire();
            }

            TickTimers();
            HandleSwitchPrimaryWeapon();
            HandleSwitchSecondaryWeapon();
            HandleReloadPrimary();

            // TODO: REMOVE
            HandleTempUpgrade();
        }

        // TODO: REMOVE
        bool didUpgrade = false;
        void HandleTempUpgrade() {
            if (!input.isUpgradePressed) { didUpgrade = false; return; }
            if (didUpgrade) return;
            didUpgrade = true;
            primaryWeapon.Upgrade();
        }

        void InitSound(WeaponClass weapon) {
            weapon.shotSound.Init(this);
            weapon.effectSound.Init(this);
        }

        bool FirePrimary() {
            if (!player.isAlive) return false;
            if (!input.isFirePressed) return false;
            if (!primaryWeapon.CanFire()) return false;

            if (primaryWeapon.type == WeaponType.Laser) {
                laser.shotSound.Play();
                FireProjectile(laser, mainGunL.position, mainGunL.rotation);
                FireProjectile(laser, mainGunR.position, mainGunR.rotation);
                if (sideGuns.activeSelf) {
                    FireProjectile(laser, sideGunL.position, sideGunL.rotation);
                    FireProjectile(laser, sideGunR.position, sideGunR.rotation);
                }
                if (rearGuns.activeSelf) {
                    FireProjectile(laser, rearGunL.position, rearGunL.rotation);
                    FireProjectile(laser, rearGunR.position, rearGunR.rotation);
                }

                return true;
            }
            
            if (primaryWeapon.type == WeaponType.MachineGun) {
                machineGun.effectSound.Play();
                if (primaryWeapon.firingCycle % 2 == 0) FireProjectile(machineGun, mainGunL.position, mainGunL.rotation);
                if (primaryWeapon.firingCycle % 2 == 1) FireProjectile(machineGun, mainGunR.position, mainGunR.rotation);
                if (sideGuns.activeSelf) {
                    if (primaryWeapon.firingCycle % 2 == 1) FireProjectile(machineGun, sideGunL.position, sideGunL.rotation);
                    if (primaryWeapon.firingCycle % 2 == 0) FireProjectile(machineGun, sideGunR.position, sideGunR.rotation);
                }
                if (rearGuns.activeSelf) {
                    if (primaryWeapon.firingCycle % 2 == 1) FireProjectile(machineGun, rearGunL.position, rearGunL.rotation);
                    if (primaryWeapon.firingCycle % 2 == 0) FireProjectile(machineGun, rearGunR.position, rearGunR.rotation);
                }
                return true;
            }

            if (primaryWeapon.type == WeaponType.DisruptorRing) {
                if (player.shield > 0f) {
                    disruptorRing.effectSound.Play();
                    disruptorRingEffect.SetActive(true);
                    player.DrainShield(disruptorRing.shieldDrain * Time.deltaTime);
                    return true;
                } else {
                    disruptorRingEffect.SetActive(false);
                    disruptorRing.effectSound.Stop();
                    return false;
                }
            }

            return false;
        }

        void AfterPrimaryFire() {
            primaryWeapon.AfterFire();
        }
        void AfterPrimaryNoFire() {
            primaryWeapon.AfterNoFire();
            StopPrimaryFX();
        }

        void StopPrimaryFX() {
            disruptorRingEffect.SetActive(false);
            disruptorRing.effectSound.Stop();
            if (!primaryWeapon.firing) machineGun.effectSound.Stop();
        }

        bool FireSecondary() {
            if (!player.isAlive) return false;
            if (!input.isFire2Pressed) return false;
            if (!secondaryWeapon.CanFire()) return false;

            if (secondaryWeapon.type == WeaponType.Nuke) {
                FireProjectile(nuke, transform.position, transform.rotation);
                return true;
            }

            if (secondaryWeapon.type == WeaponType.Missile) {
                if (secondaryWeapon.firingCycle % 2 == 0) FireProjectile(missile, mainGunL.position, mainGunL.rotation);
                if (secondaryWeapon.firingCycle % 2 == 1) FireProjectile(missile, mainGunR.position, mainGunR.rotation);
                return true;
            }

            return false;
        }

        void AfterSecondaryFire() {
            secondaryWeapon.AfterFire();
        }
        void AfterSecondaryNoFire() {
            secondaryWeapon.AfterNoFire();
            StopSecondaryFX();
        }

        void StopSecondaryFX() {
            // add any sound / fx stops here
        }

        void FireProjectile(WeaponClass weapon, Vector3 position, Quaternion rotation) {
            // Quaternion aim = Quaternion.AngleAxis(-input.look.x * aimMaxAngle, Vector3.forward);
            // GameObject instance = Object.Instantiate(prefab, position, aim * rotation);
            GameObject instance = Object.Instantiate(weapon.prefab, position, rotation);
            DamageDealer damager = instance.GetComponent<DamageDealer>();
            Collider2D collider = instance.GetComponent<Collider2D>();
            if (collider != null) player.IgnoreCollider(instance.GetComponent<Collider2D>());
            if (damager != null) {
                damager.SetIgnoreTag(UTag.Player);
                damager.SetDamageMultiplier(weapon.damageMultiplier);
            }
            // recoil
            if (rb != null) {
                rb.AddForce(-transform.up * weapon.recoil, ForceMode2D.Impulse);
            }
            // launch rocket
            if (weapon.type == WeaponType.Missile) {
                Rocket rocket = instance.GetComponent<Rocket>();
                if (rocket != null) {
                    rocket.Launch(transform.up * weapon.launchForce);
                }
            }
        }

        void TickTimers() {
            laser.TickTimers();
            machineGun.TickTimers();
            disruptorRing.TickTimers();
            missile.TickTimers();
            nuke.TickTimers();
        }

        void HandleSwitchPrimaryWeapon() {
            if (!player.isAlive) return;
            if (!input.isSwitchWeaponPressed) { didSwitchPrimaryWeapon = false; return; }
            if (didSwitchPrimaryWeapon) return;

            // TODO: PLAY DEPLOYMENT SOUND - UNIQ TO EACH WEAPON?
            // TODO: ANIMATE OUT CURRENT WEAPON - USE AN ANIMATOR
            // TODO: ANIMATE IN NEXT WEAPON - USE AN ANIMATOR

            didSwitchPrimaryWeapon = true;

            switch (primaryWeapon.type)
            {
                case WeaponType.Laser:
                    primaryWeapon = machineGun;
                    break;
                case WeaponType.MachineGun:
                    primaryWeapon = disruptorRing;
                    break;
                case WeaponType.DisruptorRing:
                    primaryWeapon = laser;
                    break;
                default:
                    Debug.LogError("Unsupported primary weapon type: " + primaryWeapon.type);
                    return;
            }

            primaryWeapon.Deploy();
            StopPrimaryFX();
        }

        void HandleSwitchSecondaryWeapon() {
            if (!player.isAlive) return;
            if (!input.isSwitchWeapon2Pressed) { didSwitchSecondaryWeapon = false; return; }
            if (didSwitchSecondaryWeapon) return;

            // TODO: PLAY DEPLOYMENT SOUND - UNIQ TO EACH WEAPON?

            didSwitchSecondaryWeapon = true;

            switch (secondaryWeapon.type)
            {
                case WeaponType.Nuke:
                    secondaryWeapon = missile;
                    break;
                case WeaponType.Missile:
                    secondaryWeapon = nuke;
                    break;
                default:
                    Debug.LogError("Unsupported secondary weapon type: " + primaryWeapon.type);
                    return;
            }

            secondaryWeapon.Deploy();
            StopSecondaryFX();
        }

        void HandleReloadPrimary() {
            if (!player.isAlive) return;
            if (!input.isReloadPressed) { didReloadPrimary = false; return; }
            didReloadPrimary = true;
            primaryWeapon.Reload();
        }

        // NOTE - "OnReload" name clashed with PlayerInputHandler method
        void OnReloadWeapon(WeaponType weaponType) {
            // TODO: ADD SOUND PER WEAPON TYPE
            Debug.Log("reloading_" + weaponType);
        }

        void OnOutOfAmmo(WeaponType weaponType) {
            // TODO: ADD SOUND PER WEAPON TYPE
            Debug.Log(weaponType + " Out of ammo :(");
        }
    }
}
