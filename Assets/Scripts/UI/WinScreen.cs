using System.Collections;
using UnityEngine;

using TMPro;
using Audio;
using Game;
using Player;
using Event;
using Core;

namespace UI {

    public class WinScreen : MonoBehaviour {
        [SerializeField] bool debug = false;
        [SerializeField] EventChannelSO eventChannel;
        [SerializeField] GameStateSO gameState;
        [SerializeField] PlayerStateSO playerState;
        [SerializeField] GameObject canvas;
        [SerializeField] GameObject textPressAnyKey;

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
        int num = 0;
        float fNum = 0;

        int totalPoints => debug ? 123456 : gameState.totalPoints;
        int numEnemiesKilled => debug ? 123 : gameState.numEnemiesKilled;
        int numPlayerDeaths => debug ? 42 : playerState.numDeaths;
        float timeElapsed => debug ? 300 : GameManager.current.timeElapsed;

        private void OnEnable() {
            eventChannel.OnAnyKeyPress.Subscribe(OnAnyKeyPress);
        }

        private void OnDisable() {
            eventChannel.OnAnyKeyPress.Unsubscribe(OnAnyKeyPress);
        }

        void Start() {
            canvas.SetActive(true);
            rowPoints.SetActive(false);
            rowEnemies.SetActive(false);
            rowDeaths.SetActive(false);
            rowTime.SetActive(false);
            textPressAnyKey.SetActive(false);
            AudioManager.current.CueTrack("starlord-main-theme");

            StartCoroutine(IStats());
        }

        void OnAnyKeyPress() {
            dismiss = true;
        }

        IEnumerator IStats() {
            yield return new WaitForSeconds(timeDelayStats);

            rowPoints.SetActive(true);

            yield return IShowStat(fieldPoints, totalPoints, 1000);
            yield return new WaitForSeconds(timeBetweenStats);

            rowEnemies.SetActive(true);

            yield return IShowStat(fieldEnemies, numEnemiesKilled, 10);
            yield return new WaitForSeconds(timeBetweenStats);

            rowDeaths.SetActive(true);

            yield return IShowStat(fieldDeaths, numPlayerDeaths);
            yield return new WaitForSeconds(timeBetweenStats);

            rowTime.SetActive(true);

            yield return IShowTimeStat(fieldTime, timeElapsed);

            yield return new WaitForSeconds(timeDelayPressAnyKey);

            AudioManager.current.PlaySound("DialogueChip");
            textPressAnyKey.SetActive(true);
            dismiss = false;
            while (!dismiss) yield return null;

            GameManager.current.GotoMainMenu();
        }

        IEnumerator IShowStat(TextMeshProUGUI field, int value, int acc = 1) {
            dismiss = false;
            num = 0;
            AudioManager.current.PlaySound("DialogueChip");
            while (!dismiss && num < value) {
                field.text = num.ToString();
                AudioManager.current.PlaySound("DialogueChip");
                num = Mathf.Min(num + acc, value);
                yield return new WaitForSeconds(0.02f);
            }
            dismiss = false;
        }

        IEnumerator IShowTimeStat(TextMeshProUGUI field, float value) {
            dismiss = false;
            fNum = 0;
            AudioManager.current.PlaySound("DialogueChip");
            while (!dismiss && fNum < value) {
                field.text = Utils.ToTimeString(fNum);
                AudioManager.current.PlaySound("DialogueChip");
                fNum = Mathf.Min(fNum + 10f, value);
                yield return new WaitForSeconds(0.02f);
            }
            dismiss = false;
        }
    }
}

