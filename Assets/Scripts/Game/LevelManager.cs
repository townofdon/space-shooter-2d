
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game {

    public class LevelManager : MonoBehaviour {
        [SerializeField] bool debug;
        [SerializeField] LevelStateSO levelState;

        public string currentScene => SceneManager.GetActiveScene().name;

        public bool IsOnTutorialLevel() {
            return SceneManager.GetActiveScene().name == levelState.tutorialLevel;
        }

        void Start() {
            levelState.Reset(GameManager.current.gameMode);
        }

        public void GotoMainMenu() {
            LoadScene(levelState.mainMenu);
        }

        public void GotoTutorialLevel() {
            LoadScene(levelState.tutorialLevel);
        }

        public void GotoLevelOne() {
            levelState.Reset(GameManager.current.gameMode);
            LoadScene(levelState.currentLevel);
        }

        public void GotoNextLevel(bool loopToLevelOne = false) {
            if (levelState.isAtLastLevel && loopToLevelOne) {
                GotoLevelOne();
                return;
            }
            if (IsOnTutorialLevel()) {
                GotoLevelOne();
                return;
            }
            if (levelState.isAtLastLevel) {
                GotoWinLoseScreen();
                return;
            }
            if (!IsOnTutorialLevel()) {
                levelState.IncrementLevelIndex();
            }
            LoadScene(levelState.currentLevel);
        }

        public void GotoWarpScene() {
            LoadScene(levelState.warpScene);
        }

        public void GotoUpgradeScene() {
            LoadScene(levelState.upgradeScene);
        }

        public void GotoWinLoseScreen() {
            LoadScene(levelState.winLoseScreen);
        }

        void LoadScene(string sceneName) {
            if (debug) Debug.Log($"Loading scene \"{sceneName}\"");
            SceneManager.LoadScene(sceneName);
        }
    }
}
