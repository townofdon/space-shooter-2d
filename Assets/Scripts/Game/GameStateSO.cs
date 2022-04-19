using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game {

    public enum GameMode {
        Battle,
        FreeRoam,
        Docked,
    }

    public enum GameDifficulty {
        Easy,
        Medium,
        Hard,
        Insane,
    }

    [CreateAssetMenu(fileName = "GameState", menuName = "ScriptableObjects/GameState", order = 0)]
    public class GameStateSO : ScriptableObject {
        [SerializeField] int _initialPoints = 0;

        GameMode _mode;
        [SerializeField] GameDifficulty _difficulty;

        int _pointsInBank;
        int _pointsGained; // points accumulated since round start - when player dies the number goes to zero
        int _numEnemiesKilled = 0;

        public int totalPoints => _pointsInBank + _pointsGained;
        public int pointsGained => _pointsGained;
        public int numEnemiesKilled => _numEnemiesKilled;
        public GameMode mode => _mode;
        public GameDifficulty difficulty => _difficulty;

        public void Init() {
            Debug.Log("GAME STATE INIT");
            _pointsInBank = _initialPoints;
            _pointsGained = 0;
            _numEnemiesKilled = 0;
            _mode = GameMode.Battle;
            _difficulty = GameDifficulty.Medium;
        }

        public void SetMode(GameMode value) {
            _mode = value;
        }

        public void SetDifficulty(GameDifficulty value) {
            _difficulty = value;
        }

        public void StorePoints() {
            _pointsInBank += _pointsGained;
            _pointsGained = 0;
        }

        public void GainPoints(int value) {
            _pointsGained += value;
        }

        public void LosePoints() {
            _pointsGained = 0;
        }

        public void IncrementEnemiesKilled() {
            _numEnemiesKilled++;
        }
    }
}
