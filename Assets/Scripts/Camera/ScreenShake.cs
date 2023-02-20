using UnityEngine;
using System.Collections;

using Core;

namespace CameraFX {

    public class ScreenShake : MonoBehaviour {

        Camera mainCamera;
        CameraPosition cameraPosition;

        Coroutine iShakeScreen;

        public void ShakeScreen(float duration, float magnitude) {
            StartCoroutine(IShakeScreen(duration, magnitude));
        }

        private void Awake() {
            mainCamera = Utils.GetCamera();
            cameraPosition = mainCamera.GetComponent<CameraPosition>();
        }

        IEnumerator IShakeScreen(float duration = 0.3f, float magnitude = 0.5f) {
            float t = 0f;
            while (t < duration) {
                t += Time.deltaTime;
                cameraPosition.SetOffset((Vector3)(Vector2)UnityEngine.Random.insideUnitCircle * magnitude);
                yield return new WaitForEndOfFrame();
                while (Time.timeScale <= 0) yield return null;
            }
            cameraPosition.SetOffset(Vector3.zero);
        }
    }
}
