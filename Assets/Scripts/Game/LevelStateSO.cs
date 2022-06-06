
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Core;
using Game;

[CreateAssetMenu(fileName = "LevelState", menuName = "ScriptableObjects/LevelState", order = 0)]
public class LevelStateSO : ScriptableObject {
    [SerializeField] List<SceneReference> _campaignLevels = new List<SceneReference>();
    [SerializeField] List<SceneReference> _arcadeLevels = new List<SceneReference>();
    [SerializeField] List<SceneReference> _demoLevels = new List<SceneReference>();
    [SerializeField] SceneReference _mainMenu;
    [SerializeField] SceneReference _tutorialLevel;
    [SerializeField] SceneReference _warpScene;
    [SerializeField] SceneReference _upgradeScene;
    [SerializeField] SceneReference _winLoseScreen;

    List<SceneReference> levels = new List<SceneReference>();

    int _currentLevelIndex = 0;

    public bool isAtLastLevel => _currentLevelIndex == levels.Count - 1;
    public string currentLevel => GetSceneName(levels[_currentLevelIndex]);
    public string mainMenu => GetSceneName(_mainMenu);
    public string tutorialLevel => GetSceneName(_tutorialLevel);
    public string warpScene => GetSceneName(_warpScene);
    public string upgradeScene => GetSceneName(_upgradeScene);
    public string winLoseScreen => GetSceneName(_winLoseScreen);

    public void Reset(GameMode gameMode) {
        _currentLevelIndex = 0;
        levels = GetLevels(gameMode);
    }

    public void SetAsCurrentLevelIfActive() {
        for (int i = 0; i < levels.Count; i++) {
            if (levels[i].SceneName == SceneManager.GetActiveScene().name) {
                _currentLevelIndex = i;
                return;
            }
        }
    }

    public void IncrementLevelIndex() {
        _currentLevelIndex = Mathf.Min(_currentLevelIndex + 1, levels.Count - 1);
    }

    string GetSceneName(SceneReference sceneRef) {
        return sceneRef.SceneName;
    }

    List<SceneReference> GetLevels(GameMode gameMode) {
        switch (gameMode) {
            case GameMode.Campaign:
                return _campaignLevels;
            case GameMode.Arcade:
                return _arcadeLevels;
            case GameMode.Demo:
                return _demoLevels;
            default:
                return _demoLevels;
        }
    }
}
