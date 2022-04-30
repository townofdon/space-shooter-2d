using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

using Event;
using Weapons;
using Core;
using Player;
using Game;
using Audio;

namespace UI {

    public class UpgradeUI : MonoBehaviour {

        [SerializeField] EventChannelSO eventChannel;
        [SerializeField] PlayerStateSO playerState;
        [SerializeField] GameObject canvas;
        [SerializeField] EventSystem eventSystem;

        [Space]
        [SerializeField] int gunsUpgradeCost = 20000;

        [Space]
        [SerializeField] WeaponClass machineGun;
        [SerializeField] WeaponClass laser;
        [SerializeField] WeaponClass missiles;
        [SerializeField] WeaponClass disruptor;

        [Space]
        [SerializeField] TextMeshProUGUI textNumCredits;
        [SerializeField] TextMeshProUGUI textMaxxedOut;

        [Space]
        [SerializeField] TextMeshProUGUI textNamePDC;
        [SerializeField] TextMeshProUGUI textNameLaser;
        [SerializeField] TextMeshProUGUI textNameMissiles;
        [SerializeField] TextMeshProUGUI textNameDisruptor;
        [SerializeField] TextMeshProUGUI textNameGuns;

        [Space]
        [SerializeField] TextMeshProUGUI textClassPDC;
        [SerializeField] TextMeshProUGUI textClassLaser;
        [SerializeField] TextMeshProUGUI textClassMissiles;
        [SerializeField] TextMeshProUGUI textClassDisruptor;
        [SerializeField] TextMeshProUGUI textClassGuns;

        [Space]
        [SerializeField] TextMeshProUGUI textCostPDC;
        [SerializeField] TextMeshProUGUI textCostLaser;
        [SerializeField] TextMeshProUGUI textCostMissiles;
        [SerializeField] TextMeshProUGUI textCostDisruptor;
        [SerializeField] TextMeshProUGUI textCostGuns;

        [Space]
        [SerializeField] Button buttonUpgradePDC;
        [SerializeField] Button buttonUpgradeLaser;
        [SerializeField] Button buttonUpgradeMissiles;
        [SerializeField] Button buttonUpgradeDisruptor;
        [SerializeField] Button buttonUpgradeGuns;

        [Space]
        [SerializeField] Color maxxedColor;

        UIButton uiButtonUpgradePDC;
        UIButton uiButtonUpgradeLaser;
        UIButton uiButtonUpgradeMissiles;
        UIButton uiButtonUpgradeDisruptor;
        UIButton uiButtonUpgradeGuns;

        InputSystemUIInputModule uiModule;

        bool isShowing;
        bool anyMaxxedOut;

        public void OnDone() {
            OnHideUpgradePanel();
            GameManager.current.GotoNextLevel();
        }

        public void UpgradePDC() {
            if (machineGun.CanUpgrade && machineGun.CostNextUpgrade <= playerState.totalMoney) {
                playerState.SpendMoney(machineGun.CostNextUpgrade);
                machineGun.Upgrade();
                AudioManager.current.PlaySound("upgrade-weapon");
            } else {
                AudioManager.current.PlaySound("upgrade-error");
            }
        }

        public void UpgradeLZR() {
            if (laser.CanUpgrade && laser.CostNextUpgrade <= playerState.totalMoney) {
                playerState.SpendMoney(laser.CostNextUpgrade);
                laser.Upgrade();
                AudioManager.current.PlaySound("upgrade-weapon");
            } else {
                AudioManager.current.PlaySound("upgrade-error");
            }
        }

        public void UpgradeMissiles() {
            if (missiles.CanUpgrade && missiles.CostNextUpgrade <= playerState.totalMoney) {
                playerState.SpendMoney(missiles.CostNextUpgrade);
                missiles.Upgrade();
                AudioManager.current.PlaySound("upgrade-weapon");
            } else {
                AudioManager.current.PlaySound("upgrade-error");
            }
        }

        public void UpgradeDisruptor() {
            if (disruptor.CanUpgrade && disruptor.CostNextUpgrade <= playerState.totalMoney) {
                playerState.SpendMoney(disruptor.CostNextUpgrade);
                disruptor.Upgrade();
                AudioManager.current.PlaySound("upgrade-weapon");
            } else {
                AudioManager.current.PlaySound("upgrade-error");
            }
        }

        public void UpgradeGuns() {
            if (gunsUpgradeCost <= playerState.totalMoney) {
                playerState.SpendMoney(gunsUpgradeCost);
                playerState.UpgradeGuns();
                AudioManager.current.PlaySound("upgrade-gun-slots");
            } else {
                AudioManager.current.PlaySound("upgrade-error");
            }
        }

        void OnEnable() {
            eventChannel.OnShowUpgradePanel.Subscribe(OnShowUpgradePanel);
        }

        void OnDisable() {
            eventChannel.OnShowUpgradePanel.Unsubscribe(OnShowUpgradePanel);
        }

