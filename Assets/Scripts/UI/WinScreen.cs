using System.Collections;
using UnityEngine;

using TMPro;
using Audio;
using Game;
using Player;
using Event;
using Core;
using Dialogue;

namespace UI {

    public class WinScreen : MonoBehaviour {
        [SerializeField] bool debug = false;
        [SerializeField] EventChannelSO eventChannel;
        [SerializeField] GameStateSO gameState;
        [SerializeField] PlayerStateSO playerState;
        [SerializeField] GameObject canvas;
        [SerializeField] GameObject textPressAnyKey;
        [SerializeField] DialogueItemSO dialogueItem;
        [SerializeField] Dialogue.HintSO upgradeHint;

        [Space]

        [SerializeField] GameObject rowPoints;
        [SerializeField] GameObject rowEnemies;
        [SerializeField] GameObject rowDeaths;
        [SerializeField] GameObject rowTime;

        [Space]

        [SerializeField] TextMeshProUGUI fieldPoints;
        [SerializeField] TextMeshProUGUI fieldEnemies;
        [SerializeField] TextMeshProUGUI fieldDeaths;
        [SerializeField] TextMeshProUGUI fieldTime;

        [Space]

        [SerializeField] float timeDelayStats = 0.7f;
        [SerializeField] float timeBetweenStats = 0.7f;
        [SerializeField] float timeDelayPressAnyKey = 0.7f;

        bool dismiss = false;
        bool waitingForDialogue = false;
        int num = 0;
        float fNum = 0;

        int totalPoints => debug ? 123456 : gameState.totalPoints;
        int numEnemiesKilled => debug ? 123 : gameState.numEnemiesKilled;
        int numPlayerDeaths => debug ? 42 : playerState.numDeaths;
        float timeElapsed => debug ? 333 : GameManager.current.timeElapsed;

        private void OnEnable() {
            eventChannel.OnAnyKeyPress.Subscribe(OnAnyKeyPress);
            eventChannel.OnDismissDialogue.Subscribe(OnDismissDialogue);
        }

        private void OnDisable() {
            eventChannel.OnAnyKeyPress.Unsubscribe(OnAnyKeyPress);
            eventChannel.OnDismissDialogue.Unsubscribe(OnDismissDialogue);
        }

        void Start() {
            canvas.SetActive(false);
            rowPoints.SetActive(false);
            rowEnemies.SetActive(false);
            rowDeaths.SetActive(false);
            rowTime.SetActive(false);
            textPressAnyKey.SetActive(false);
            AudioManager.current.CueTrack("starlord-main-theme");

            StartCoroutine(IDialogue());
        }

        void OnAnyKeyPress() {
            dismiss = true;
        }

        void OnDismissDialogue() {
            waitingForDialogue = false;
        }

        IEnumerator IDialogue() {
            eventChannel.OnShowDialogue.Invoke(dialogueItem);
            waitingForDialogue = true;
            while (waitingForDialogue) yield return null;
            yield return IStats();
        }

        IEnumerator IStats() {
            canvas.SetActive(true);
            yield return new WaitForSeconds(timeDelayStats);

            rowPoints.SetActive(true);

            yield return IShowStat(fieldPoints, totalPoints);
            yield return new WaitForSeconds(timeBetweenStats);

            rowEnemies.SetActive(true);

            yield return IShowStat(fieldEnemies, numEnemiesKilled);
            yield return new WaitForSeconds(timeBetweenStats);

            rowDeaths.SetActive(true);

            yield return IShowStat(fieldDeaths, numPlayerDeaths);
            yield return new WaitForSeconds(timeBetweenStats);

            rowTime.SetActive(true);

            yield return IShowTimeStat(fieldTime, timeElapsed);

            yield return new WaitForSeconds(timeDelayPressAnyKey);

            eventChannel.OnShowHint.Invoke(upgradeHint, "Keyboard&Mouse");

            AudioManager.current.PlaySound("DialogueChip");
            textPressAnyKey.SetActive(true);
            dismiss = false;
            while (!dismiss) yield return null;

            GameManager.current.GotoLevelOne(true);
        }

        IEnumerator IShowStat(TextMeshProUGUI field, int value, string append = "") {
            dismiss = false;
            num = 0;
            AudioManager.current.PlaySound("DialogueChip");
            while (!dismiss && num < value) {
                AudioManager.current.PlaySound("DialogueChip");
                num = Mathf.Min(num + GetStep(value - num), value);
                field.text = num.ToString() + (append == "" ? "" : " ") + append;
                yield return new WaitForSeconds(0.02f);
            }
            field.text = value.ToString() + (append == "" ? "" : " ") + append;
            dismiss = false;
        }

        IEnumerator IShowTimeStat(TextMeshProUGUI field, float value) {
            dismiss = false;
            fNum = 0;
            AudioManager.current.PlaySound("DialogueChip");
            while (!dismiss && fNum < value) {
                AudioManager.current.PlaySound("DialogueChip");
                fNum = Mathf.Min(fNum + 10f, value);
                field.text = Utils.ToTimeString(fNum);
                yield return new WaitForSeconds(0.02f);
            }
            field.text = Utils.ToTimeString(value);
            dismiss = false;
        }

        // int GetFinalScore() {
        //     return (int)(gameState.totalPoints * GetPointsModifier());
        // }

        // float GetPointsModifier() {
        //     switch (gameState.difficulty) {
        //         case GameDifficulty.Insane:
        //             return 5.0f;
        //         case GameDifficulty.Hard:
        //             return 2f;
        //         case GameDifficulty.Medium:
        //             return 1.2f;
        //         case GameDifficulty.Easy:
        //         default:
        //             return 1.0f;
        //     }
        // }

        int GetStep(int diff) {
            if (diff >= 50000) return 10000;
            if (diff >= 5000) return 1000;
            if (diff >= 500) return 100;
            if (diff >= 50) return 10;
            if (diff >= 10) return 5;
            return 1;
        }
    }
}

