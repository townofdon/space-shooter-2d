using UnityEngine;

using Weapons;

namespace Player {

    public enum PlayerShipColor {
        Red,
        Yellow,
        Blue,
        Green,
    }

    public enum PlayerInputControlMode {
        Player,
        GameBrain,
        Disabled,
    }

    [CreateAssetMenu(fileName = "PlayerState", menuName = "ScriptableObjects/PlayerState", order = 0)]
    public class PlayerStateSO : ScriptableObject {
        [SerializeField] float _initialHealth = 50f;
        [SerializeField] float _initialShield = 100f;
        [SerializeField] int _initialMoney = 500;
        [SerializeField] bool _hasGunsUpgrade = false;

        WeaponClass _primaryWeapon;
        WeaponClass _secondaryWeapon;
        PlayerInputControlMode _controlMode;
        PlayerShipColor _shipColor;
        float _maxHealth;
        float _maxShield;
        int _moneyInBank;
        int _moneyGained; // money accumulated since round start - when player dies the number goes to zero
        int _numDeaths;

        public WeaponClass primaryWeapon => _primaryWeapon;
        public WeaponClass secondaryWeapon => _secondaryWeapon;
        public PlayerInputControlMode controlMode => _controlMode;
        public PlayerShipColor shipColor => _shipColor;
        public float maxHealth => _maxHealth;
        public float maxShield => _maxShield;
        public int totalMoney => _moneyInBank + _moneyGained;
        public int numDeaths => _numDeaths;
        public bool hasGunsUpgrade => _hasGunsUpgrade;

        public void Init() {
            _primaryWeapon = null;
            _secondaryWeapon = null;
            _maxHealth = _initialHealth;
            _maxShield = _initialShield;
            _moneyInBank = _initialMoney;
            _moneyGained = 0;
            _numDeaths = 0;
            _controlMode = PlayerInputControlMode.Player;
        }

        public void ResetAfterDeath() {
            LoseMoney();
        }

        public void SetInputControlMode(PlayerInputControlMode value) {
            _controlMode = value;
        }

        public void SetShipColor(PlayerShipColor value) {
            _shipColor = value;
        }

        public void SetPrimaryWeapon(WeaponClass weapon) {
            _primaryWeapon = weapon;
        }

        public void SetSecondaryWeapon(WeaponClass weapon) {
            _secondaryWeapon = weapon;
        }

        public void ResetGunsUpgrade() {
            _hasGunsUpgrade = false;
        }

        public void UpgradeGuns() {
            _hasGunsUpgrade = true;
        }

        public void UpgradeHealth(float value) {
            _maxHealth = Mathf.Max(_maxHealth, value);
        }

        public void UpgradeShield(float value) {
            _maxShield = Mathf.Max(_maxShield, value);
        }

        public void DepositMoney() {
            _moneyInBank += _moneyGained;
            _moneyGained = 0;
        }

        public void GainMoney(int value) {
            _moneyGained += value;
        }

        public void SpendMoney(int value) {
            _moneyGained -= value;
            if (_moneyGained < 0) _moneyInBank += _moneyGained;
            _moneyGained = Mathf.Max(_moneyGained, 0);
            _moneyInBank = Mathf.Max(_moneyInBank, 0);
        }

        public void LoseMoney() {
            _moneyGained = 0;
        }

        public void IncrementDeaths() {
            _numDeaths++;
        }
    }
}
