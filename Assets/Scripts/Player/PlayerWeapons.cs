using UnityEngine;
using Core;
using Weapons;
using Game;
using Damage;

namespace Player
{
    struct WeaponState {
        public WeaponState(WeaponType weaponType = WeaponType.Laser) {
            current = weaponType;
            prev = null;
            deploying = new Core.Timer();
            firing = new Core.Timer();
            burstCooldown = new Core.Timer();
            didSwitch = false;
            burstStep = 0;
            firingCycle = 0;
        }
        public WeaponType current { get; set; }
        public System.Nullable<WeaponType> prev { get; set; }
        public bool didSwitch { get; set; }
        public Core.Timer deploying { get; set; }
        public Core.Timer firing { get; set; }
        public Core.Timer burstCooldown { get; set; }
        public int burstStep { get; set; }
        public int firingCycle { get; set; }

        public void Set(WeaponClass weapon) {
            prev = current;
            current = weapon.type;
            burstStep = weapon.burst;
            firingCycle = 0;
            burstCooldown.SetDuration(weapon.burstInterval);
            deploying.SetDuration(weapon.deploymentTime);
            firing.SetDuration(weapon.firingRate);
            burstCooldown.End();
            firing.End();
        }
    }

    public class PlayerWeapons : MonoBehaviour
    {
        [Header("Weapon Settings")][Space]
        [SerializeField] WeaponType startingPrimaryWeapon = WeaponType.Laser;
        [SerializeField] WeaponType startingSecondaryWeapon = WeaponType.Nuke;
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
        WeaponState primaryWeapon = new WeaponState(WeaponType.Laser);
        WeaponState secondaryWeapon = new WeaponState(WeaponType.Nuke);

        // WeaponType primaryWeapon;
        // WeaponType secondaryWeapon;
        // bool didSwitchWeapon = false;
        // float timeDeployingNextWeapon = 0f;

        // // state - general weapons
        // float firingTime = 0f;
        // float burstCooldown = 0f;
        // int burstStep = 0;
        // int firingCycle = 0;

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
            primaryWeapon.Set(GameManager.current.GetWeaponClass(startingPrimaryWeapon));
            secondaryWeapon.Set(GameManager.current.GetWeaponClass(startingSecondaryWeapon));
            nukeFiringTime = 0f;
            // init sounds
            InitSound(laser);
            InitSound(machineGun);
            InitSound(disruptorRing);
            InitSound(nuke);
        }

        void Update()
        {
            HandleFirePrimary();
            HandleFireSecondary();
            TickTimers();
            HandleSwitchPrimaryWeapon();
            HandleSwitchSecondaryWeapon();

            if (!player.isAlive || !input.isFirePressed || primaryWeapon.deploying.active) DeactivatePrimaryWeapons();
            if (primaryWeapon.burstCooldown.active) OnBurstCooldownPrimary();
        }

        void InitSound(WeaponClass weapon) {
            weapon.shotSound.Init(this);
            weapon.effectSound.Init(this);
        }

        void HandleFirePrimary() {
            if (!player.isAlive) return;
            if (!input.isFirePressed) return;
            if (primaryWeapon.deploying.active) return;
            if (primaryWeapon.burstCooldown.active) return;

            if (primaryWeapon.current == WeaponType.Laser && !primaryWeapon.firing.active) {
                laser.shotSound.Play();
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
                primaryWeapon.burstStep++;
                if (primaryWeapon.burstStep >= laser.burst) {
                    primaryWeapon.burstCooldown.Start();
                    // burstCooldown = laser.burstInterval;
                    primaryWeapon.burstStep = 0;
                }
                // firingTime = laser.firingRate;
                primaryWeapon.firing.Start();
            }

            if (primaryWeapon.current == WeaponType.MachineGun && !primaryWeapon.firing.active) {
                // machineGun.shotSound.Play();
                machineGun.effectSound.Play();
                if (machineGunAmmo > 0 || machineGun.infiniteAmmo) {
                    if (primaryWeapon.firingCycle % 2 == 0) FireProjectile(machineGun.prefab, mainGunL.position, mainGunL.rotation, machineGun.lifetime);
                    if (primaryWeapon.firingCycle % 2 == 1) FireProjectile(machineGun.prefab, mainGunR.position, mainGunR.rotation, machineGun.lifetime);
                    machineGunAmmo--;
                    primaryWeapon.burstStep++;
                }
                if (sideGuns.activeSelf && (machineGunAmmo > 0 || machineGun.infiniteAmmo)) {
                    if (primaryWeapon.firingCycle % 2 == 1) FireProjectile(machineGun.prefab, sideGunL.position, sideGunL.rotation, machineGun.lifetime);
                    if (primaryWeapon.firingCycle % 2 == 0) FireProjectile(machineGun.prefab, sideGunR.position, sideGunR.rotation, machineGun.lifetime);
                    machineGunAmmo--;
                }
                if (rearGuns.activeSelf && (machineGunAmmo > 0 || machineGun.infiniteAmmo)) {
                    if (primaryWeapon.firingCycle % 2 == 1) FireProjectile(machineGun.prefab, rearGunL.position, rearGunL.rotation, machineGun.lifetime);
                    if (primaryWeapon.firingCycle % 2 == 0) FireProjectile(machineGun.prefab, rearGunR.position, rearGunR.rotation, machineGun.lifetime);
                    machineGunAmmo--;
                }
                if (primaryWeapon.burstStep >= machineGun.burst) {
                    // burstCooldown = machineGun.burstInterval;
                    primaryWeapon.burstCooldown.Start();
                    primaryWeapon.burstStep = 0;
                }
                // firingTime = machineGun.firingRate;
                primaryWeapon.firing.Start();
                primaryWeapon.firingCycle++;
            }

            if (primaryWeapon.current == WeaponType.DisruptorRing) {
                if (player.shield > 0f) {
                    disruptorRing.effectSound.Play();
                    disruptorRingEffect.SetActive(true);
                    player.DrainShield(disruptorRing.shieldDrain * Time.deltaTime);
                } else {
                    disruptorRingEffect.SetActive(false);
                    disruptorRing.effectSound.Stop();
                }
            }

            if (primaryWeapon.firingCycle > 99) primaryWeapon.firingCycle = 0;
        }

