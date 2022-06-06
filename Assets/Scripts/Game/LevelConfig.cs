using System.Collections;
using UnityEngine;

using TMPro;

using Audio;
using Player;

namespace Game {

    public class LevelConfig : MonoBehaviour {
        [SerializeField] string musicTrack = "starlord-main-theme";
        [SerializeField] string levelName;
        [SerializeField] GameObject transitionCanvas;
        [SerializeField] TextMeshProUGUI levelTitle;
        [SerializeField] bool ShowTitleScreen = true;
        [SerializeField] LevelStateSO levelState;

        Coroutine ieLevelStart;

        void Start() {
            if (ieLevelStart != null) StopCoroutine(ieLevelStart);
            StartCoroutine(ILevelStart());
            levelState.SetAsCurrentLevelIfActive();
        }

        IEnumerator ILevelStart() {
            if (musicTrack != "") AudioManager.current.PlayTrack(musicTrack);
            if (ShowTitleScreen) {
                levelTitle.text = levelName;
                transitionCanvas.SetActive(true);
                yield return new WaitForSeconds(2f);
                transitionCanvas.SetActive(false);
            }
            GameManager.current.StartGameTimer();
            GameManager.current.RespawnPlayerShip();
            PlayerUtils.InvalidateCache();
            AudioManager.current.PlaySound("ship-whoosh");
        }
    }
}

