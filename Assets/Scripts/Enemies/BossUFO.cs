using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Damage;
using Player;
using Core;
using Weapons;
using Audio;

namespace Enemies {

    public class BossUFO : MonoBehaviour {

        [Header("Agro")]
        [Space]
        [SerializeField][Range(0f, 1f)] float agroThreshold = 0.5f;
        [SerializeField][Range(0f, 1f)] float secondaryMissileThreshold = 0.3f;
        [SerializeField][Range(0f, 1f)] float tertiaryMissileThreshold = 0.15f;
        [SerializeField][Range(0f, 1f)] float weaponUpgradeThreshold = 0.7f;
        [SerializeField] Animator anim;

        [Header("Missiles")]
        [Space]
        [SerializeField] Transform missileLaunchL;
        [SerializeField] Transform missileLaunchR;
        [SerializeField] GameObject missilePrefab;
        [SerializeField][Range(0f, 50f)] float launchForce = 5f;
        [SerializeField][Range(0f, 50f)] float launchForceSecondary = 7f;
        [SerializeField][Range(0f, 50f)] float launchForceTertiary = 10f;
        [SerializeField] float launchInterval = 2f;
        [SerializeField] Sound launchSound;

        // cached
        DamageableBehaviour self;
        EnemyShooter shooter;
        PlayerGeneral player;
        GameObject instance;
        Rigidbody2D rbInstance;
        Rocket rocket;

        // state
        bool isAgro;
        bool didUpgrade;
        Timer launchCooldown = new Timer();

        void Start() {
            self = GetComponent<DamageableBehaviour>();
            shooter = GetComponent<EnemyShooter>();
            player = PlayerUtils.FindPlayer();

            AppIntegrity.AssertPresent(missileLaunchL);
            AppIntegrity.AssertPresent(missileLaunchR);
            AppIntegrity.AssertPresent(missilePrefab);

            launchCooldown.SetDuration(launchInterval);
            launchSound.Init(this);
        }

        void Update() {
            if (player == null || !player.isAlive) player = PlayerUtils.FindPlayer();
            HandleAgro();
            HandleLaunchMissiles();
            launchCooldown.Tick();
        }

        void HandleAgro() {
            if (self.healthPct < agroThreshold && !isAgro) {
                isAgro = true;
                if (anim != null) anim.SetTrigger("Agro");
            }

            if (self.healthPct < weaponUpgradeThreshold && !didUpgrade) {
                shooter.UpgradeWeapon();
                didUpgrade = true;
            }
        }

        void HandleLaunchMissiles() {
            if (!isAgro) return;
            if (!self.isAlive) return;
            if (launchCooldown.active) return;

            launchCooldown.Start();
            launchSound.Play();

            LaunchMissile(missileLaunchL.position, Vector2.left, launchForce);
            LaunchMissile(missileLaunchR.position, Vector2.right, launchForce);

            if (self.healthPct < secondaryMissileThreshold) {
                LaunchMissile(missileLaunchL.position, Vector2.left, launchForceSecondary);
                LaunchMissile(missileLaunchR.position, Vector2.right, launchForceSecondary);
            }

            if (self.healthPct < tertiaryMissileThreshold) {
                LaunchMissile(missileLaunchL.position, Vector2.left, launchForceTertiary);
                LaunchMissile(missileLaunchR.position, Vector2.right, launchForceTertiary);
            }
        }

        void LaunchMissile(Vector3 position, Vector3 heading, float force) {
            instance = Instantiate(missilePrefab, position, Quaternion.Euler(0f, 0f, 180f));
            rbInstance = instance.GetComponent<Rigidbody2D>();
            if (rbInstance == null) return;
            rbInstance.AddForce(heading * force, ForceMode2D.Impulse);
            rocket = instance.GetComponent<Rocket>();
            if (rocket == null) return;
            rocket.SetIgnoreUUID(self.uuid);
            rocket.SetIgnoreLayers(ULayer.Enemies.mask | ULayer.Projectiles.mask);
            if (player == null || !player.isAlive) return;
            rocket.SetTarget(player.transform);
        }
    }

}
