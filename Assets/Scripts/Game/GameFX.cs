using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Core;
using UI;

namespace Game
{    

    public class GameFX : MonoBehaviour
    {
        [Header("Warp")]
        [SerializeField] bool debug = false;
        [SerializeField] ParticleSystem warpFX;
        [SerializeField][Range(0f, 10f)] float warpStartSpeed = 0.1f;
        [SerializeField][Range(0f, 10f)] float warpEndSpeed = 5f;
        [SerializeField][Range(0f, 10f)] float warpDuration = 3f;
        [SerializeField][Range(0f, 10f)] float timeWaitAfterWarp = 0.5f;
        [SerializeField] AnimationCurve warpSpeedMod = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        [Space]
        [Header("Warp - other objects")]
        [SerializeField][Range(0f, 20f)] float otherSpeed = 20f;
        [SerializeField][Range(0f, 20f)] float otherAccelTime = 1f;

        [Space]
        [Header("Warp - BG scroll speed")]
        [SerializeField] List<BackgroundScroller> starBackgrounds = new List<BackgroundScroller>();
        [SerializeField] float bgAccel = 1f;

        Timer warping = new Timer(TimerDirection.Increment);
        ParticleSystem.MainModule warpFXModule;
        Coroutine iWarp;

        public Coroutine Warp() {
            if (iWarp != null) return iWarp;
            iWarp = StartCoroutine(IWarp());
            AccelerateObjects();
            return iWarp;
        }

        void Start() {
            warpFX.Stop();
            warpFXModule = warpFX.main;
        }

        IEnumerator IWarp() {
            warping.SetDuration(warpDuration);
            warping.Start();
            warpFX.gameObject.SetActive(true);
            warpFXModule.simulationSpeed = warpStartSpeed;
            warpFX.Play();
            while (warping.active) {
                warping.Tick();
                warpFXModule.simulationSpeed = Mathf.Lerp(warpStartSpeed, warpEndSpeed, warpSpeedMod.Evaluate(warping.value));
                foreach (var bg in starBackgrounds) bg.SetScrollDelta(bg.GetScrollDelta() + bg.GetScrollDelta() * Time.deltaTime * bgAccel);
                yield return null;
            }
            yield return new WaitForSeconds(timeWaitAfterWarp);
            iWarp = null;
        }

        void AccelerateObjects() {
            foreach (var marker in Object.FindObjectsOfType<OffscreenMarker>()) Destroy(marker.gameObject);
            foreach (var obj in Object.FindObjectsOfType<AccelOnWarp>()) obj.BeginWarp(otherSpeed, otherAccelTime);
        }

        void OnGUI() {
            if (!debug) return;
            if (GUILayout.Button("Warp")) {
                Warp();
            }
        }
    }
}
