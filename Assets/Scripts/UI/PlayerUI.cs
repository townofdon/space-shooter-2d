using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Player;
using Game;
using Audio;

namespace UI {

    public class PlayerUI : MonoBehaviour {

        [Header("General Settings")]
        [Space]
        [SerializeField] GameObject canvas;
        [SerializeField] GameStateSO gameState;
        [SerializeField] PlayerStateSO playerState;

        [Header("Health UI")]
        [Space]
        [SerializeField] Slider shieldSlider;
        [SerializeField] Image shieldIcon;
        [SerializeField] Image shieldBarBg;
        [SerializeField] Image shieldBarFill;
        [SerializeField] Color shieldColor;
        [SerializeField] Gradient shieldAlarmGradient;
        [SerializeField] Slider healthSlider;
        [SerializeField] Image healthIcon;
        [SerializeField] Image healthBarBg;
        [SerializeField] Image healthBarFill;
        [SerializeField] Gradient healthGradient;

        [Header("Weapon UI")]
        [Space]
        [SerializeField] Color activeBGColor;
        [SerializeField] Color activeFGColor;
        [SerializeField] Color inactiveBGColor;
        [SerializeField] Color inactiveFGColor;
        [SerializeField] Color warningActiveBGColor;
        [SerializeField] Color warningActiveFGColor;
        [SerializeField] Color warningInactiveBGColor;
        [SerializeField] Color warningInactiveFGColor;
        [SerializeField] GameObject reloading;
        [SerializeField] Image nukeBG;
        [SerializeField] Image nukeIcon;
        [SerializeField] TextMeshProUGUI nukeText;
        [SerializeField] Image missileBG;
        [SerializeField] Image missileIcon;
        [SerializeField] TextMeshProUGUI missileText;
        [SerializeField] Image laserBG;
        [SerializeField] Image laserIcon;
        [SerializeField] TextMeshProUGUI laserText;
        [SerializeField] Image pdcBG;
        [SerializeField] Image pdcIcon;
        [SerializeField] TextMeshProUGUI pdcText;
        [SerializeField] TextMeshProUGUI clipAmmoText;
        [SerializeField] TextMeshProUGUI reserveAmmoText;

        [Header("Points UI")]
        [Space]
        [SerializeField] TextMeshProUGUI scoreText;
        [SerializeField] TextMeshProUGUI moneyText;
        [SerializeField] Sound scoreSound;

        // state
        enum ShieldState {
            nominal,
            alarm,
        }
        ShieldState shieldState;
        Coroutine shieldAlarm;

        // cached
        PlayerGeneral player;
        PlayerWeapons weapons;

        float lastHealth = 0f;
        float lastShield = 0f;
        int tempPoints = 0;
        int tempMoney = 0;
        int tempStep = 1;

        void Start() {
            FindPlayer();
            scoreSound.Init(this);
            tempPoints = gameState.totalPoints;
            tempMoney = playerState.totalMoney;
        }

        void Update() {
            if (player == null || !player.isAlive) {
                FindPlayer();
                canvas.SetActive(false);
            } else {
                canvas.SetActive(true);
                DrawHealthUI();
                DrawWeaponsUI();
                DrawPointsUI();
            }
        }

        void FindPlayer() {
            player = PlayerUtils.FindPlayer();
            if (player != null) weapons = player.GetComponent<PlayerWeapons>();
        }

        void DrawHealthUI() {
            if (player.health != lastHealth) {
                lastHealth = player.health;
                healthSlider.value = player.healthPct;
                SetHealthColor(player.healthPct);
            }

            if (player.shield != lastShield) {
                lastShield = player.shield;
                shieldSlider.value = player.shieldPct;
                SetShieldState(player.shieldPct);
                if (shieldState == ShieldState.alarm) {
                    if (shieldAlarm == null) shieldAlarm = StartCoroutine(IShieldAlarm());
                } else {
                    SetShieldColor(shieldColor);
                }
            }
        }

        void SetHealthColor(float value) {
            healthIcon.color = healthGradient.Evaluate(value);
            healthBarBg.color = healthGradient.Evaluate(value);
            healthBarFill.color = healthGradient.Evaluate(value);
        }

        void SetShieldColor(Color color) {
            shieldIcon.color = color;
            shieldBarBg.color = color;
            shieldBarFill.color = color;
        }

