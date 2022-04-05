using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Event;
using Core;
using Audio;

namespace UI {

    public class PauseUI : MonoBehaviour {
        [SerializeField] EventChannelSO eventChannel;
        [SerializeField] List<Button> buttons;
        [SerializeField] Image backgroundImage;
        [SerializeField] GameObject canvas;
        [SerializeField] GameObject content;

        bool isShowing;
        bool everFocused;
        Coroutine ieShow;
        Coroutine ieHide;

        public void Continue() {
            eventChannel.OnUnpause.Invoke();
        }

        public void GotoMainMenu() {
            if (ieShow != null) StopCoroutine(ieShow);
            if (ieHide != null) StopCoroutine(ieHide);
            eventChannel.OnGotoMainMenu.Invoke();
        }

        public void OnButtonFocusSound() {
            if (everFocused) AudioManager.current.PlaySound("MenuFocus");
            everFocused = true;
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

            backgroundImage.enabled = false;
            canvas.SetActive(false);
            content.SetActive(false);
        }

        void OnShowPauseMenu() {
            if (ieShow != null) StopCoroutine(ieShow);
            if (ieHide != null) StopCoroutine(ieHide);
            if (ieShow == null) ieShow = StartCoroutine(IShowPauseMenu());
        }

        void OnHidePauseMenu() {
            if (ieShow != null) StopCoroutine(ieShow);
            if (ieHide != null) StopCoroutine(ieHide);
            if (ieHide == null) ieHide = StartCoroutine(IHidePauseMenu());
        }

        IEnumerator IShowPauseMenu() {
            // could add a delay here

            isShowing = true;
            canvas.SetActive(true);
            backgroundImage.enabled = true;

            // TODO: animate in menu
            yield return null;

            content.SetActive(true);
            if (buttons.Count > 0) buttons[0].Select();

            ieShow = null;
        }

        IEnumerator IHidePauseMenu() {
            content.SetActive(false);

            // TODO: animate in menu (use WaitForSecondsRealTime)
            yield return null;

            backgroundImage.enabled = false;
            canvas.SetActive(false);
            isShowing = false;
            ieHide = null;
        }
    }
}

