using System.Collections.Generic;
using System.Collections;
using UnityEngine;

using Core;
using Game;
using Weapons;
using Damage;

namespace Enemies
{

    public class EnemyShooter : MonoBehaviour
    {
        [SerializeField] WeaponType weaponType = WeaponType.LaserRed;
        [SerializeField] List<Transform> guns = new List<Transform>();
        [SerializeField][Range(0f, 10f)] float triggerHoldTime = 1f;
        [SerializeField][Range(0f, 10f)] float triggerReleaseTime = 1f;
        [SerializeField][Range(0f, 10f)] float triggerTimeVariance = 0f;

        // Components
        EnemyShip enemy;

        // cached
        WeaponClass weapon;

        // state
        Timer triggerHeld = new Timer();
        Timer triggerReleased = new Timer();
        Timer firingTime = new Timer();
        Timer cooldown = new Timer();
        Timer burstCooldown = new Timer();
        Timer overheated = new Timer();
        int burstStep = 0;
        int ammo = 0;

        void Start() {
            AppIntegrity.AssertPresent<WeaponType>(weaponType);
            enemy = Utils.GetRequiredComponent<EnemyShip>(gameObject);
            weapon = GameManager.current.GetWeaponClass(weaponType);

            firingTime.SetDuration(weapon.firingRate);
            cooldown.SetDuration(weapon.cooldownTime);
            overheated.SetDuration(weapon.overheatTime);
            burstCooldown.SetDuration(weapon.burstInterval);

            ammo = weapon.startingAmmo;

            StartCoroutine(PressAndReleaseTrigger());
        }

        void Update() {
            if (Fire()) {
                AfterFire();
            } else {
                AfterNoFire();
            }
            TickTimers();
        }

        bool Fire() {
            if (!enemy.isAlive) return false;
            if (!triggerHeld.active) return false;
            if (firingTime.active) return false;
            if (burstCooldown.active) return false;
            if (weapon.overheats && cooldown.active) return false;

            if (weapon.infiniteAmmo || ammo > 0) {
                foreach (var gun in guns) {    
                    FireProjectile(weapon.prefab, gun.position, gun.rotation);
                }
                return true;
            } else {
                return false;
            }
        }

        void AfterFire() {
            if (!weapon.infiniteAmmo) ammo--;
            if (weapon.overheats) {
                overheated.Tick();
                if (overheated.tEnd) {
                    cooldown.Start();
                    overheated.Start();
                }
            }

            firingTime.Start();
            burstStep++;
            if (burstStep >= weapon.burst) {
                burstCooldown.Start();
                burstStep = 0;
            }
        }

        void AfterNoFire() {
            overheated.TickReversed();
        }

        void FireProjectile(GameObject prefab, Vector3 position, Quaternion rotation) {
            GameObject instance = Object.Instantiate(prefab, position, rotation);
            DamageDealer damager = instance.GetComponent<DamageDealer>();
            Collider2D collider = instance.GetComponent<Collider2D>();
            if (collider != null) enemy.IgnoreCollider(instance.GetComponent<Collider2D>());
            if (damager != null) damager.SetIgnoreUUID(enemy.uuid);
            Destroy(instance, weapon.lifetime);
        }

        void TickTimers() {
            firingTime.Tick();
            cooldown.Tick();
            burstCooldown.Tick();
            triggerHeld.Tick();
            triggerReleased.Tick();
        }

        IEnumerator PressAndReleaseTrigger() {
            while (true) {
                triggerHeld.SetDuration(Mathf.Max(triggerHoldTime + UnityEngine.Random.Range(-triggerTimeVariance / 2, triggerTimeVariance), 0.1f));
                triggerReleased.SetDuration(Mathf.Max(triggerReleaseTime + UnityEngine.Random.Range(-triggerTimeVariance / 2, triggerTimeVariance), 0.1f));
                yield return triggerHeld.StartAndWaitUntilFinished();
                yield return triggerReleased.StartAndWaitUntilFinished();
            }
        }
    }
}
