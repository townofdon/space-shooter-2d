
using UnityEngine;

namespace Game {

    public static class GameUtils {
        public static float GetPointsMod() {
            switch (GameManager.current.difficulty) {
                case GameDifficulty.Insane:
                    return 5.0f;
                case GameDifficulty.Hard:
                    return 2f;
                case GameDifficulty.Medium:
                    return 1.25f;
                case GameDifficulty.Easy:
                default:
                    return 1.0f;
            }
        }
    }
}

