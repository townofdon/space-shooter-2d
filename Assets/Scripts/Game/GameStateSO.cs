using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game {

    // public enum GameMode {
    //     Battle,
    //     FreeRoam,
    //     Docked,
    // }
    public enum GameMode {
        Campaign,
        Arcade,
        Demo,
    }

    public enum GameDifficulty {
        Easy,
        Medium,
        Hard,
        Insane,
    }

    [CreateAssetMenu(fileName = "GameState", menuName = "ScriptableObjects/GameState", order = 0)]
    public class GameStateSO : ScriptableObject {

        [SerializeField] int _initialLivesEasy = 5;
        [SerializeField] int _initialLivesMedium = 5;
        [SerializeField] int _initialLivesHard = 4;
        [SerializeField] int _initialLivesInsane = 3;

        [Space]

        [SerializeField] int _initialPoints = 0;

        [Space]

        [SerializeField] GameDifficulty _initialDifficulty = GameDifficulty.Medium;
        [SerializeField] GameDifficulty _difficulty = GameDifficulty.Medium;

        GameMode _mode;
        int _lives = 0;
        int _pointsInBank;
        int _pointsGained; // points accumulated since round start - when player dies the number goes to zero
        int _numEnemiesKilled = 0;

        public int lives => _lives;
        public int totalPoints => _pointsInBank + _pointsGained;
        public int pointsGained => _pointsGained;
        public int numEnemiesKilled => _numEnemiesKilled;
        public GameMode mode => _mode;
        public GameDifficulty difficulty => _difficulty;

        public void Init() {
            NewGame();
            _mode = GameMode.Campaign;
            _difficulty = _initialDifficulty;
        }

        public void NewGame() {
            _pointsInBank = _initialPoints;
            _pointsGained = 0;
            _numEnemiesKilled = 0;
            _lives = GetInitialLives();
        }

        public void GainLife() {
            _lives++;
        }

        public void LoseLife() {
            if (_mode == GameMode.Arcade) return;
            _lives--;
            _lives = Mathf.Max(_lives, 0);
        }

        public void SetMode(GameMode value) {
            _mode = value;
        }

        public void SetDifficulty(GameDifficulty value) {
            _difficulty = value;
            _lives = GetInitialLives();
        }

        public void StorePoints() {
            _pointsInBank += _pointsGained;
            _pointsGained = 0;
            _pointsInBank = Mathf.Max(_pointsInBank, 0);
        }

        public void GainPoints(int value) {
            _pointsGained += value;
        }

        public void LosePoints() {
            // if points gained goes negative, then the bank will need to cover the balance at the end of the level.
            _pointsGained -= GetPointsToLose();
            if (totalPoints < 0) {
                _pointsInBank = 0;
                _pointsGained = 0;
            }
        }

        public void IncrementEnemiesKilled() {
            _numEnemiesKilled++;
        }

        public int GetPointsToLose() {
            return (int)(Mathf.Clamp(_pointsGained * 0.5f, 1000f, 10000f));
        }

        int GetInitialLives() {
            switch (_difficulty) {
                case GameDifficulty.Easy:
                    return _initialLivesEasy;
                case GameDifficulty.Medium:
                    return _initialLivesMedium;
                case GameDifficulty.Hard:
                    return _initialLivesHard;
                case GameDifficulty.Insane:
                    return _initialLivesInsane;
                default:
                    return 0;
            }
        }
    }
}