        void SetShieldState(float value) {
            if (value > 0) {
                shieldState = ShieldState.nominal;
            } else {
                shieldState = ShieldState.alarm;
            }
        }

        IEnumerator IShieldAlarm() {
            int i = 1;
            while (shieldState == ShieldState.alarm) {
                SetShieldColor(shieldAlarmGradient.Evaluate(i));
                i = i == 1 ? 0 : 1;
                yield return new WaitForSeconds(0.2f);
            }
            shieldAlarm = null;
        }

        void DrawWeaponsUI() {
            if (GameManager.isPaused) return;
            if (weapons == null) return;
            if (weapons.primaryWeaponType == Weapons.WeaponType.Laser) {
                SetWeaponAsActive(laserBG, laserIcon, laserText);
                SetWeaponAsInactive(pdcBG, pdcIcon, pdcText, weapons.machineGunHasAmmo);
            } else {
                SetWeaponAsActive(pdcBG, pdcIcon, pdcText, weapons.machineGunHasAmmo);
                SetWeaponAsInactive(laserBG, laserIcon, laserText);
            }
            if (weapons.secondaryWeaponType == Weapons.WeaponType.Nuke) {
                SetWeaponAsActive(nukeBG, nukeIcon, nukeText, weapons.nukeHasAmmo);
                SetWeaponAsInactive(missileBG, missileIcon, missileText, weapons.missileHasAmmo);
            } else {
                SetWeaponAsActive(missileBG, missileIcon, missileText, weapons.missileHasAmmo);
                SetWeaponAsInactive(nukeBG, nukeIcon, nukeText, weapons.nukeHasAmmo);
            }

            if (weapons.primaryWeaponReloading) {
                reloading.SetActive(true);
            } else {
                reloading.SetActive(false);
            }
            // ammo
            clipAmmoText.text = weapons.primaryWeaponClipAmmo;
            reserveAmmoText.text = weapons.primaryWeaponReserveAmmo;
            nukeText.text = weapons.nukeAmmo;
            missileText.text = weapons.missileAmmo;
        }

        void SetWeaponAsActive(Image bg = null, Image icon = null, TextMeshProUGUI text = null, bool hasAmmo = true) {
            if (hasAmmo) {
                if (bg != null) bg.color = activeBGColor;
                if (icon != null) icon.color = activeFGColor;
                if (text != null) text.color = activeFGColor;
            } else {
                if (bg != null) bg.color = warningActiveBGColor;
                if (icon != null) icon.color = warningActiveFGColor;
                if (text != null) text.color = warningActiveFGColor;
            }
        }

        void SetWeaponAsInactive(Image bg = null, Image icon = null, TextMeshProUGUI text = null, bool hasAmmo = true) {
            if (hasAmmo) {
                if (bg != null) bg.color = inactiveBGColor;
                if (icon != null) icon.color = inactiveFGColor;
                if (text != null) text.color = inactiveFGColor;
            } else {
                if (bg != null) bg.color = warningInactiveBGColor;
                if (icon != null) icon.color = warningInactiveFGColor;
                if (text != null) text.color = warningInactiveFGColor;
            }
        }

        void DrawPointsUI() {
            if (tempPoints < gameState.totalPoints || tempMoney < playerState.totalMoney) {
                scoreSound.Play();
            }
            if (tempPoints < gameState.totalPoints) {
                tempStep = getStep(gameState.totalPoints - tempPoints);
                tempPoints = Mathf.Min(tempPoints + tempStep, gameState.totalPoints);
            }
            if (tempMoney < playerState.totalMoney) {
                tempStep = getStep(playerState.totalMoney - tempMoney);
                tempMoney = Mathf.Min(tempMoney + tempStep, playerState.totalMoney);
            }
            if (tempPoints > gameState.totalPoints) {
                tempStep = getStep(tempPoints - gameState.totalPoints);
                tempPoints = Mathf.Max(tempPoints - tempStep, gameState.totalPoints);
            }
            if (tempMoney > playerState.totalMoney) {
                tempStep = getStep(tempMoney - playerState.totalMoney);
                tempMoney = Mathf.Max(tempMoney - tempStep, playerState.totalMoney);
            }
            scoreText.text = tempPoints.ToString("00000000");
            moneyText.text = tempMoney.ToString("0000000") + " CR";
        }

        int getStep(int diff) {
            if (diff >= 100000) return 5000;
            if (diff >= 10000) return 500;
            if (diff >= 1000) return 50;
            return 1;
        }
    }
}
