using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

using Event;
using Core;
using Player;
using Weapons;
using Game;
using Audio;

namespace UI {

    public class DebugUI : MonoBehaviour {
        [SerializeField] EventChannelSO eventChannel;
        [SerializeField] List<Button> buttons;
        [SerializeField] GameObject canvas;

        [SerializeField] WeaponClass machineGun;
        [SerializeField] WeaponClass laser;
        [SerializeField] WeaponClass missiles;
        [SerializeField] WeaponClass disruptor;

        [SerializeField] TextMeshProUGUI textPlayer;
        [SerializeField] TextMeshProUGUI textLevelHealth;
        [SerializeField] TextMeshProUGUI textLevelShield;

        [SerializeField] TextMeshProUGUI textLevelPDC;
        [SerializeField] TextMeshProUGUI textLevelLaser;
        [SerializeField] TextMeshProUGUI textLevelMissiles;
        [SerializeField] TextMeshProUGUI textLevelDisruptor;

        [SerializeField] TextMeshProUGUI textClassPDC;
        [SerializeField] TextMeshProUGUI textClassLaser;
        [SerializeField] TextMeshProUGUI textClassMissiles;
        [SerializeField] TextMeshProUGUI textClassDisruptor;

        [SerializeField] Button buttonUpgradePDC;
        [SerializeField] Button buttonUpgradeLaser;
        [SerializeField] Button buttonUpgradeMissiles;
        [SerializeField] Button buttonUpgradeDisruptor;

        UIButton uiButtonUpgradePDC;
        UIButton uiButtonUpgradeLaser;
        UIButton uiButtonUpgradeMissiles;
        UIButton uiButtonUpgradeDisruptor;

        // cache
        PlayerGeneral player;

        // state
        bool isShowing;

        public void UpgradePDC() { machineGun.Upgrade(); }
        public void ResetPDC() { machineGun.Reset(); }

        public void UpgradeLZR() { laser.Upgrade(); }
        public void ResetLZR() { laser.Reset(); }

        public void UpgradeMissiles() { missiles.Upgrade(); }
        public void ResetMissiles() { missiles.Reset(); }

        public void UpgradeDisruptor() { disruptor.Upgrade(); }
        public void ResetDisruptor() { disruptor.Reset(); }

        public void WarpToLevel(int level) {
            GameManager.isPaused = false;
            Time.timeScale = 1f;
            AudioManager.current.StopTrack();
            SceneManager.LoadScene(level);
        }

        void OnEnable() {
            eventChannel.OnShowDebug.Subscribe(OnShowDebugMenu);
            eventChannel.OnHideDebug.Subscribe(OnHideDebugMenu);
        }

        void OnDisable() {
            eventChannel.OnShowDebug.Unsubscribe(OnShowDebugMenu);
            eventChannel.OnHideDebug.Unsubscribe(OnHideDebugMenu);
        }

        void Awake() {
            AppIntegrity.AssertPresent(canvas);
            AppIntegrity.AssertPresent(machineGun);
            AppIntegrity.AssertPresent(laser);
            AppIntegrity.AssertPresent(missiles);
            AppIntegrity.AssertPresent(disruptor);

            AppIntegrity.AssertPresent(textPlayer);
            AppIntegrity.AssertPresent(textLevelHealth);
            AppIntegrity.AssertPresent(textLevelShield);

            AppIntegrity.AssertPresent(textLevelPDC);
            AppIntegrity.AssertPresent(textLevelLaser);
            AppIntegrity.AssertPresent(textLevelMissiles);
            AppIntegrity.AssertPresent(textLevelDisruptor);

            AppIntegrity.AssertPresent(textClassPDC);
            AppIntegrity.AssertPresent(textClassLaser);
            AppIntegrity.AssertPresent(textClassMissiles);
            AppIntegrity.AssertPresent(textClassDisruptor);

            AppIntegrity.AssertPresent(buttonUpgradePDC);
            AppIntegrity.AssertPresent(buttonUpgradeLaser);
            AppIntegrity.AssertPresent(buttonUpgradeMissiles);
            AppIntegrity.AssertPresent(buttonUpgradeDisruptor);

            uiButtonUpgradePDC = new UIButton(buttonUpgradePDC);
            uiButtonUpgradeLaser = new UIButton(buttonUpgradeLaser);
            uiButtonUpgradeMissiles = new UIButton(buttonUpgradeMissiles);
            uiButtonUpgradeDisruptor = new UIButton(buttonUpgradeDisruptor);

            canvas.SetActive(false);
            player = PlayerUtils.FindPlayer();
        }

        void Update() {
            UpdateGUI();
        }

        void UpdateGUI() {
            if (!isShowing) return;
            if (player == null) {
                textPlayer.text = "NOT FOUND";
            } else if (!player.isAlive) {
                textPlayer.text = "DEAD";
            } else {
                textPlayer.text = player.name;
            }

            textLevelPDC.text = machineGun.upgradeLevel.ToString();
            textLevelLaser.text = laser.upgradeLevel.ToString();
            textLevelMissiles.text = missiles.upgradeLevel.ToString();
            textLevelDisruptor.text = disruptor.upgradeLevel.ToString();

            textClassPDC.text = machineGun.assetClass;
            textClassLaser.text = laser.assetClass;
            textClassMissiles.text = missiles.assetClass;
            textClassDisruptor.text = disruptor.assetClass;

            textLevelHealth.text = player != null ? player.health.ToString() : "-";
            textLevelHealth.text = player != null ? player.shield.ToString() : "-";

            EnablifyWeaponButton(machineGun, uiButtonUpgradePDC);
            EnablifyWeaponButton(laser, uiButtonUpgradeLaser);
            EnablifyWeaponButton(missiles, uiButtonUpgradeMissiles);
            EnablifyWeaponButton(disruptor, uiButtonUpgradeDisruptor);
        }

        void EnablifyWeaponButton(WeaponClass weapon, UIButton button) {
            if (weapon.CanUpgrade) {
                button.Enable();
            } else {
                button.Disable();
            }
        }

        void OnShowDebugMenu() {
            player = PlayerUtils.FindPlayer();
            isShowing = true;
            canvas.SetActive(true);
            if (buttons.Count > 0) buttons[0].Select();
        }

        void OnHideDebugMenu() {
            isShowing = false;
            canvas.SetActive(false);
        }
    }
}

