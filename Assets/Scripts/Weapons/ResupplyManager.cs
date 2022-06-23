using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Enemies;
using Player;
using Event;

namespace Weapons {

    public class ResupplyManager : MonoBehaviour {

        [SerializeField] EventChannelSO eventChannel;

        [Space]

        [SerializeField] WaveConfigSO waveAmmoLTR;
        [SerializeField] WaveConfigSO waveAmmoRTL;
        [SerializeField] WaveConfigSO waveNukeLTR;
        [SerializeField] WaveConfigSO waveNukeRTL;
        [SerializeField] WaveConfigSO waveMissileLTR;
        [SerializeField] WaveConfigSO waveMissileRTL;

        [Space]

        [SerializeField][Range(0f, 60f)] float waitTimeAmmo = 10f;
        [SerializeField][Range(0f, 60f)] float waitTimeNuke = 30f;
        [SerializeField][Range(0f, 60f)] float waitTimeMissile = 15f;

        PlayerGeneral player;
        Coroutine iResupplyAmmo;
        Coroutine iResupplyMissile;
        Coroutine iResupplyNuke;

        private void OnEnable() {
            eventChannel.OnOutOfAmmo.Subscribe(OnOutOfAmmo);
            eventChannel.OnTakeAmmo.Subscribe(OnTakeAmmo);
            eventChannel.OnWinLevel.Subscribe(StopAllResupplies);
            eventChannel.OnPlayerDeath.Subscribe(StopAllResupplies);
        }

        private void OnDisable() {
            eventChannel.OnOutOfAmmo.Unsubscribe(OnOutOfAmmo);
            eventChannel.OnTakeAmmo.Unsubscribe(OnTakeAmmo);
            eventChannel.OnWinLevel.Unsubscribe(StopAllResupplies);
            eventChannel.OnPlayerDeath.Unsubscribe(StopAllResupplies);
        }

        void StopAllResupplies() {
            StopAllCoroutines();
            iResupplyAmmo = null;
            iResupplyMissile = null;
            iResupplyNuke = null;
        }
        void StopAllResupplies(bool showUpgradePanel) {
            StopAllResupplies();
        }

        void OnTakeAmmo(WeaponType weaponType, int value) {
            switch (weaponType) {
                case WeaponType.MachineGun:
                    if (iResupplyAmmo != null) StopCoroutine(iResupplyAmmo);
                    iResupplyAmmo = null;
                    break;
                case WeaponType.Missile:
                    if (iResupplyMissile != null) StopCoroutine(iResupplyMissile);
                    iResupplyMissile = null;
                    break;
                case WeaponType.Nuke:
                    if (iResupplyNuke != null) StopCoroutine(iResupplyNuke);
                    iResupplyNuke = null;
                    break;
            }
        }

        void OnOutOfAmmo(WeaponType weaponType) {
            switch (weaponType) {
                case WeaponType.MachineGun:
                    if (iResupplyAmmo == null) iResupplyAmmo = StartCoroutine(IResupplyAmmo());
                    break;
                case WeaponType.Missile:
                    if (iResupplyMissile == null) iResupplyMissile = StartCoroutine(IResupplyMissile());
                    break;
                case WeaponType.Nuke:
                    if (iResupplyNuke == null) iResupplyNuke = StartCoroutine(IResupplyNuke());
                    break;
            }
        }

        IEnumerator IResupplyAmmo() {
            yield return new WaitForSeconds(waitTimeAmmo);
            if (waveAmmoLTR != null) StartCoroutine(SpawnEnemies(waveAmmoLTR));
            if (waveAmmoRTL != null) StartCoroutine(SpawnEnemies(waveAmmoRTL));
            iResupplyAmmo = null;
        }
        IEnumerator IResupplyMissile() {
            yield return new WaitForSeconds(waitTimeMissile);
            if (waveMissileLTR != null) StartCoroutine(SpawnEnemies(waveMissileLTR));
            if (waveMissileRTL != null) StartCoroutine(SpawnEnemies(waveMissileRTL));
            iResupplyMissile = null;
        }
        IEnumerator IResupplyNuke() {
            yield return new WaitForSeconds(waitTimeNuke);
            if (waveNukeLTR != null) StartCoroutine(SpawnEnemies(waveNukeLTR));
            if (waveNukeRTL != null) StartCoroutine(SpawnEnemies(waveNukeRTL));
            iResupplyNuke = null;
        }

        // ------------------------------------------------------------------------------------------------
        // yes, the following was copied from BattlePlayer. Sue me. Refactoring is a problem for future Don.
        // ------------------------------------------------------------------------------------------------

        IEnumerator SpawnEnemies(WaveConfigSO wave) {
            if (wave != null) {
                for (int i = 0; i < wave.numLoops; i++) {
                    for (int j = 0; j < wave.spawnCount; j++) {
                        player = PlayerUtils.FindPlayer();
                        GameObject enemy = SpawnObject(wave.GetEnemy(j), wave);
                        if (wave.HasPath()) {
                            SetEnemyPathfollow(enemy, wave.GetWaypoints(), wave.pathfinderLoopMode, wave.flipX, wave.flipY, wave.maxPathLoops);
                        }
                        LaunchEnemy(enemy, wave);
                        yield return new WaitForSeconds(wave.spawnInterval);
                    }
                    if (i < wave.numLoops - 1) yield return new WaitForSeconds(wave.loopInterval);
                }
            }
        }

        GameObject SpawnObject(WaveEnemy enemy, WaveConfigSO wave) {
            return Instantiate(
                enemy.prefab,
                (enemy.hasSpawnLocation
                    ? wave.ParseSpawnLocation(enemy.spawnLocation) + (Vector2)enemy.spawnOffset
                    : wave.GetSpawnPosition() + enemy.spawnOffset
                ),
                Quaternion.identity
            );
        }

        void SetEnemyPathfollow(GameObject enemy, List<Transform> waypoints, PathfinderLoopMode loopMode, bool flipX, bool flipY, int maxPathLoops) {
            var pathFollower = enemy.GetComponent<Pathfollower>();
            if (pathFollower == null) return;
            pathFollower.SetWaypoints(waypoints, flipX, flipY);
            pathFollower.SetLoopMode(loopMode);
            pathFollower.SetMaxLoops(maxPathLoops);
            pathFollower.Begin();
            var enemyMovement = enemy.GetComponent<EnemyMovement>();
            if (enemyMovement == null) return;
            enemyMovement.SetMode(MovementMode.Default);
        }

        void LaunchEnemy(GameObject enemy, WaveConfigSO wave) {
            var rb = enemy.GetComponent<Rigidbody2D>();
            if (rb == null) return;
            rb.velocity = wave.GetLaunchVelocity(enemy.transform.position, player != null ? player.transform.position : Vector2.zero);
        }

    }
}