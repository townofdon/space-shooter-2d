using UnityEngine;
using Core;
using Weapons;
using Game;
using Damage;

namespace Player
{    

    public class PlayerWeapons : MonoBehaviour
    {
        [Header("Weapon Settings")][Space]
        [SerializeField] WeaponType startingWeapon = WeaponType.Laser;
        [SerializeField] float aimMaxAngle = 30f;

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

        // cached
        WeaponClass laser;
        WeaponClass machineGun;
        WeaponClass disruptorRing;
        WeaponClass nuke;

        // state
        WeaponType currentWeapon;
        bool didSwitchWeapon = false;
        float timeDeployingNextWeapon = 0f;

        // state - general weapons
        float firingTime = 0f;
        float burstCooldown = 0f;
        int burstStep = 0;
        int firingCycle = 0;

        // state - machineGun
        int machineGunAmmo = 100;

        // state - nuke
        int nukeAmmo = 10;
        float nukeFiringTime = 0f;

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
            laser = GameManager.current.GetWeaponClass(WeaponType.Laser);
            machineGun = GameManager.current.GetWeaponClass(WeaponType.MachineGun);
            disruptorRing = GameManager.current.GetWeaponClass(WeaponType.DisruptorRing);
            nuke = GameManager.current.GetWeaponClass(WeaponType.Nuke);
            // init
            currentWeapon = startingWeapon;
            firingTime = 0f;
            nukeFiringTime = 0f;
        }

        void Update()
        {
            HandleFire();
            TickTimers();
            HandleSwitchWeapons();

            if (!player.isAlive || !input.isFirePressed || timeDeployingNextWeapon > 0f) DeactivateWeapons();
        }

        void HandleFire() {
            if (!player.isAlive) return;
            if (!input.isFirePressed) return;
            if (timeDeployingNextWeapon > 0f) return;
            if (burstCooldown > 0f) return;

            if (currentWeapon == WeaponType.Laser && firingTime <= 0) {
                FireProjectile(laser.prefab, mainGunL.position, mainGunL.rotation, laser.lifetime);
                FireProjectile(laser.prefab, mainGunR.position, mainGunR.rotation, laser.lifetime);
                if (sideGuns.activeSelf) {
                    FireProjectile(laser.prefab, sideGunL.position, sideGunL.rotation, laser.lifetime);
                    FireProjectile(laser.prefab, sideGunR.position, sideGunR.rotation, laser.lifetime);
                }
                if (rearGuns.activeSelf) {
                    FireProjectile(laser.prefab, rearGunL.position, rearGunL.rotation, laser.lifetime);
                    FireProjectile(laser.prefab, rearGunR.position, rearGunR.rotation, laser.lifetime);
                }
                burstStep++;
                if (burstStep >= laser.burst) {
                    burstCooldown = laser.burstInterval;
                    burstStep = 0;
                }
                firingTime = laser.firingRate;
            }

            if (currentWeapon == WeaponType.MachineGun && firingTime <= 0) {
                if (machineGunAmmo > 0 || machineGun.infiniteAmmo) {
                    if (firingCycle % 2 == 0) FireProjectile(machineGun.prefab, mainGunL.position, mainGunL.rotation, machineGun.lifetime);
                    if (firingCycle % 2 == 1) FireProjectile(machineGun.prefab, mainGunR.position, mainGunR.rotation, machineGun.lifetime);
                    machineGunAmmo--;
                    burstStep++;
                }
                if (sideGuns.activeSelf && (machineGunAmmo > 0 || machineGun.infiniteAmmo)) {
                    if (firingCycle % 2 == 1) FireProjectile(machineGun.prefab, sideGunL.position, sideGunL.rotation, machineGun.lifetime);
                    if (firingCycle % 2 == 0) FireProjectile(machineGun.prefab, sideGunR.position, sideGunR.rotation, machineGun.lifetime);
                    machineGunAmmo--;
                }
                if (rearGuns.activeSelf && (machineGunAmmo > 0 || machineGun.infiniteAmmo)) {
                    if (firingCycle % 2 == 1) FireProjectile(machineGun.prefab, rearGunL.position, rearGunL.rotation, machineGun.lifetime);
                    if (firingCycle % 2 == 0) FireProjectile(machineGun.prefab, rearGunR.position, rearGunR.rotation, machineGun.lifetime);
                    machineGunAmmo--;
                }
                if (burstStep >= machineGun.burst) {
                    burstCooldown = machineGun.burstInterval;
                    burstStep = 0;
                }
                firingTime = machineGun.firingRate;
                firingCycle++;
            }

            if (currentWeapon == WeaponType.Nuke && nukeFiringTime <= 0) {
                if (nukeAmmo > 0 || nuke.infiniteAmmo) {
                    FireProjectile(nuke.prefab, transform.position, Quaternion.identity, nuke.lifetime);
                    nukeFiringTime = nuke.firingRate;
                    nukeAmmo -= 1;
                } else {
                    // TODO: PLAY SOUND
                }
            }

            if (currentWeapon == WeaponType.DisruptorRing) {
                if (player.shield > 0f) {
                    disruptorRingEffect.SetActive(true);
                    player.DrainShield(disruptorRing.shieldDrain * Time.deltaTime);
                } else {
                    disruptorRingEffect.SetActive(false);
                }
            }

            if (firingCycle > 99) firingCycle = 0;
        }

