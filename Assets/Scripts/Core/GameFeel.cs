using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using XInputDotNetPure;

using CameraFX;

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

        // public static IEnumerator ShakeScreen(Camera camera, float duration = 0.3f, float magnitude = 0.5f) {
        //     // set initial position
        //     Vector3 initialPosition = new Vector3(0f, 0f, -10f);
        //     float t = 0f;
        //     while (t < duration) {
        //         t += Time.deltaTime;
        //         camera.transform.position = initialPosition + (Vector3)UnityEngine.Random.insideUnitCircle * magnitude;
        //         yield return new WaitForEndOfFrame();
        //     }
        //     // TODO: in next game, def want to move this method inside of a monobehaviour so that IShakeScreen can be cancelled
        //     // Another solution would be figuring out a way to modify the camera offset, not the position itself
        //     // camera.transform.position = initialPosition;
        //     // Yes, this is hard-coded for now and this works perfectly for THIS GAME. Future games esp. with moving camera would need
        //     // a different implementation.
        //     camera.transform.position = new Vector3(0f, 0f, -10f);
        // }

        static ScreenShake screenShake;

        public static void ShakeScreen(float duration = 0.3f, float magnitude = 0.5f) {
            if (screenShake == null) screenShake = Utils.GetCamera().GetComponent<ScreenShake>();
            if (screenShake == null) {
                Debug.LogError($"Camera missing \"ScreenShake\" component for scene \"{SceneManager.GetActiveScene().name}\"");
                return;
            }
            screenShake.ShakeScreen(duration, magnitude);
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
