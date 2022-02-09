using System.Collections;
using UnityEngine;

namespace Core
{    

    public static class GameFeel
    {
        public static IEnumerator ShakeScreen(Camera camera, float duration = 0.3f, float magnitude = 0.5f) {
            // set initial position
            Vector3 initialPosition = camera.transform.position;
            float t = 0f;
            while (t < duration) {
                t += Time.deltaTime;
                camera.transform.position = initialPosition + (Vector3)UnityEngine.Random.insideUnitCircle * magnitude;
                yield return new WaitForEndOfFrame();
            }
            camera.transform.position = initialPosition;
        }

        public static IEnumerator PauseTime(float duration = 0.1f, float timeScale = 0f) {
            Time.timeScale = timeScale;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
        }
    }
}
