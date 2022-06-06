using System.Collections;
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
    public enum UpgradeItem {
        PDC,
        Laser,
        Nuke,
        Missiles,
        Disruptor,
        Health,
        Shield,
        Guns,
        Null,
    }

    public class UpgradeUI : MonoBehaviour {

        [SerializeField] EventChannelSO eventChannel;
        [SerializeField] GameStateSO gameState;
        [SerializeField] PlayerStateSO playerState;
        [SerializeField] GameObject canvas;
        [SerializeField] EventSystem eventSystem;

        [Space]
        [SerializeField] int gunsUpgradeCost = 20000;
        [SerializeField] int upgradePointBonus = 10000;

        [Space]
        [SerializeField] WeaponClass machineGun;
        [SerializeField] WeaponClass laser;
        [SerializeField] WeaponClass missiles;
        [SerializeField] WeaponClass disruptor;
        [SerializeField] WeaponClass nuke;

        [Space]
        [SerializeField] TextMeshProUGUI textNumCredits;
        [SerializeField] TextMeshProUGUI textMaxxedOut;

        [Space]
        [SerializeField] TextMeshProUGUI textNamePDC;
        [SerializeField] TextMeshProUGUI textNameLaser;
        [SerializeField] TextMeshProUGUI textNameMissiles;
        [SerializeField] TextMeshProUGUI textNameDisruptor;
        [SerializeField] TextMeshProUGUI textNameNuke;
        [SerializeField] TextMeshProUGUI textNameGuns;

        [Space]
        [SerializeField] TextMeshProUGUI textClassPDC;
        [SerializeField] TextMeshProUGUI textClassLaser;
        [SerializeField] TextMeshProUGUI textClassMissiles;
        [SerializeField] TextMeshProUGUI textClassDisruptor;
        [SerializeField] TextMeshProUGUI textClassNuke;
        [SerializeField] TextMeshProUGUI textClassGuns;

        [Space]
        [SerializeField] TextMeshProUGUI textCostPDC;
        [SerializeField] TextMeshProUGUI textCostLaser;
        [SerializeField] TextMeshProUGUI textCostMissiles;
        [SerializeField] TextMeshProUGUI textCostDisruptor;
        [SerializeField] TextMeshProUGUI textCostNuke;
        [SerializeField] TextMeshProUGUI textCostGuns;

        [Space]
        [SerializeField] Button buttonUpgradePDC;
        [SerializeField] Button buttonUpgradeLaser;
        [SerializeField] Button buttonUpgradeMissiles;
        [SerializeField] Button buttonUpgradeDisruptor;
        [SerializeField] Button buttonUpgradeNuke;
        [SerializeField] Button buttonUpgradeGuns;

        [Space]
        [SerializeField] GameObject upgradeSlotsPDC;
        [SerializeField] GameObject upgradeSlotsLaser;
        [SerializeField] GameObject upgradeSlotsMissiles;
        [SerializeField] GameObject upgradeSlotsDisruptor;
        [SerializeField] GameObject upgradeSlotsNuke;
        [SerializeField] GameObject upgradeSlotsGuns;

        [Space]
        [SerializeField] GameObject textDetailContent;
        [SerializeField] TextMeshProUGUI textDetailTitle;
        [SerializeField] TextMeshProUGUI textDetailCurrentClass;
        [SerializeField] TextMeshProUGUI textDetailNextClass;
        [SerializeField] TextMeshProUGUI textDetailDescription;
        [SerializeField] GameObject textDetailNotEnoughCredits;

        [Space]
        [SerializeField] Color maxxedColor;
        [SerializeField] Color slotActiveColorBG;
        [SerializeField] Color slotActiveColor;
        [SerializeField] Color slotInactiveColorBG;
        [SerializeField] Color slotInactiveColor;
        [SerializeField] Color slotHiddenColor;

        UIButton uiButtonUpgradePDC;
        UIButton uiButtonUpgradeLaser;
        UIButton uiButtonUpgradeMissiles;
        UIButton uiButtonUpgradeDisruptor;
        UIButton uiButtonUpgradeNuke;
        UIButton uiButtonUpgradeGuns;

        UpgradeSlot upgradeSlotPDC;
        UpgradeSlot upgradeSlotLaser;
        UpgradeSlot upgradeSlotMissiles;
        UpgradeSlot upgradeSlotDisruptor;
        UpgradeSlot upgradeSlotNuke;
        UpgradeSlot upgradeSlotGuns;

        InputSystemUIInputModule uiModule;

        bool isShowing;
        UpgradeItem currentUpgradeSelected = UpgradeItem.PDC;
        // bool anyMaxxedOut;

        public void OnDone() {
            OnHideUpgradePanel();
            GameManager.current.GotoNextLevel();
        }

        public void UpgradePDC() {
            if (machineGun.CanUpgrade && machineGun.CostNextUpgrade <= playerState.totalMoney) {
                playerState.SpendMoney(machineGun.CostNextUpgrade);
                machineGun.Upgrade();
                upgradeSlotPDC.SetUpgradeLevel(machineGun.CurrentUpgradeLevel);
                AudioManager.current.PlaySound("upgrade-weapon");
            } else {
                AudioManager.current.PlaySound("upgrade-error");
            }
        }

        public void UpgradeLZR() {
            if (laser.CanUpgrade && laser.CostNextUpgrade <= playerState.totalMoney) {
                playerState.SpendMoney(laser.CostNextUpgrade);
                laser.Upgrade();
                upgradeSlotLaser.SetUpgradeLevel(laser.CurrentUpgradeLevel);
                AudioManager.current.PlaySound("upgrade-weapon");
            } else {
                AudioManager.current.PlaySound("upgrade-error");
            }
        }

        public void UpgradeMissiles() {
            if (missiles.CanUpgrade && missiles.CostNextUpgrade <= playerState.totalMoney) {
                playerState.SpendMoney(missiles.CostNextUpgrade);
                missiles.Upgrade();
                upgradeSlotMissiles.SetUpgradeLevel(missiles.CurrentUpgradeLevel);
                AudioManager.current.PlaySound("upgrade-weapon");
            } else {
                AudioManager.current.PlaySound("upgrade-error");
            }
        }

        public void UpgradeDisruptor() {
            if (disruptor.CanUpgrade && disruptor.CostNextUpgrade <= playerState.totalMoney) {
                playerState.SpendMoney(disruptor.CostNextUpgrade);
                disruptor.Upgrade();
                upgradeSlotDisruptor.SetUpgradeLevel(disruptor.CurrentUpgradeLevel);
                AudioManager.current.PlaySound("upgrade-weapon");
            } else {
                AudioManager.current.PlaySound("upgrade-error");
            }
        }

        public void UpgradeNuke() {
            if (nuke.CanUpgrade && nuke.CostNextUpgrade <= playerState.totalMoney) {
                playerState.SpendMoney(nuke.CostNextUpgrade);
                nuke.Upgrade();
                upgradeSlotNuke.SetUpgradeLevel(nuke.CurrentUpgradeLevel);
                AudioManager.current.PlaySound("upgrade-weapon");
            } else {
                AudioManager.current.PlaySound("upgrade-error");
            }
        }

        public void UpgradeGuns() {
            if (gunsUpgradeCost <= playerState.totalMoney) {
                playerState.SpendMoney(gunsUpgradeCost);
                playerState.UpgradeGuns();
                upgradeSlotGuns.SetUpgradeLevel(1);
                AudioManager.current.PlaySound("upgrade-gun-slots");
            } else {
                AudioManager.current.PlaySound("upgrade-error");
            }
        }

        public void MenuSelectLaser() {
            OnMenuSelect(UpgradeItem.Laser);
        }
        public void MenuSelectPDC() {
            OnMenuSelect(UpgradeItem.PDC);
        }
        public void MenuSelectNuke() {
            OnMenuSelect(UpgradeItem.Nuke);
        }
        public void MenuSelectMissile() {
            OnMenuSelect(UpgradeItem.Missiles);
        }
        public void MenuSelectDisruptor() {
            OnMenuSelect(UpgradeItem.Disruptor);
        }
        public void MenuSelectGuns() {
            OnMenuSelect(UpgradeItem.Guns);
        }
        public void MenuSelectHealth() {
            OnMenuSelect(UpgradeItem.Health);
        }
        public void MenuSelectShield() {
            OnMenuSelect(UpgradeItem.Shield);
        }
        public void MenuSelectNull() {
            OnMenuSelect(UpgradeItem.Null);
        }
        void OnMenuSelect(UpgradeItem item) {
            currentUpgradeSelected = item;
            AudioManager.current.PlaySound("MenuSwitch");
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
            AppIntegrity.AssertPresent(nuke);
            AppIntegrity.AssertPresent(disruptor);

            AppIntegrity.AssertPresent(textNumCredits);
            // AppIntegrity.AssertPresent(textMaxxedOut);

            AppIntegrity.AssertPresent(textCostPDC);
            AppIntegrity.AssertPresent(textCostLaser);
            AppIntegrity.AssertPresent(textCostMissiles);
            AppIntegrity.AssertPresent(textCostDisruptor);
            AppIntegrity.AssertPresent(textCostNuke);
            AppIntegrity.AssertPresent(textCostGuns);

            AppIntegrity.AssertPresent(textClassPDC);
            AppIntegrity.AssertPresent(textClassLaser);
            AppIntegrity.AssertPresent(textClassMissiles);
            AppIntegrity.AssertPresent(textClassDisruptor);
            AppIntegrity.AssertPresent(textClassNuke);
            AppIntegrity.AssertPresent(textClassGuns);

            AppIntegrity.AssertPresent(buttonUpgradePDC);
            AppIntegrity.AssertPresent(buttonUpgradeLaser);
            AppIntegrity.AssertPresent(buttonUpgradeMissiles);
            AppIntegrity.AssertPresent(buttonUpgradeDisruptor);
            AppIntegrity.AssertPresent(buttonUpgradeNuke);
            AppIntegrity.AssertPresent(buttonUpgradeGuns);

            uiButtonUpgradePDC = new UIButton(buttonUpgradePDC);
            uiButtonUpgradeLaser = new UIButton(buttonUpgradeLaser);
            uiButtonUpgradeMissiles = new UIButton(buttonUpgradeMissiles);
            uiButtonUpgradeDisruptor = new UIButton(buttonUpgradeDisruptor);
            uiButtonUpgradeNuke = new UIButton(buttonUpgradeNuke);
            uiButtonUpgradeGuns = new UIButton(buttonUpgradeGuns);

            upgradeSlotPDC = InitUpgradeSlot(upgradeSlotsPDC, 0, machineGun.MaxUpgradeLevel);
            upgradeSlotLaser = InitUpgradeSlot(upgradeSlotsLaser, 0, laser.MaxUpgradeLevel);
            upgradeSlotMissiles = InitUpgradeSlot(upgradeSlotsMissiles, 0, missiles.MaxUpgradeLevel);
            upgradeSlotDisruptor = InitUpgradeSlot(upgradeSlotsDisruptor, 0, disruptor.MaxUpgradeLevel);
            upgradeSlotNuke = InitUpgradeSlot(upgradeSlotsNuke, 0, nuke.MaxUpgradeLevel);
            upgradeSlotGuns = InitUpgradeSlot(upgradeSlotsGuns, 0, 1);

            buttonUpgradePDC.Select();
        }

        void Update() {
            UpdateGUI();
        }

        UpgradeSlot InitUpgradeSlot(GameObject upgradeSlotsGO, int initialUpgradeLevel, int maxUpgradeLevel) {
            UpgradeSlot slot = new UpgradeSlot(upgradeSlotsGO, initialUpgradeLevel, maxUpgradeLevel);
            slot.SetActiveColor(slotActiveColor);
            slot.SetActiveColorBG(slotActiveColorBG);
            slot.SetInactiveColor(slotInactiveColor);
            slot.SetInactiveColorBG(slotInactiveColorBG);
            slot.SetHiddenColor(slotHiddenColor);
            return slot;
        }

        void UpdateGUI() {
            if (!isShowing) return;

            textNumCredits.text = playerState.totalMoney.ToString() + " CR";

            textClassPDC.text = machineGun.assetClass;
            textClassLaser.text = laser.assetClass;
            textClassMissiles.text = missiles.assetClass;
            textClassDisruptor.text = disruptor.assetClass;
            textClassNuke.text = nuke.assetClass;
            textClassGuns.text = playerState.hasGunsUpgrade ? "Destructoid" : "Stock";

            textCostPDC.text = machineGun.CanUpgrade ? machineGun.CostNextUpgrade.ToString() + " CR" : "-";
            textCostLaser.text = laser.CanUpgrade ? laser.CostNextUpgrade.ToString() + " CR" : "-";
            textCostMissiles.text = missiles.CanUpgrade ? missiles.CostNextUpgrade.ToString() + " CR" : "-";
            textCostDisruptor.text = disruptor.CanUpgrade ? disruptor.CostNextUpgrade.ToString() + " CR" : "-";
            textCostNuke.text = nuke.CanUpgrade ? nuke.CostNextUpgrade.ToString() + " CR" : "-";
            textCostGuns.text = !playerState.hasGunsUpgrade ? gunsUpgradeCost.ToString() + " CR" : "-";

            EnablifyWeaponButton(machineGun, uiButtonUpgradePDC);
            EnablifyWeaponButton(laser, uiButtonUpgradeLaser);
            EnablifyWeaponButton(missiles, uiButtonUpgradeMissiles);
            EnablifyWeaponButton(disruptor, uiButtonUpgradeDisruptor);
            EnablifyWeaponButton(nuke, uiButtonUpgradeNuke);
            EnablifyUpgradeGunsButton(uiButtonUpgradeGuns);

            // anyMaxxedOut = !machineGun.CanUpgrade || !laser.CanUpgrade || !missiles.CanUpgrade || !disruptor.CanUpgrade;
            // if (anyMaxxedOut) {
            //     textMaxxedOut.enabled = true;
            // } else {
            //     textMaxxedOut.enabled = false;
            // }

            MaxxifyText(machineGun.CanUpgrade, textNamePDC, textClassPDC);
            MaxxifyText(laser.CanUpgrade, textNameLaser, textClassLaser);
            MaxxifyText(missiles.CanUpgrade, textNameMissiles, textClassMissiles);
            MaxxifyText(disruptor.CanUpgrade, textNameDisruptor, textClassDisruptor);
            MaxxifyText(nuke.CanUpgrade, textNameNuke, textClassNuke);
            MaxxifyText(!playerState.hasGunsUpgrade, textNameGuns, textClassGuns);

            if (currentUpgradeSelected == UpgradeItem.Null) {
                textDetailContent.SetActive(false);
            } else {
                textDetailContent.SetActive(true);
                textDetailTitle.text = GetDetailTitle(currentUpgradeSelected);
                textDetailCurrentClass.text = GetCurrentClassText(currentUpgradeSelected);
                textDetailNextClass.text = GetNextClassText(currentUpgradeSelected);
                textDetailDescription.text = GetDescriptionText(currentUpgradeSelected);
                if (HasEnoughUpgradeCredits(currentUpgradeSelected)) {
                    textDetailNotEnoughCredits.SetActive(false);
                } else {
                    textDetailNotEnoughCredits.SetActive(true);
                }
            }
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

        void RefreshUpgrade(UpgradeSlot slot, int currentLevel) {
            slot.SetUpgradeLevel(currentLevel);
        }

        void OnShowUpgradePanel() {
            isShowing = true;
            canvas.SetActive(true);
            StartCoroutine(IHackUiModule());
            buttonUpgradePDC.Select();
            RefreshUpgrade(upgradeSlotPDC, machineGun.CurrentUpgradeLevel);
            RefreshUpgrade(upgradeSlotLaser, laser.CurrentUpgradeLevel);
            RefreshUpgrade(upgradeSlotMissiles, missiles.CurrentUpgradeLevel);
            RefreshUpgrade(upgradeSlotDisruptor, disruptor.CurrentUpgradeLevel);
            RefreshUpgrade(upgradeSlotNuke, nuke.CurrentUpgradeLevel);
            RefreshUpgrade(upgradeSlotGuns, playerState.hasGunsUpgrade ? 1 : 0);
        }

        void OnHideUpgradePanel() {
            isShowing = false;
            canvas.SetActive(false);
        }

        string GetDetailTitle(UpgradeItem item) {
            switch (item) {
                case UpgradeItem.PDC:
                    return "PDC";
                case UpgradeItem.Laser:
                    return "LZR";
                case UpgradeItem.Nuke:
                    return "NUKE";
                case UpgradeItem.Missiles:
                    return "MISSILES";
                case UpgradeItem.Disruptor:
                    return "DISRUPTOR";
                case UpgradeItem.Health:
                    return "HULL ARMOR";
                case UpgradeItem.Shield:
                    return "SHIELD";
                case UpgradeItem.Guns:
                    return "GUNS";
                default:
                    return "";
            }
        }

        string GetCurrentClassText(UpgradeItem item) {
            switch (item) {
                case UpgradeItem.PDC:
                    return machineGun.assetClass;
                case UpgradeItem.Laser:
                    return laser.assetClass;
                case UpgradeItem.Nuke:
                    return nuke.assetClass;
                case UpgradeItem.Missiles:
                    return missiles.assetClass;
                case UpgradeItem.Disruptor:
                    return disruptor.assetClass;
                case UpgradeItem.Health:
                    return "Nanotitanium Composite Alloy Hull";
                case UpgradeItem.Shield:
                    return "Titan MK VII";
                case UpgradeItem.Guns:
                    return playerState.hasGunsUpgrade ? "Dual Cannon Mounts" : "Stock Cannon Mounts";
                default:
                    return "";
            }
        }

        string GetNextClassText(UpgradeItem item) {
            switch (item) {
                case UpgradeItem.PDC:
                    return machineGun.nextAssetClass;
                case UpgradeItem.Laser:
                    return laser.nextAssetClass;
                case UpgradeItem.Nuke:
                    return nuke.nextAssetClass;
                case UpgradeItem.Missiles:
                    return missiles.nextAssetClass;
                case UpgradeItem.Disruptor:
                    return disruptor.nextAssetClass;
                case UpgradeItem.Health:
                    // TODO: ADD STATE TO HANDLE
                    return "Nanotitanium Composite Alloy Hull";
                case UpgradeItem.Shield:
                    // TODO: ADD STATE TO HANDLE
                    return "Titan MK VII";
                case UpgradeItem.Guns:
                    return playerState.hasGunsUpgrade ? "-" : "Dual Cannon Mounts";
                default:
                    return "";
            }
        }

        string GetDescriptionText(UpgradeItem item) {
            switch (item) {
                case UpgradeItem.PDC:
                    return machineGun.nextDescription;
                case UpgradeItem.Laser:
                    return laser.nextDescription;
                case UpgradeItem.Nuke:
                    return nuke.nextDescription;
                case UpgradeItem.Missiles:
                    return missiles.nextDescription;
                case UpgradeItem.Disruptor:
                    return disruptor.nextDescription;
                case UpgradeItem.Health:
                    // TODO: ADD STATE TO HANDLE
                    return "Nanotitanium Composite Alloy Hull";
                case UpgradeItem.Shield:
                    // TODO: ADD STATE TO HANDLE
                    return "Titan MK VII";
                case UpgradeItem.Guns:
                    return playerState.hasGunsUpgrade
                        ? "Upgrade equipped. Double the munitions, double the fun"
                        : "Adds a second cannon to each existing mount point on your ship";
                default:
                    return "";
            }
        }

        bool HasEnoughUpgradeCredits(UpgradeItem item) {
            switch (item) {
                case UpgradeItem.PDC:
                    return !machineGun.CanUpgrade || machineGun.CostNextUpgrade <= playerState.totalMoney;
                case UpgradeItem.Laser:
                    return !laser.CanUpgrade || laser.CostNextUpgrade <= playerState.totalMoney;
                case UpgradeItem.Nuke:
                    return !nuke.CanUpgrade || nuke.CostNextUpgrade <= playerState.totalMoney;
                case UpgradeItem.Missiles:
                    return !missiles.CanUpgrade || missiles.CostNextUpgrade <= playerState.totalMoney;
                case UpgradeItem.Disruptor:
                    return !disruptor.CanUpgrade || disruptor.CostNextUpgrade <= playerState.totalMoney;
                case UpgradeItem.Health:
                    // TODO: ADD STATE TO HANDLE
                    return false;
                case UpgradeItem.Shield:
                    // TODO: ADD STATE TO HANDLE
                    return false;
                case UpgradeItem.Guns:
                    return playerState.hasGunsUpgrade || gunsUpgradeCost <= playerState.totalMoney;
                default:
                    return true;
            }
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

