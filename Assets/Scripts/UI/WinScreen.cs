using System.Collections;
using UnityEngine;

using TMPro;
using Audio;
using Game;
using Player;
using Event;
using Core;
using Dialogue;
using UnityEngine.UI;

namespace UI {

    public class WinScreen : MonoBehaviour {
        [SerializeField] bool debug = false;
        [SerializeField] EventChannelSO eventChannel;
        [SerializeField] GameStateSO gameState;
        [SerializeField] PlayerStateSO playerState;
        [SerializeField] GameObject canvasStats;
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

        [SerializeField] TextMeshProUGUI titleWin;
        [SerializeField] TextMeshProUGUI titleLose;

        [Space]

        [SerializeField] float timeDelayStats = 0.7f;
        [SerializeField] float timeBetweenStats = 0.7f;
        [SerializeField] float timeDelayPressAnyKey = 0.7f;

        [Space]
        [Header("Leaderboard")]
        [SerializeField] UIInputHandler input;
        [SerializeField] GameObject canvasHighScoreEntry;
        [SerializeField] GameObject canvasNameConfirm;
        [SerializeField] GameObject canvasLeaderboard;
        [SerializeField] HighScoreManager highScoreManager;
        [SerializeField] HighScoreEntry highScoreEntry;
        [SerializeField] HighScoreDisplay highScoreDisplay;
        [SerializeField] Button confirmNameButton;
        [SerializeField] TextMeshProUGUI textConfirmName;
        [SerializeField] GameObject highScoreFX;
        [SerializeField] GameObject highScoreToast;
        [SerializeField] Vector2 highScoreToastOffset;
        [SerializeField] Gradient highScoreGradient;

        bool dismiss = false;
        bool waitingForDialogue = false;
        int num = 0;
        float fNum = 0;

        int highScoreIndex = -1;
        string playerName;
        bool isEnteringName = false;

        int totalPoints => debug ? 123456 : gameState.totalPoints;
        int numEnemiesKilled => debug ? 123 : gameState.numEnemiesKilled;
        int numPlayerDeaths => debug ? 42 : playerState.numDeaths;
        float timeElapsed => debug ? 333 : GameManager.current.timeElapsed;

        private void OnEnable() {
            eventChannel.OnAnyKeyPress.Subscribe(OnAnyKeyPress);
            eventChannel.OnDismissDialogue.Subscribe(OnDismissDialogue);
            eventChannel.OnSubmitName.Subscribe(OnSubmitName);
        }

        private void OnDisable() {
            eventChannel.OnAnyKeyPress.Unsubscribe(OnAnyKeyPress);
            eventChannel.OnDismissDialogue.Unsubscribe(OnDismissDialogue);
            eventChannel.OnSubmitName.Unsubscribe(OnSubmitName);
        }

        void Start() {
            canvasStats.SetActive(false);
            rowPoints.SetActive(false);
            rowEnemies.SetActive(false);
            rowDeaths.SetActive(false);
            rowTime.SetActive(false);
            textPressAnyKey.SetActive(false);

            if (gameState.lives > 0) {
                AudioManager.current.CueTrack("starlord-main-theme");
                ShowTitleWin();
                StartCoroutine(IDialogue());
            } else {
                AudioManager.current.StopTrack();
                ShowTitleLose();
                StartCoroutine(IStats());
            }
        }

        void OnSubmitName(string name) {
            if (!canvasHighScoreEntry.activeSelf) return;
            playerName = name;
            textConfirmName.text = name;
            canvasNameConfirm.SetActive(true);
            confirmNameButton.Select();
            AudioManager.current.PlaySound("MenuSelect");
        }

        public void OnConfirmName() {
            eventChannel.OnSubmitHighScore.Invoke(playerName, gameState.totalPoints);
            canvasHighScoreEntry.SetActive(false);
            canvasNameConfirm.SetActive(false);
            AudioManager.current.PlaySound("MenuConfirm");
            isEnteringName = false;
        }