        void Awake() {
            AppIntegrity.AssertPresent(canvas);
            AppIntegrity.AssertPresent(eventSystem);
            AppIntegrity.AssertPresent(machineGun);
            AppIntegrity.AssertPresent(laser);
            AppIntegrity.AssertPresent(missiles);
            AppIntegrity.AssertPresent(disruptor);

            AppIntegrity.AssertPresent(textNumCredits);
            AppIntegrity.AssertPresent(textMaxxedOut);

            AppIntegrity.AssertPresent(textCostPDC);
            AppIntegrity.AssertPresent(textCostLaser);
            AppIntegrity.AssertPresent(textCostMissiles);
            AppIntegrity.AssertPresent(textCostDisruptor);
            AppIntegrity.AssertPresent(textCostGuns);

            AppIntegrity.AssertPresent(textClassPDC);
            AppIntegrity.AssertPresent(textClassLaser);
            AppIntegrity.AssertPresent(textClassMissiles);
            AppIntegrity.AssertPresent(textClassDisruptor);
            AppIntegrity.AssertPresent(textClassGuns);

            AppIntegrity.AssertPresent(buttonUpgradePDC);
            AppIntegrity.AssertPresent(buttonUpgradeLaser);
            AppIntegrity.AssertPresent(buttonUpgradeMissiles);
            AppIntegrity.AssertPresent(buttonUpgradeDisruptor);
            AppIntegrity.AssertPresent(buttonUpgradeGuns);

            uiButtonUpgradePDC = new UIButton(buttonUpgradePDC);
            uiButtonUpgradeLaser = new UIButton(buttonUpgradeLaser);
            uiButtonUpgradeMissiles = new UIButton(buttonUpgradeMissiles);
            uiButtonUpgradeDisruptor = new UIButton(buttonUpgradeDisruptor);
            uiButtonUpgradeGuns = new UIButton(buttonUpgradeGuns);
        }

        void Update() {
            UpdateGUI();
        }

        void UpdateGUI() {
            if (!isShowing) return;

            textNumCredits.text = playerState.totalMoney.ToString() + " CR";

            textClassPDC.text = machineGun.assetClass;
            textClassLaser.text = laser.assetClass;
            textClassMissiles.text = missiles.assetClass;
            textClassDisruptor.text = disruptor.assetClass;
            textClassGuns.text = playerState.hasGunsUpgrade ? "Destructoid" : "Stock";

            textCostPDC.text = machineGun.CanUpgrade ? machineGun.CostNextUpgrade.ToString() + " CR" : "-";
            textCostLaser.text = laser.CanUpgrade ? laser.CostNextUpgrade.ToString() + " CR" : "-";
            textCostMissiles.text = missiles.CanUpgrade ? missiles.CostNextUpgrade.ToString() + " CR" : "-";
            textCostDisruptor.text = disruptor.CanUpgrade ? disruptor.CostNextUpgrade.ToString() + " CR" : "-";
            textCostGuns.text = !playerState.hasGunsUpgrade ? gunsUpgradeCost.ToString() + " CR" : "-";

            EnablifyWeaponButton(machineGun, uiButtonUpgradePDC);
            EnablifyWeaponButton(laser, uiButtonUpgradeLaser);
            EnablifyWeaponButton(missiles, uiButtonUpgradeMissiles);
            EnablifyWeaponButton(disruptor, uiButtonUpgradeDisruptor);
            EnablifyUpgradeGunsButton(uiButtonUpgradeGuns);

            anyMaxxedOut = !machineGun.CanUpgrade || !laser.CanUpgrade || !missiles.CanUpgrade || !disruptor.CanUpgrade;

            if (anyMaxxedOut) {
                textMaxxedOut.enabled = true;
            } else {
                textMaxxedOut.enabled = false;
            }

            MaxxifyText(machineGun.CanUpgrade, textNamePDC, textClassPDC);
            MaxxifyText(laser.CanUpgrade, textNameLaser, textClassLaser);
            MaxxifyText(missiles.CanUpgrade, textNameMissiles, textClassMissiles);
            MaxxifyText(disruptor.CanUpgrade, textNameDisruptor, textClassDisruptor);
            MaxxifyText(!playerState.hasGunsUpgrade, textNameGuns, textClassGuns);
        }

        // yeah, the refactor is here waiting for ya
        // bool CanPurchaseUpgrade(WeaponClass weapon) {
        //     return weapon.CanUpgrade && weapon.CostNextUpgrade < playerState.totalMoney;
        // }

        void EnablifyWeaponButton(WeaponClass weapon, UIButton button) {
            if (weapon.CanUpgrade && weapon.CostNextUpgrade <= playerState.totalMoney) {
                button.Enable();
            } else {
                button.Disable();
            }
        }

        void EnablifyUpgradeGunsButton(UIButton button) {
            if (!playerState.hasGunsUpgrade && gunsUpgradeCost <= playerState.totalMoney) {
                button.Enable();
            } else {
                button.Disable();
            }
        }

        void MaxxifyText(bool canUpgrade, TextMeshProUGUI elem1, TextMeshProUGUI elem2) {
            if (!canUpgrade) {
                elem1.color = maxxedColor;
                elem2.color = maxxedColor;
            }
        }

        void OnShowUpgradePanel() {
            isShowing = true;
            canvas.SetActive(true);
            StartCoroutine(IHackUiModule());
            buttonUpgradePDC.Select();
        }

        void OnHideUpgradePanel() {
            isShowing = false;
            canvas.SetActive(false);
        }

        IEnumerator IHackUiModule() {
            uiModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            uiModule.enabled = false;
            yield return new WaitForFixedUpdate();
            uiModule.enabled = true;
            buttonUpgradePDC.Select();
        }
    }
}

