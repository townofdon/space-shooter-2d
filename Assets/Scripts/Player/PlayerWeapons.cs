using UnityEngine;
using Core;
using Weapons;
using Damage;
using Audio;

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

        [Header("Audio")][Space]
        [SerializeField] Sound switchWeaponSound;

        // components
        PlayerGeneral player;
        PlayerInputHandler input;
        Rigidbody2D rb;

        // state
        WeaponClass primaryWeapon;
        WeaponClass secondaryWeapon;
        WeaponClass tertiaryWeapon;
        bool didSwitchPrimaryWeapon;
        bool didSwitchSecondaryWeapon;
        bool didReloadPrimary;

        public WeaponType primaryWeaponType => primaryWeapon.type;
        public string primaryWeaponClipAmmo => GetAmmoString(primaryWeapon.ammoInClipDisplayed);
        public string primaryWeaponReserveAmmo => GetAmmoString(primaryWeapon.reserveAmmo, primaryWeapon.ammoInClipDisplayed == int.MaxValue);
        public bool primaryWeaponReloading => primaryWeapon.reloading;
        public bool primaryWeaponDeploying => primaryWeapon.deploying;
        public WeaponType secondaryWeaponType => secondaryWeapon.type;
        public string nukeAmmo => GetAmmoString(Mathf.Min(nuke.ammo, 99));
        public string missileAmmo => GetAmmoString(Mathf.Min(missile.ammo, 99));

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
            tertiaryWeapon = disruptorRing;
            switchWeaponSound.Init(this);
        }

        void InitWeapon(WeaponClass weapon) {
            AppIntegrity.AssertPresent<WeaponClass>(weapon);
            weapon.Init();
            weapon.shotSound.Init(this);
            weapon.effectSound.Init(this);
            weapon.reloadSound.Init(this);
            weapon.RegisterCallbacks(OnReloadWeapon, OnOutOfAmmoAlarm, OnOutOfAmmoGunClick);
            foreach (var upgradeSound in weapon.upgradeSounds) upgradeSound.Init(this);
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
            if (FireTertiary()) {
                AfterTertiaryFire();
            } else {
                AfterTertiaryNoFire();
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
            secondaryWeapon.Upgrade();
        }

        void InitSound(WeaponClass weapon) {
            weapon.shotSound.Init(this);
            weapon.effectSound.Init(this);
        }

        bool FirePrimary() {
            if (!player.isAlive) return false;
            if (disruptorRingEffect.activeSelf) return false;
            if (!primaryWeapon.ShouldFire(input.isFirePressed)) return false;

            if (primaryWeapon.type == WeaponType.Laser) {
                laser.shotSound.Play();
                FireProjectile(laser, mainGunL.position, mainGunL.rotation);
                FireProjectile(laser, mainGunR.position, mainGunR.rotation);
                if (sideGuns.activeSelf) {
                    FireProjectile(laser, sideGunL.position, sideGunL.rotation, false);
                    FireProjectile(laser, sideGunR.position, sideGunR.rotation, false);
                }
                if (rearGuns.activeSelf) {
                    FireProjectile(laser, rearGunL.position, rearGunL.rotation, false);
                    FireProjectile(laser, rearGunR.position, rearGunR.rotation, false);
                }
                return true;
            }
            
            if (primaryWeapon.type == WeaponType.MachineGun) {
                machineGun.effectSound.Play();
                if (primaryWeapon.firingCycle % 2 == 0) FireProjectile(machineGun, mainGunL.position, mainGunL.rotation);
                if (primaryWeapon.firingCycle % 2 == 1) FireProjectile(machineGun, mainGunR.position, mainGunR.rotation);
                if (sideGuns.activeSelf) {
                    if (primaryWeapon.firingCycle % 2 == 1) FireProjectile(machineGun, sideGunL.position, sideGunL.rotation, false);
                    if (primaryWeapon.firingCycle % 2 == 0) FireProjectile(machineGun, sideGunR.position, sideGunR.rotation, false);
                }
                if (rearGuns.activeSelf) {
                    if (primaryWeapon.firingCycle % 2 == 1) FireProjectile(machineGun, rearGunL.position, rearGunL.rotation, false);
                    if (primaryWeapon.firingCycle % 2 == 0) FireProjectile(machineGun, rearGunR.position, rearGunR.rotation, false);
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
            if (!primaryWeapon.firing) machineGun.effectSound.Stop();
        }

        bool FireSecondary() {
            if (!player.isAlive) return false;
            if (disruptorRingEffect.activeSelf) return false;
            if (!secondaryWeapon.ShouldFire(input.isFire2Pressed)) return false;

            if (!secondaryWeapon.reloading) {
                secondaryWeapon.reloadSound.Stop();
            }

            if (secondaryWeapon.type == WeaponType.Nuke) {
                FireProjectile(nuke, transform.position, transform.rotation);
                return true;
            }

            if (secondaryWeapon.type == WeaponType.Missile) {
                missile.shotSound.Play();
                if (secondaryWeapon.firingCycle % 2 == 0) FireProjectile(missile, mainGunL.position, mainGunL.rotation);
                if (secondaryWeapon.firingCycle % 2 == 1) FireProjectile(missile, mainGunR.position, mainGunR.rotation);
                // // kinda hacky, but needed to fire the missiles off faster to give the player a chance to not have missiles blow up in their face... which kinda sucks
                // if (secondaryWeapon.upgradeLevel > 1) {
                //     FireProjectile(missile, mainGunL.position, mainGunL.rotation);
                //     // manually increment firing counter state
                //     secondaryWeapon.AfterFire();
                //     FireProjectile(missile, mainGunR.position, mainGunR.rotation);
                // }
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
            if (!secondaryWeapon.reloading) secondaryWeapon.reloadSound.Stop();
        }

        bool FireTertiary() {
            if (!player.isAlive) return false;
            if (!tertiaryWeapon.ShouldFire(input.isMeleePressed)) return false;

            if (tertiaryWeapon.type == WeaponType.DisruptorRing) {
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

        void AfterTertiaryFire() {
            tertiaryWeapon.AfterFire();
        }
        void AfterTertiaryNoFire() {
            tertiaryWeapon.AfterNoFire();
            StopTertiaryFX();
        }

        void StopTertiaryFX() {
            disruptorRingEffect.SetActive(false);
            disruptorRing.effectSound.Stop();
        }

        void FireProjectile(WeaponClass weapon, Vector3 position, Quaternion rotation, bool shouldRecoil = true) {
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
            if (shouldRecoil && rb != null) {
                rb.AddForce(-transform.up * weapon.recoil, ForceMode2D.Impulse);
            }
            // launch rocket
            if (weapon.type == WeaponType.Missile) {
                Rocket rocket = instance.GetComponent<Rocket>();
                if (rocket != null) {
                    rocket.Launch(GetRocketLaunchVector(weapon) * GetRocketLaunchForce(weapon));
                }
            }
        }

        Vector3 GetRocketLaunchVector(WeaponClass weapon) {
            return transform.up
                + (weapon.firingCycle % 2 == 0 ? -transform.right : transform.right)
                * Mathf.Max(0.05f, 0.2f * Mathf.Floor(weapon.firingCycle / 2f));
        }

        float GetRocketLaunchForce(WeaponClass weapon) {
            return weapon.launchForce + Mathf.Floor(weapon.firingCycle / 2f) * 7.5f;
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

            switchWeaponSound.Play();
            // TODO: ANIMATE OUT CURRENT WEAPON - USE AN ANIMATOR
            // TODO: ANIMATE IN NEXT WEAPON - USE AN ANIMATOR

            primaryWeapon.reloadSound.Stop();

            didSwitchPrimaryWeapon = true;

            switch (primaryWeapon.type)
            {
                case WeaponType.Laser:
                    primaryWeapon = machineGun;
                    break;
                case WeaponType.MachineGun:
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

            secondaryWeapon.reloadSound.Stop();
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
            if (weaponType == primaryWeapon.type) {
                primaryWeapon.reloadSound.Play();
            }
            if (weaponType == secondaryWeapon.type) {
                secondaryWeapon.reloadSound.Play();
            }
        }

        void OnOutOfAmmoAlarm(WeaponType weaponType) {
            // TODO: PLAY SOUND
            // outOfAmmoSound.Play();
            Debug.Log(weaponType + " Out of ammo :(");
        }

        void OnOutOfAmmoGunClick(WeaponType weaponType) {
            // TODO: PLAY SOUND
            // outOfAmmoGunClickSound.Play();
            Debug.Log(weaponType + " *cliccckkk");
        }

        string GetAmmoString(int ammoVal, bool hideIfInfinite = false) {
            if (ammoVal == int.MaxValue) return hideIfInfinite ? "" : "∞";
            return ammoVal.ToString();
        }
    }
}