        void HandleFireSecondary() {
            if (!player.isAlive) return;
            if (!input.isFire2Pressed) return;
            if (secondaryWeapon.deploying.active) return;
            if (secondaryWeapon.burstCooldown.active) return;

            if (secondaryWeapon.current == WeaponType.Nuke && nukeFiringTime <= 0) {
                if (nukeAmmo > 0 || nuke.infiniteAmmo) {
                    FireProjectile(nuke.prefab, transform.position, Quaternion.identity, nuke.lifetime);
                    nukeFiringTime = nuke.firingRate;
                    nukeAmmo -= 1;
                } else {
                    // TODO: PLAY SOUND
                }
            }
        }

        void FireProjectile(GameObject prefab, Vector3 position, Quaternion rotation, float lifetime) {
            // Quaternion aim = Quaternion.AngleAxis(-input.look.x * aimMaxAngle, Vector3.forward);
            // GameObject instance = Object.Instantiate(prefab, position, aim * rotation);
            GameObject instance = Object.Instantiate(prefab, position, rotation);
            DamageDealer damager = instance.GetComponent<DamageDealer>();
            Collider2D collider = instance.GetComponent<Collider2D>();
            if (collider != null) player.IgnoreCollider(instance.GetComponent<Collider2D>());
            if (damager != null) damager.SetIgnoreTag(UTag.Player);
            Destroy(instance, lifetime);
        }

        void DeactivatePrimaryWeapons() {
            // firingTime = 0f;
            // burstStep = 0;
            // burstCooldown = 0f;
            primaryWeapon.firing.End();
            primaryWeapon.burstStep = 0;
            primaryWeapon.burstCooldown.End();
            disruptorRingEffect.SetActive(false);
            disruptorRing.effectSound.Stop();
            machineGun.effectSound.Stop();
        }

        void OnBurstCooldownPrimary() {
            machineGun.effectSound.Stop();
        }

        void TickTimers() {
            nukeFiringTime = Mathf.Max(nukeFiringTime - Time.deltaTime, 0f);
            // firingTime = Mathf.Max(firingTime - Time.deltaTime, 0f);
            // burstCooldown = Mathf.Max(burstCooldown - Time.deltaTime, 0f);
            // timeDeployingNextWeapon = Mathf.Max(timeDeployingNextWeapon - Time.deltaTime, 0f);
            primaryWeapon.firing.Tick();
            primaryWeapon.burstCooldown.Tick();
            primaryWeapon.deploying.Tick();
            secondaryWeapon.firing.Tick();
            secondaryWeapon.burstCooldown.Tick();
            secondaryWeapon.deploying.Tick();
        }

        void HandleSwitchPrimaryWeapon() {
            if (!player.isAlive) return;
            if (!input.isSwitchWeaponPressed) { primaryWeapon.didSwitch = false; return; }
            if (primaryWeapon.didSwitch) return;

            // TODO: PLAY DEPLOYMENT SOUND - UNIQ TO EACH WEAPON?
            // TODO: ADD SCRIPTABLE OBJECTS FOR WEAPON TYPES
            // TODO: ANIMATE OUT CURRENT WEAPON - USE AN ANIMATOR
            // TODO: ANIMATE IN NEXT WEAPON - USE AN ANIMATOR

            primaryWeapon.didSwitch = true;

            switch (primaryWeapon.current)
            {
                case WeaponType.Laser:
                    // primaryWeapon = WeaponType.MachineGun;
                    // timeDeployingNextWeapon = machineGun.deploymentTime;
                    primaryWeapon.Set(machineGun);
                    break;
                case WeaponType.MachineGun:
                    // primaryWeapon = WeaponType.DisruptorRing;
                    // timeDeployingNextWeapon = nuke.deploymentTime;
                    primaryWeapon.Set(disruptorRing);
                    break;
                case WeaponType.DisruptorRing:
                default:
                    // primaryWeapon = WeaponType.Laser;
                    // timeDeployingNextWeapon = laser.deploymentTime;
                    primaryWeapon.Set(laser);
                    break;
            }

            if (primaryWeapon.current != primaryWeapon.prev) primaryWeapon.deploying.Start();
        }

        void HandleSwitchSecondaryWeapon() {
            if (!player.isAlive) return;
            if (!input.isSwitchWeaponPressed) { secondaryWeapon.didSwitch = false; return; }
            if (secondaryWeapon.didSwitch) return;

            // TODO: PLAY DEPLOYMENT SOUND - UNIQ TO EACH WEAPON?
            // TODO: ADD SCRIPTABLE OBJECTS FOR WEAPON TYPES

            secondaryWeapon.didSwitch = true;

            switch (secondaryWeapon.current)
            {
                case WeaponType.Nuke:
                    // secondaryWeapon = WeaponType.Bomb;
                    // timeDeployingNextWeapon = disruptorRing.deploymentTime;
                    break;
                default:
                    secondaryWeapon.Set(nuke);
                    break;
            }

            if (secondaryWeapon.current != secondaryWeapon.prev) secondaryWeapon.deploying.Start();
        }
    }
}

