using System.Collections;
using UnityEngine;
using XInputDotNetPure;

namespace Core
{    

    public static class GameFeel
    {
        // NOTE FOR FUTURE DON - BETTER SOLUTION WOULD BE TO PLACE ShakeScreen METHOD DIRECTLY ON A SCRIPT ATTACHED TO THE CAMERA.
        // THAT WAY, THE MONOBEHAVIOUR IS ALWAYS IN THE SCENE, AND IT CAN CANCEL COROUTINES. METHODS CAN STILL BE MADE TO BE CALLED
        // IMPERATIVELY.

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
