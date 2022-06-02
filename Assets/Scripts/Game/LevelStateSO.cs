
using System.Collections.Generic;
using UnityEngine;

using Core;

[CreateAssetMenu(fileName = "LevelState", menuName = "ScriptableObjects/LevelState", order = 0)]
public class LevelStateSO : ScriptableObject {
    [SerializeField] List<SceneReference> _levels = new List<SceneReference>();
    [SerializeField] SceneReference _mainMenu;
    [SerializeField] SceneReference _tutorialLevel;
    [SerializeField] SceneReference _warpScene;
    [SerializeField] SceneReference _upgradeScene;

    int _currentLevelIndex = 0;

    public string currentLevel => GetSceneName(_levels[_currentLevelIndex]);
    public string mainMenu => GetSceneName(_mainMenu);
    public string tutorialLevel => GetSceneName(_tutorialLevel);
    public string warpScene => GetSceneName(_warpScene);
    public string upgradeScene => GetSceneName(_upgradeScene);

    public void Reset() {
        _currentLevelIndex = 0;
    }

    public void IncrementLevelIndex() {
        _currentLevelIndex = Mathf.Min(_currentLevelIndex + 1, _levels.Count - 1);
    }

    string GetSceneName(SceneReference sceneRef) {
        return sceneRef.SceneName;
    }
}
