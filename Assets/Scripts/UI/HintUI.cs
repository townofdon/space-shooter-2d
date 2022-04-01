using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Event;
using Core;
using Dialogue;

public class HintUI : MonoBehaviour {
    [SerializeField] EventChannelSO eventChannel;
    [SerializeField] float timeShowHint = 3f;
    [SerializeField] float timeDelayShowNext = 0.4f;

    [SerializeField] GameObject canvas;
    [SerializeField] GameObject panel0;
    [SerializeField] GameObject panel1;
    [SerializeField] GameObject icon0;
    [SerializeField] GameObject icon1;
    [SerializeField] GameObject textButtonGO0;
    [SerializeField] GameObject textButtonGO1;
    [SerializeField] TextMeshProUGUI textButton0;
    [SerializeField] TextMeshProUGUI textButton1;
    [SerializeField] TextMeshProUGUI textHint0;
    [SerializeField] TextMeshProUGUI textHint1;

    // state
    bool isShowingPanel0 = false;
    bool isShowingPanel1 = false;
    Queue<HintSO> hints = new Queue<HintSO>();
    HintSO currentHint;
    string controlScheme = "Keyboard&Mouse";
    int currentPanelIndex = -1;
    void OnEnable() {
        eventChannel.OnShowHint.Subscribe(OnShowHint);
    }

    void OnDisable() {
        eventChannel.OnShowHint.Unsubscribe(OnShowHint);
    }

    void Start() {
        AppIntegrity.AssertPresent(eventChannel);
        AppIntegrity.AssertPresent(canvas);
        AppIntegrity.AssertPresent(panel0);
        AppIntegrity.AssertPresent(panel1);
        AppIntegrity.AssertPresent(icon0);
        AppIntegrity.AssertPresent(icon1);
        AppIntegrity.AssertPresent(textButtonGO0);
        AppIntegrity.AssertPresent(textButtonGO1);
        AppIntegrity.AssertPresent(textButton0);
        AppIntegrity.AssertPresent(textButton1);
        AppIntegrity.AssertPresent(textHint0);
        AppIntegrity.AssertPresent(textHint1);

        canvas.SetActive(true);
        panel0.SetActive(false);
        panel1.SetActive(false);
        hints.Clear();

        StartCoroutine(IShowHints());
    }

    void OnShowHint(HintSO hint, string currentControlScheme = "Keyboard&Mouse") {
        controlScheme = currentControlScheme;
        hints.Enqueue(hint);
    }

    IEnumerator IShowHints() {
        while (true) {
            if (hints.Count > 0) {
                currentHint = hints.Dequeue();
                currentPanelIndex = -1;
                do {
                    if (currentHint.showButton) {
                        if (controlScheme == "Gamepad") {
                            currentPanelIndex = TryShowPanel(currentHint.gamepadButton, currentHint.hintText);
                        } else {
                            currentPanelIndex = TryShowPanel(currentHint.keyboardButton, currentHint.hintText);
                        }
                    } else {
                        currentPanelIndex = TryShowPanel(currentHint.hintText);
                    }
                    yield return null;
                } while (currentPanelIndex < 0);
                yield return IHidePanel(currentPanelIndex);

                // note - I couldn't figure out how to show two panels at the same time. This may acktchually be a feature, not a bug ;)
                // StartCoroutine(IHidePanel(currentPanelIndex));
            }
            yield return null;
        }
    }

    int TryShowPanel(string hintText) {
        return TryShowPanel("", hintText);
    }

    int TryShowPanel(string buttonText, string hintText) {
        if (!isShowingPanel0) {
            isShowingPanel0 = true;
            if (buttonText == "") {
                icon0.SetActive(true);
                textButtonGO0.SetActive(false);
            } else {
                icon0.SetActive(false);
                textButtonGO0.SetActive(true);
            }
            panel0.SetActive(true);
            textButton0.text = buttonText;
            textHint0.text = hintText;
            return 0;
        }
        if (!isShowingPanel1) {
            isShowingPanel1 = true;
            if (buttonText == "") {
                icon1.SetActive(true);
                textButtonGO1.SetActive(false);
            } else {
                icon1.SetActive(false);
                textButtonGO1.SetActive(true);
            }
            textButton1.text = buttonText;
            textHint1.text = hintText;
            return 1;
        }
        return -1;
    }

    IEnumerator IHidePanel(int index) {
        yield return new WaitForSeconds(timeShowHint);

        if (index == 0) {
            panel0.SetActive(false);
        } else if (index == 1) {
            panel1.SetActive(false);
        }

        yield return new WaitForSeconds(timeDelayShowNext);

        if (index == 0) {
            isShowingPanel0 = false;
        } else if (index == 1) {
            isShowingPanel1 = false;
        }
    }
}
