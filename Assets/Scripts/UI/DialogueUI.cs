using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Event;
using Dialogue;
using Core;
using Audio;
using Game;

namespace UI {

    public class DialogueUI : MonoBehaviour {
        [SerializeField] bool debug;
        [SerializeField] EventChannelSO eventChannel;
        [SerializeField] GameObject canvas;
        [SerializeField] Image avatarImage;
        [SerializeField] TextMeshProUGUI textAnnouncer;
        [SerializeField] TextMeshProUGUI textStatement;
        [SerializeField] TextMeshProUGUI textPressAnyKey;
        [SerializeField] float inputDelay = 0.25f;

        // state
        bool isShowing = false;
        bool dismiss = false;
        string currentStatement = "I have nothing to say to you, Lord Hyperion!!";
        int numLettersShowing = 0;
        DialogueItemSO currentItem;
        Coroutine ieShowing;
        Coroutine ieHiding;
        Timer justAppeared = new Timer();

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
            justAppeared.SetDuration(inputDelay);
        }

        void Update() {
            justAppeared.Tick();
        }

        void OnShowDialogue(DialogueItemSO dialogueItem) {
            if (GameManager.current.gameMode == GameMode.Arcade) {
                eventChannel.OnDismissDialogue.Invoke();
                return;
            }
            justAppeared.Start();
            currentItem = dialogueItem;
            ieShowing = StartCoroutine(IShowDialogue());
        }

        void OnAnyKeyPress() {
            if (justAppeared.active) return;
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
            HideDialogue();
        }

        IEnumerator IShowStatements() {
            while (currentItem.HasNextStatement()) {
                dismiss = false;
                textStatement.text = "";
                textStatement.maxVisibleCharacters = 0;
                currentStatement = currentItem.GetNextStatement();
                textStatement.text = currentStatement;
                textPressAnyKey.enabled = false;
                yield return null;

                while (!dismiss && textStatement.maxVisibleCharacters < currentStatement.Length) {
                    textStatement.maxVisibleCharacters++;
                    AudioManager.current.PlaySound("DialogueChip");
                    yield return new WaitForSeconds(0.02f);
                }

                dismiss = false;
                textStatement.maxVisibleCharacters = Mathf.Max(currentStatement.Length, 9999);
                textPressAnyKey.enabled = true;
                yield return null;

                while (!dismiss) yield return null;
            }

            yield return null;
        }

        void HideDialogue() {
            canvas.SetActive(false);
            eventChannel.OnDismissDialogue.Invoke();
        }

        private void OnGUI() {
            if (!debug) return;
            GUILayout.TextField(currentStatement);
            GUILayout.TextField(isShowing ? "showing" : "hidden");
        }
    }
}

