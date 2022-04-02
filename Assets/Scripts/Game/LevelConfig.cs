using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

using Audio;
using Core;
using Player;

namespace Game {

    public class LevelConfig : MonoBehaviour {
        [SerializeField] string musicTrack = "starlord-main-theme";
        [SerializeField] string levelName;
        [SerializeField] GameObject transitionCanvas;
        [SerializeField] TextMeshProUGUI levelTitle;


        void Start() {
            StartCoroutine(ILevelStart());
        }

        IEnumerator ILevelStart() {
            AudioManager.current.PlayTrack(musicTrack);
            levelTitle.text = levelName;
            transitionCanvas.SetActive(true);
            yield return new WaitForSeconds(2f);
            transitionCanvas.SetActive(false);
            GameManager.current.RespawnPlayerShip();
            PlayerUtils.InvalidateCache();
        }
    }
}

