using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        PlayerInputControlMode _controlMode;
        PlayerShipColor _shipColor;
        float _maxHealth;
        float _maxShield;
        int _moneyInBank;
        int _moneyGained; // money accumulated since round start - when player dies the number goes to zero
        int _numDeaths;

        public PlayerShipColor shipColor => _shipColor;
        public PlayerInputControlMode controlMode => _controlMode;
        public float maxHealth => _maxHealth;
        public float maxShield => _maxShield;
        public int totalMoney => _moneyInBank + _moneyGained;
        public int numDeaths => _numDeaths;

        public void Init() {
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
            _moneyGained = Mathf.Max(_moneyGained, 0);
        }

        public void LoseMoney() {
            _moneyGained = 0;
        }

        public void IncrementDeaths() {
            _numDeaths++;
        }
    }
}
