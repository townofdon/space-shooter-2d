using System.Collections.Generic;
using System.Collections;
using UnityEngine;

using Core;
using Weapons;
using Damage;

namespace Enemies
{
    [RequireComponent(typeof(EnemyShip))]

    public class EnemyShooter : MonoBehaviour
    {
        [SerializeField] WeaponClass weapon;
        [SerializeField] List<Transform> guns = new List<Transform>();
        [SerializeField][Range(0f, 10f)] float triggerHoldTime = 1f;
        [SerializeField][Range(0f, 10f)] float triggerReleaseTime = 1f;
        [SerializeField][Range(0f, 10f)] float triggerTimeVariance = 0f;

        // Components
        EnemyShip enemy;

        // state
        Timer triggerHeld = new Timer();
        Timer triggerReleased = new Timer();

        void Start() {
            enemy = Utils.GetRequiredComponent<EnemyShip>(gameObject);
            InitWeapon();
            StartCoroutine(PressAndReleaseTrigger());
        }

        void InitWeapon() {
            AppIntegrity.AssertPresent<WeaponClass>(weapon);
            weapon = weapon.Clone();
            weapon.Init();
            weapon.shotSound.Init(this);
            weapon.effectSound.Init(this);
        }

        void Update() {
            if (Fire()) {
                AfterFire();
            } else {
                AfterNoFire();
            }
            weapon.TickTimers();
        }

        bool Fire() {
            if (!enemy.isAlive) return false;
            if (!weapon.ShouldFire(triggerHeld.active)) return false;

            foreach (var gun in guns) {    
                FireProjectile(weapon.prefab, gun.position, gun.rotation);
                weapon.shotSound.Play();
                weapon.effectSound.Play();
            }
            return true;
        }

        void AfterFire() {
            weapon.AfterFire();
        }

        void AfterNoFire() {
            weapon.AfterNoFire();
            weapon.effectSound.Stop();
        }

        void FireProjectile(GameObject prefab, Vector3 position, Quaternion rotation) {
            GameObject instance = Object.Instantiate(prefab, position, rotation);
            DamageDealer damager = instance.GetComponent<DamageDealer>();
            Collider2D collider = instance.GetComponent<Collider2D>();
            if (collider != null) enemy.IgnoreCollider(instance.GetComponent<Collider2D>());
            if (damager != null) damager.SetIgnoreUUID(enemy.uuid);
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
