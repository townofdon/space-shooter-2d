
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

using CandyCoded.env;
using Event;

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
        [SerializeField] EventChannelSO eventChannel;

        string fullURL;
        string privateCode;
        string publicCode;
        string webURL = "http://dreamlo.com/lb/";
        const int numHighScores = 10;

        HighScore[] highScores = new HighScore[numHighScores];
        bool _isLoadingHighScores = true;

        public bool isLoadingHighScores => _isLoadingHighScores;

        public bool IsScoreTopTen(int score) {
            for (int i = 0; i < highScores.Length && i < 10; i++) {
                if (score > highScores[i].score) return true;
            }
            return false;
        }

        public int GetHighScoreIndex(int score) {
            for (int i = 0; i < highScores.Length && i < 10; i++) {
                if (score > highScores[i].score) return i;
            }
            return -1;
        }

        public int GetHighScoreIndexByName(string name) {
            for (int i = 0; i < highScores.Length && i < 10; i++) {
                if (name == highScores[i].name) return i;
            }
            return -1;
        }

        void OnEnable() {
            eventChannel.OnSubmitHighScore.Subscribe(OnSubmitHighScore);
        }

        void OnDisable() {
            eventChannel.OnSubmitHighScore.Unsubscribe(OnSubmitHighScore);
        }

        void Awake() {
            fullURL = GetEnvValue("DREAMLO_FULL_URL");
            webURL = GetEnvValue("DREAMLO_URL");
            privateCode = GetEnvValue("DREAMLO_PRIVATE_CODE");
            publicCode = GetEnvValue("DREAMLO_PUBLIC_CODE");
        }

        void Start() {
            StartCoroutine(IFetchHighScores());
        }

        void OnSubmitHighScore(string name, int score) {
            OptimisticallyAddHighScore(name, score);
            eventChannel.OnFetchHighScores.Invoke(highScores);
            StartCoroutine(IAddHighScore(name, score));
        }

        IEnumerator IAddHighScore(string name, int score) {
            UnityWebRequest www = UnityWebRequest.Get(webURL + privateCode + "/add/" + UnityWebRequest.EscapeURL(name) + "/" + score);
            yield return www.SendWebRequest();
            switch (www.result) {
                case UnityWebRequest.Result.Success:
                    yield return IFetchHighScores();
                    break;
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                default:
                    Debug.LogWarning("Error uploading: " + www.responseCode + " " + www.error);
                    break;
            }
        }

        IEnumerator IFetchHighScores() {
            UnityWebRequest www = UnityWebRequest.Get(webURL + publicCode + "/pipe/");
            yield return www.SendWebRequest();
            switch (www.result) {
                case UnityWebRequest.Result.Success:
                    ParseHighScores(www.downloadHandler.text);
                    eventChannel.OnFetchHighScores.Invoke(highScores);
                    break;
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                default:
                    Debug.LogWarning("Error downloading: " + www.responseCode + " " + www.error);
                    break;
            }
            _isLoadingHighScores = false;
        }

        IEnumerator RefreshHighscores() {
            while (true) {
                yield return IFetchHighScores();
                yield return new WaitForSeconds(30);
            }
        }

        void ParseHighScores(string text) {
            string[] entries = text.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            highScores = new HighScore[Mathf.Min(entries.Length, numHighScores)];
            for (int i = 0; i < Mathf.Min(entries.Length, numHighScores); i++) {
                string[] entryInfo = entries[i].Split(new char[] { '|' });
                string name = entryInfo[0];
                int score = int.Parse(entryInfo[1]);
                highScores[i] = new HighScore(name, score);
            }
        }

        void OptimisticallyAddHighScore(string name, int score) {
            int newIndex = GetHighScoreIndex(score);
            if (newIndex == -1) return;
            HighScore[] newHighScores = new HighScore[numHighScores];
            for (int i = 0; i < highScores.Length; i++) {
                if (i < newIndex) {
                    newHighScores[i] = highScores[i];
                } else if (i == newIndex) {
                    newHighScores[i] = new HighScore(name, score);
                } else {
                    newHighScores[i] = highScores[i - 1];
                }
            }
            highScores = newHighScores;
        }

        string GetEnvValue(string envKey) {
            if (env.TryParseEnvironmentVariable(envKey, out string value)) {
                return value;
            }
            return "";
        }

        // void Awake() {
        //     // -----------------------------------
        //     // Debug.Log("FETCHING HIGH SCORES...");
        //     // GetHighScores((HighScore[] scores) => {
        //     //     Debug.Log("------------");
        //     //     Debug.Log("HIGH SCORES");
        //     //     foreach (var score in scores) {
        //     //         Debug.Log(score.name + ": " + score.score);
        //     //     }
        //     // });
        //     // -----------------------------------
        //     // Debug.Log("ADDING HIGH SCORES...");
        //     // AddHighScore("ARTEMISPRG", 10000);
        //     // AddHighScore("BOSONHIGGS", 20000);
        //     // AddHighScore("CYLONCMNDR", 30000);
        //     // AddHighScore("DEGOBAHDAN", 40000);
        //     // AddHighScore("EPICLYEVAN", 50000);
        //     // AddHighScore("FREDDY_FOX", 60000);
        //     // AddHighScore("GIANTSDERP", 70000);
        //     // AddHighScore("HALLY-8000", 80000);
        //     // AddHighScore("INDIGO_IRE", 90000);
        //     // AddHighScore("JUGGERNAUT", 100000);
        // }
    }
}

