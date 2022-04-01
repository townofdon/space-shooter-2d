using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Event;
using Dialogue;
using Core;
using Audio;

namespace UI {

}

public class DialogueUI : MonoBehaviour {
    [SerializeField] EventChannelSO eventChannel;
    [SerializeField] GameObject canvas;
    [SerializeField] Image avatarImage;
    [SerializeField] TextMeshProUGUI textAnnouncer;
    [SerializeField] TextMeshProUGUI textStatement;
    [SerializeField] TextMeshProUGUI textPressAnyKey;

    // state
    bool isShowing = false;
    bool dismiss = false;
    string currentStatement = "I have nothing to say to you, Lord Hyperion!!";
    int numLettersShowing = 0;
    DialogueItemSO currentItem;
    Coroutine ieShowing;
    Coroutine ieHiding;

    private void OnEnable() {
        eventChannel.OnShowDialogue.Subscribe(OnShowDialogue);
        eventChannel.OnAnyKeyPress.Subscribe(OnAnyKeyPress);
    }

    private void OnDisable() {
        eventChannel.OnShowDialogue.Unsubscribe(OnShowDialogue);
        eventChannel.OnAnyKeyPress.Unsubscribe(OnAnyKeyPress);
    }

    void Start() {
        AppIntegrity.AssertPresent(eventChannel);
        AppIntegrity.AssertPresent(canvas);
        AppIntegrity.AssertPresent(avatarImage);
        AppIntegrity.AssertPresent(textAnnouncer);
        AppIntegrity.AssertPresent(textStatement);
        AppIntegrity.AssertPresent(textPressAnyKey);

        canvas.SetActive(false);
    }


    void Update() {

    }

    void OnShowDialogue(DialogueItemSO dialogueItem) {
        currentItem = dialogueItem;
        ieShowing = StartCoroutine(IShowDialogue());
    }

    void OnAnyKeyPress() {
        dismiss = true;
    }

    IEnumerator IShowDialogue() {
        canvas.SetActive(true);
        yield return null;

        currentItem.Init();
        textAnnouncer.text = currentItem.entityName;
        avatarImage.sprite = currentItem.entityImage;
        textStatement.text = "";
        textStatement.maxVisibleCharacters = 0;
        textPressAnyKey.enabled = false;

        yield return IShowStatements();
        yield return IHideDialogue();
    }

    IEnumerator IShowStatements() {
        while (currentItem.HasNextStatement()) {
            dismiss = false;
            textStatement.text = "";
            textStatement.maxVisibleCharacters = 0;
            currentStatement = currentItem.GetNextStatement();
            textStatement.text = currentStatement;
            textPressAnyKey.enabled = false;

            while (!dismiss && textStatement.maxVisibleCharacters < currentStatement.Length) {
                textStatement.maxVisibleCharacters++;
                AudioManager.current.PlaySound("DialogueChip");
                yield return new WaitForSeconds(0.02f);
            }

            dismiss = false;
            textStatement.maxVisibleCharacters = Mathf.Max(currentStatement.Length, 9999);
            textPressAnyKey.enabled = true;

            while (!dismiss) yield return null;
        }

        yield return null;
    }

    IEnumerator IHideDialogue() {
        canvas.SetActive(false);
        yield return null;
        eventChannel.OnDismissDialogue.Invoke();
    }
}
