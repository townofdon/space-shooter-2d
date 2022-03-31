using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

using Event;
using Core;

namespace UI {

    public class PauseUI : MonoBehaviour {
        [SerializeField] EventChannelSO eventChannel;
        [SerializeField] List<Button> buttons;
        [SerializeField] Image backgroundImage;
        [SerializeField] GameObject canvas;
        [SerializeField] GameObject content;
        [SerializeField] UnityEngine.EventSystems.EventSystem eventSystem;

        bool isShowing;
        Coroutine ieShow;
        Coroutine ieHide;

        public void Continue() {
            eventChannel.OnUnpause.Invoke();
        }

        public void GotoMainMenu() {
            StopAllCoroutines();
            StartCoroutine(IGotoMainMenu());
        }

        void OnEnable() {
            eventChannel.OnPause.Subscribe(OnShowPauseMenu);
            eventChannel.OnUnpause.Subscribe(OnHidePauseMenu);
        }

        void OnDisable() {
            eventChannel.OnPause.Unsubscribe(OnShowPauseMenu);
            eventChannel.OnUnpause.Unsubscribe(OnHidePauseMenu);
        }

        void Awake() {
            AppIntegrity.AssertPresent(backgroundImage);
            AppIntegrity.AssertPresent(canvas);
            AppIntegrity.AssertPresent(content);
            AppIntegrity.AssertPresent(eventSystem);

            backgroundImage.enabled = false;
            eventSystem.enabled = false;
            canvas.SetActive(false);
            content.SetActive(false);
        }

        void Update() {

        }

        void OnNavigate(InputValue value) {
            Debug.Log("Navigate >> " + value.Get<Vector2>());
        }

        void OnSubmit(InputValue value) {
            if (value.isPressed) Debug.Log("Submit >> " + value);
        }

        void OnCancel(InputValue value) {
            if (value.isPressed) Debug.Log("Cancel >> " + value);
        }

        void OnShowPauseMenu() {
            StopAllCoroutines();
            if (ieShow == null) ieShow = StartCoroutine(IShowPauseMenu());
        }

        void OnHidePauseMenu() {
            StopAllCoroutines();
            if (ieHide == null) ieHide = StartCoroutine(IHidePauseMenu());
        }

        IEnumerator IShowPauseMenu() {
            // could add a delay here

            isShowing = true;
            canvas.SetActive(true);
            eventSystem.enabled = true;
            backgroundImage.enabled = true;

            // TODO: animate in menu
            yield return null;

            content.SetActive(true);
            if (buttons.Count > 0) buttons[0].Select();

            ieShow = null;
        }

        IEnumerator IHidePauseMenu() {
            eventSystem.enabled = false;
            content.SetActive(false);

            // TODO: animate in menu (use WaitForSecondsRealTime)
            yield return null;

            backgroundImage.enabled = false;
            canvas.SetActive(false);
            isShowing = false;
            ieHide = null;
        }

        IEnumerator IGotoMainMenu() {
            yield return IHidePauseMenu();
            eventChannel.OnGotoMainMenu.Invoke();
        }
    }
}

