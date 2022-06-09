
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace UI {

    public class HighScoreDisplay : MonoBehaviour {
        [SerializeField] HighScoreManager highscoreManager;
        [SerializeField] Transform scoresTable;
        [SerializeField] Transform scoresBGTable;

        [Space]

        [SerializeField][Range(0f, 30f)] float highlightSpeed = 5f;
        [SerializeField][Range(0, 100)] int highlightCycle = 30;
        [SerializeField][Range(-1, 10)] int currentPlayerIndex = -1;
        [SerializeField] Color currentPlayerColor;
        [SerializeField] Gradient nameGradient;
        [SerializeField] Gradient scoreGradient;
        [SerializeField] Gradient scoreBGGradient;

        float highlightedIndex = 0;

        List<TextMeshProUGUI> rankFields = new List<TextMeshProUGUI>();
        List<TextMeshProUGUI> delimFields = new List<TextMeshProUGUI>();
        List<TextMeshProUGUI> scoreFields = new List<TextMeshProUGUI>();
        List<TextMeshProUGUI> scoreBGFields = new List<TextMeshProUGUI>();
        List<TextMeshProUGUI> nameFields = new List<TextMeshProUGUI>();

        void Start() {
            foreach (Transform row in scoresTable) {
                TextMeshProUGUI[] fields = row.GetComponentsInChildren<TextMeshProUGUI>();
                rankFields.Add(fields[0]);
                delimFields.Add(fields[1]);
                nameFields.Add(fields[2]);
                scoreFields.Add(fields[3]);
            }
            foreach (Transform row in scoresBGTable) {
                TextMeshProUGUI[] fields = row.GetComponentsInChildren<TextMeshProUGUI>();
                scoreBGFields.Add(fields[0]);
            }
            for (int i = 0; i < scoreFields.Count; i++) {
                rankFields[i].text = Monospace(i.ToString("00"));
                nameFields[i].text = "";
                scoreFields[i].text = "";
                scoreBGFields[i].text = Monospace("00000000");
                rankFields[i].color = nameGradient.Evaluate((float)i / (scoreFields.Count - 1));
                delimFields[i].color = scoreBGGradient.Evaluate((float)i / (scoreFields.Count - 1));
                nameFields[i].color = nameGradient.Evaluate((float)i / (scoreFields.Count - 1));
                scoreFields[i].color = scoreGradient.Evaluate((float)i / (scoreFields.Count - 1));
                scoreBGFields[i].color = scoreBGGradient.Evaluate((float)i / (scoreFields.Count - 1));
            }
            highscoreManager = GetComponent<HighScoreManager>();
            StartCoroutine(RefreshHighscores());
        }

        void Update() {
            UpdateColours();
        }

        void UpdateColours() {
            for (int i = 0; i < scoreFields.Count; i++) {
                if (i == currentPlayerIndex || i == Mathf.FloorToInt(highlightedIndex) % highlightCycle) {
                    rankFields[i].color = currentPlayerColor;
                    nameFields[i].color = currentPlayerColor;
                    scoreFields[i].color = currentPlayerColor;
                } else {
                    rankFields[i].color = nameGradient.Evaluate((float)i / (scoreFields.Count - 1));
                    nameFields[i].color = nameGradient.Evaluate((float)i / (scoreFields.Count - 1));
                    scoreFields[i].color = scoreGradient.Evaluate((float)i / (scoreFields.Count - 1));
                }
            }
            highlightedIndex += Time.deltaTime * highlightSpeed;
        }

        public void OnHighscoresDownloaded(HighScore[] highScores) {
            for (int i = 0; i < scoreFields.Count; i++) {
                if (i < highScores.Length) {
                    nameFields[i].text = Monospace(highScores[i].name);
                    scoreFields[i].text = Monospace(highScores[i].score.ToString());
                    scoreBGFields[i].text = Monospace(highScores[i].score.ToString("00000000"));
                }
            }
        }

        IEnumerator RefreshHighscores() {
            while (true) {
                highscoreManager.GetHighScores(OnHighscoresDownloaded);
                yield return new WaitForSeconds(30);
            }
        }

        string Monospace(string score) {
            return "<mspace=0.75em>" + score + "</mspace>";
        }
    }
}

