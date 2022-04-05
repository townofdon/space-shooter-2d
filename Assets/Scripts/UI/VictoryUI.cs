using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Event;
using Weapons;
using Core;
using Player;
using Game;

namespace UI {

    public class VictoryUI : MonoBehaviour {

        [SerializeField] EventChannelSO eventChannel;
        [SerializeField] GameObject canvas;

        bool isShowing;

        void OnEnable() {
            eventChannel.OnShowVictory.Subscribe(OnShowVictory);
            eventChannel.OnHideVictory.Subscribe(OnHideVictory);
        }

        void OnDisable() {
            eventChannel.OnShowVictory.Unsubscribe(OnShowVictory);
            eventChannel.OnHideVictory.Unsubscribe(OnHideVictory);
        }

        void Awake() {
            canvas.SetActive(false);
        }

        void OnShowVictory() {
            isShowing = true;
            canvas.SetActive(true);
        }

        void OnHideVictory() {
            isShowing = false;
            canvas.SetActive(false);
        }
    }
}

