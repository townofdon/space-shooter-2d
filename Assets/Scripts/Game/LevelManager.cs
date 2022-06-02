
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game {

    public class LevelManager : MonoBehaviour {
        [SerializeField] bool debug;
        [SerializeField] LevelStateSO levelState;

        void Start() {
            levelState.Reset();
        }

        public void GotoMainMenu() {
            LoadScene(levelState.mainMenu);
        }

        public void GotoTutorialLevel() {
            LoadScene(levelState.tutorialLevel);
        }

        public void GotoLevelOne() {
            levelState.Reset();
            LoadScene(levelState.currentLevel);
        }

        public void GotoNextLevel() {
            if (SceneManager.GetActiveScene().name != levelState.tutorialLevel) {
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

        void LoadScene(string sceneName) {
            if (debug) Debug.Log($"Loading scene \"{sceneName}\"");
            SceneManager.LoadScene(sceneName);
        }
    }
}