        public void OnSubmitCancel() {
            canvasHighScoreEntry.SetActive(true);
            canvasNameConfirm.SetActive(false);
            highScoreEntry.CancelSubmit();
            AudioManager.current.PlaySound("MenuSelect");
        }

        void ShowTitleWin() {
            titleWin.enabled = true;
            titleWin.gameObject.SetActive(true);
            titleLose.enabled = false;
            titleLose.gameObject.SetActive(false);
        }

        void ShowTitleLose() {
            titleWin.enabled = false;
            titleWin.gameObject.SetActive(false);
            titleLose.enabled = true;
            titleLose.gameObject.SetActive(true);
        }

        void OnAnyKeyPress() {
            dismiss = true;
        }

        void OnDismissDialogue() {
            waitingForDialogue = false;
        }

        IEnumerator IDialogue() {
            waitingForDialogue = true;
            eventChannel.OnShowDialogue.Invoke(dialogueItem);
            while (waitingForDialogue) yield return null;
            yield return IStats();
        }

        IEnumerator IStats() {
            canvasStats.SetActive(true);
            yield return new WaitForSeconds(timeDelayStats);

            rowPoints.SetActive(true);

            yield return IShowStat(fieldPoints, totalPoints);

            if (highScoreManager.IsScoreTopTen(gameState.totalPoints)) {
                AudioManager.current.PlaySound("HighScore");
                StartCoroutine(ISpawnHighScoreToasts());
                StartCoroutine(IColorPointsField());
            }

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

            // eventChannel.OnShowHint.Invoke(upgradeHint, "Keyboard&Mouse");
            AudioManager.current.PlaySound("show-hint");

            AudioManager.current.PlaySound("DialogueChip");
            textPressAnyKey.SetActive(true);
            dismiss = false;
            while (!dismiss) yield return null;

            canvasStats.SetActive(false);
            yield return IHighScores();
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

        IEnumerator IHighScores() {
            if (highScoreManager.IsScoreTopTen(gameState.totalPoints)) {
                highScoreIndex = highScoreManager.GetHighScoreIndex(gameState.totalPoints);
                highScoreDisplay.SetSelectedHighScore(highScoreIndex);
                canvasHighScoreEntry.SetActive(true);
                input.ResetInputs();
                highScoreEntry.EnableInput();
                while (canvasHighScoreEntry.activeSelf || canvasNameConfirm.activeSelf || isEnteringName) yield return null;
                highScoreIndex = highScoreManager.GetHighScoreIndexByName(playerName);
                highScoreDisplay.SetSelectedHighScore(highScoreIndex);
            } else {
                AudioManager.current.PlaySound("MenuSelect");
            }

            canvasHighScoreEntry.SetActive(false);
            canvasNameConfirm.SetActive(false);
            canvasLeaderboard.SetActive(true);

            yield return new WaitForSeconds(timeDelayPressAnyKey);
            dismiss = false;
            while (!dismiss) yield return null;

            GameManager.current.GotoMainMenu();
        }

        IEnumerator ISpawnHighScoreToasts() {
            yield return null;
            Vector3 pointsPosition = Utils.GetCamera().ScreenToWorldPoint(fieldPoints.rectTransform.position);
            if (highScoreFX != null) Instantiate(highScoreFX, Vector3.zero, Quaternion.identity);
            for (int i = 0; i < 10; i++) {
                if (highScoreToast != null) Instantiate(highScoreToast, pointsPosition + (Vector3)highScoreToastOffset, Quaternion.identity);
                yield return new WaitForSeconds(.2f);
            }
        }

        IEnumerator IColorPointsField() {
            float t = 0f;
            while (true) {
                fieldPoints.color = highScoreGradient.Evaluate(t);
                t += Time.deltaTime;
                if (t > 1) t = 0f;
                yield return null;
            }
        }

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

