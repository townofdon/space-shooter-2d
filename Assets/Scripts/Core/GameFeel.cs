using System.Collections;
using UnityEngine;

namespace Core
{    

    public static class GameFeel
    {
        public static void ShakeScreen() {}

        public static IEnumerator PauseTime(float duration = 0.1f, float timeScale = 0f) {
            Time.timeScale = timeScale;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
        }
    }
}
