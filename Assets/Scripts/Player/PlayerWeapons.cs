using UnityEngine;
using Core;
using Weapons;

namespace Player
{

    enum WeaponType {
        Nuke,
        Laser,
        MachineGun,
        DisruptorRing
    }

    public class PlayerWeapons : MonoBehaviour
    {
        [Header("Weapon Settings")][Space]
        [SerializeField] WeaponType startingWeapon = WeaponType.Nuke;

        [Header("Nuke")][Space]
        [SerializeField] GameObject nuke;
        [SerializeField] float nukeCooldownTime = 1f;

        [Header("DisruptorRing")][Space]
        [SerializeField] GameObject disruptorRing;
        [SerializeField] float disruptorRingShieldDrain = 25f;

        // components
        PlayerGeneral player;
        PlayerInputHandler input;

        // state
        WeaponType currentWeapon;
        bool didSwitchWeapon = false;
        float timeDeployingNextWeapon = 0f;

        // state - nuke
        float nukeTime = 0f;

        void Start()
        {
            AppIntegrity.AssertPresent<GameObject>(nuke);
            AppIntegrity.AssertPresent<GameObject>(disruptorRing);
            input = Utils.GetRequiredComponent<PlayerInputHandler>(gameObject);
            player = Utils.GetRequiredComponent<PlayerGeneral>(gameObject);
            // init
            currentWeapon = startingWeapon;
            nukeTime = 0f;
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

            if (currentWeapon == WeaponType.Nuke && nukeTime <= 0) {
                Object.Instantiate(nuke, transform.position, new Quaternion(0f,0f,0f,0f));
                nukeTime = nukeCooldownTime;
            }

            if (currentWeapon == WeaponType.DisruptorRing) {
                if (player.shield > 0f) {
                    disruptorRing.SetActive(true);
                    player.DrainShield(disruptorRingShieldDrain * Time.deltaTime);
                } else {
                    disruptorRing.SetActive(false);
                }
            }
        }

        void DeactivateWeapons() {
            if (currentWeapon == WeaponType.DisruptorRing) {
                disruptorRing.SetActive(false);
            }
        }

        void TickTimers() {
            nukeTime = Mathf.Clamp(nukeTime - Time.deltaTime, 0f, nukeCooldownTime);
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
                case WeaponType.Nuke:
                    currentWeapon = WeaponType.DisruptorRing;
                    timeDeployingNextWeapon = 0.5f;
                    break;
                case WeaponType.DisruptorRing:
                    currentWeapon = WeaponType.Nuke;
                    timeDeployingNextWeapon = 0.25f;
                    break;
                default:
                    break;
            }
        }
    }
}

