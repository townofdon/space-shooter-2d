using System.Collections;
using UnityEngine;
using XInputDotNetPure;

namespace Core
{    

    public static class GameFeel
    {
        // TODO: ADD VOID METHODS FOR ALL METHODS BELOW;
        //       CHANGE IEnumerator METHODS TO INameOfMethod
        //       CANCEL PREVIOUS COROUTINE IF PRESENT - KEEP TRACK OF ALL COROUTINE STATES

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

        public static IEnumerator ShakeGamepad(float duration = 0.1f, float leftMotor = 0.5f, float rightMotor = 0.5f) {
            GamePad.SetVibration(0, leftMotor, rightMotor);
            yield return new WaitForSecondsRealtime(duration);
            GamePad.SetVibration(0, 0, 0);
        }

        public static void ResetGamepadShake() {
            GamePad.SetVibration(0, 0, 0);
        }
    }
}
