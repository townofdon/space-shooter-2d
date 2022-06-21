using System.Collections;
using UnityEngine;

using Event;
using Core;

namespace Enemies {

    public class EnemyShooterController : MonoBehaviour {
        [SerializeField] bool debug = false;
        [SerializeField][Range(0f, 10f)] float delayStart = 0f;
        [SerializeField][Range(0f, 10f)] float triggerHoldTime = 1f;
        [SerializeField][Range(0f, 10f)] float triggerReleaseTime = 1f;
        [SerializeField][Range(0f, 10f)] float triggerTimeVariance = 0f;

        VoidEventHandler _OnFire = new VoidEventHandler();
        public VoidEventHandler OnFire => _OnFire;

        VoidEventHandler _OnCeaseFire = new VoidEventHandler();
        public VoidEventHandler OnCeaseFire => _OnCeaseFire;

        // state
        Timer triggerHeld = new Timer();
        Timer triggerReleased = new Timer();
        Coroutine firing;

        public void SetTriggerHoldTime(float value) {
            triggerHoldTime = value;
        }

        public void SetTriggerReleaseTime(float value) {
            triggerReleaseTime = value;
        }

        public void SetTriggerTimeVariance(float value) {
            triggerTimeVariance = value;
        }

        void OnEnable() {
            if (firing != null) StopCoroutine(firing);
            firing = StartCoroutine(PressAndReleaseTrigger());
        }

        IEnumerator PressAndReleaseTrigger() {
            // simulate human-like response time
            yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(0.2f, 0.6f));
            if (delayStart > 0f) {
                yield return new WaitForSeconds(Utils.RandomVariance2(delayStart, 1f, delayStart * 0.5f));
            }
            while (true) {
                yield return IFire();
                yield return ICeaseFire();
            }
        }

        IEnumerator IFire() {
            _OnFire.Invoke();
            triggerHeld.SetDuration(Mathf.Max(triggerHoldTime + GetTriggerVariance(), 0.1f));
            yield return triggerHeld.StartAndWaitUntilFinished(true);
        }

        IEnumerator ICeaseFire() {
            _OnCeaseFire.Invoke();
            triggerReleased.SetDuration(Mathf.Max(triggerReleaseTime + GetTriggerVariance(), triggerReleaseTime * 0.5f));
            yield return triggerReleased.StartAndWaitUntilFinished(true);

        }

        float GetTriggerVariance() {
            return UnityEngine.Random.Range(-triggerTimeVariance * 0.5f, triggerTimeVariance * 0.5f);
        }

        void OnGUI() {
            if (!debug) return;
            if (GUILayout.Button("Fire")) {
                _OnFire.Invoke();
            }
            if (GUILayout.Button("CeaseFire")) {
                _OnCeaseFire.Invoke();
            }
        }
    }
}
