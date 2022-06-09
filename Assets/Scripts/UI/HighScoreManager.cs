
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

using Core;

namespace UI {

    public struct HighScore {
        private string _name;
        private int _score;
        public HighScore(string __name, int __score) {
            _name = __name;
            _score = __score;
        }
        public string name => _name;
        public int score => _score;
    }

    public class HighScoreManager : MonoBehaviour {
        const string fullURL = "http://dreamlo.com/lb/CXtID4nu5E2MCjMOxOdLpwt43bLfU56UmCX1z8Uofb4Q";
        const string privateCode = "CXtID4nu5E2MCjMOxOdLpwt43bLfU56UmCX1z8Uofb4Q";
        const string publicCode = "629feb6d8f40bb11c075f7a9";
        const string webURL = "http://dreamlo.com/lb/";
        const int numHighScores = 10;

        // DisplayHighScores highscoreDisplay;
        HighScore[] highScores = new HighScore[numHighScores];
        // static HighScores instance;

        void Awake() {
            // highscoreDisplay = GetComponent<DisplayHighScores>();

            // -----------------------------------
            // Debug.Log("FETCHING HIGH SCORES...");
            // GetHighScores((HighScore[] scores) => {
            //     Debug.Log("------------");
            //     Debug.Log("HIGH SCORES");
            //     foreach (var score in scores) {
            //         Debug.Log(score.name + ": " + score.score);
            //     }
            // });
            // -----------------------------------
            // Debug.Log("ADDING HIGH SCORES...");
            // AddHighScore("ARTEMISPRG", 10000, (HighScore[] scores) => { });
            // AddHighScore("BOSONHIGGS", 20000, (HighScore[] scores) => { });
            // AddHighScore("CYLONCMNDR", 30000, (HighScore[] scores) => { });
            // AddHighScore("DEGOBAHDAN", 40000, (HighScore[] scores) => { });
            // AddHighScore("EPICLYEVAN", 50000, (HighScore[] scores) => { });
            // AddHighScore("FREDDY_FOX", 60000, (HighScore[] scores) => { });
            // AddHighScore("GIANTSDERP", 70000, (HighScore[] scores) => { });
            // AddHighScore("HALLY-8000", 80000, (HighScore[] scores) => { });
            // AddHighScore("INDIGO_IRE", 90000, (HighScore[] scores) => { });
            // AddHighScore("JUGGERNAUT", 100000, (HighScore[] scores) => { });
        }

        public void AddHighScore(string username, int score, System.Action<HighScore[]> OnHighScores) {
            StartCoroutine(Utils.WaitFor(IAddHighScore(username, score), () => OnHighScores(highScores)));
        }


        public void GetHighScores(System.Action<HighScore[]> OnHighScores) {
            StartCoroutine(Utils.WaitFor(IGetHighScores(), () => OnHighScores(highScores)));
        }

        IEnumerator IAddHighScore(string username, int score) {
            UnityWebRequest www = UnityWebRequest.Get(webURL + privateCode + "/add/" + UnityWebRequest.EscapeURL(username) + "/" + score);
            yield return www.SendWebRequest();
            switch (www.result) {
                case UnityWebRequest.Result.Success:
                    yield return IGetHighScores();
                    break;
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                default:
                    Debug.LogWarning("Error uploading: " + www.responseCode + " " + www.error);
                    break;
            }
        }

        IEnumerator IGetHighScores() {
            UnityWebRequest www = UnityWebRequest.Get(webURL + publicCode + "/pipe/");
            yield return www.SendWebRequest();
            switch (www.result) {
                case UnityWebRequest.Result.Success:
                    FormatHighScores(www.downloadHandler.text);
                    break;
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                default:
                    Debug.LogWarning("Error downloading: " + www.responseCode + " " + www.error);
                    break;
            }
        }

        void FormatHighScores(string text) {
            string[] entries = text.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            highScores = new HighScore[Mathf.Min(entries.Length, numHighScores)];
            for (int i = 0; i < Mathf.Min(entries.Length, numHighScores); i++) {
                string[] entryInfo = entries[i].Split(new char[] { '|' });
                string username = entryInfo[0];
                int score = int.Parse(entryInfo[1]);
                highScores[i] = new HighScore(username, score);
            }
        }

    }
}