        void FireProjectile(GameObject prefab, Vector3 position, Quaternion rotation, float lifetime) {
            Quaternion aim = Quaternion.AngleAxis(-input.look.x * aimMaxAngle, Vector3.forward);
            GameObject instance = Object.Instantiate(prefab, position, aim * rotation);
            DamageDealer damager = instance.GetComponent<DamageDealer>();
            Collider2D collider = instance.GetComponent<Collider2D>();
            if (collider != null) player.IgnoreCollider(instance.GetComponent<Collider2D>());
            if (damager != null) damager.SetIgnoreTag(UTag.Player);
            Destroy(instance, lifetime);
        }

        void DeactivateWeapons() {
            firingTime = 0f;
            burstStep = 0;
            burstCooldown = 0f;
            disruptorRingEffect.SetActive(false);
        }

        void TickTimers() {
            firingTime = Mathf.Max(firingTime - Time.deltaTime, 0f);
            nukeFiringTime = Mathf.Max(nukeFiringTime - Time.deltaTime, 0f);
            burstCooldown = Mathf.Max(burstCooldown - Time.deltaTime, 0f);
            timeDeployingNextWeapon = Mathf.Max(timeDeployingNextWeapon - Time.deltaTime, 0f);
        }

        void HandleSwitchWeapons() {
            if (!player.isAlive) return;
            if (!input.isSwitchWeaponPressed) { didSwitchWeapon = false; return; }
            if (didSwitchWeapon) return;

            // TODO: PLAY DEPLOYMENT SOUND - UNIQ TO EACH WEAPON?
            // TODO: ADD SCRIPTABLE OBJECTS FOR WEAPON TYPES
            // TODO: ANIMATE OUT CURRENT WEAPON - USE AN ANIMATOR
            // TODO: ANIMATE IN NEXT WEAPON - USE AN ANIMATOR
            // TODO: SET timeDeployingNextWeapon PER WEAPONTYPE

            didSwitchWeapon = true;

            switch (currentWeapon)
            {
                case WeaponType.Laser:
                    currentWeapon = WeaponType.MachineGun;
                    timeDeployingNextWeapon = machineGun.deploymentTime;
                    break;
                case WeaponType.MachineGun:
                    currentWeapon = WeaponType.Nuke;
                    timeDeployingNextWeapon = nuke.deploymentTime;
                    break;
                case WeaponType.Nuke:
                    currentWeapon = WeaponType.DisruptorRing;
                    timeDeployingNextWeapon = disruptorRing.deploymentTime;
                    break;
                case WeaponType.DisruptorRing:
                    currentWeapon = WeaponType.Laser;
                    timeDeployingNextWeapon = laser.deploymentTime;
                    break;
                default:
                    break;
            }
        }
    }
}

