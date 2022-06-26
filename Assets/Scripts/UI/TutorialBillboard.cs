using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

using Core;
using Player;

namespace UI {

    public class TutorialBillboard : MonoBehaviour {
        [SerializeField] Canvas canvas;
        [SerializeField] GameObject gamepadButtons;
        [SerializeField] GameObject keyboardKeys;
        [SerializeField] GameObject schemeGamepad;
        [SerializeField] GameObject schemeKeyboard;

        string controlScheme = "Gamepad";

        PlayerGeneral player;
        PlayerInput playerInput;

        void Start() {
            canvas.worldCamera = Utils.GetCamera();
            StartCoroutine(ISetControlScheme());
        }

        IEnumerator ISetControlScheme() {
            while (playerInput == null) {
                playerInput = GetPlayerInput();
                yield return null;
            }
            controlScheme = playerInput.currentControlScheme;
            if (controlScheme == "Gamepad") {
                gamepadButtons.SetActive(true);
                schemeGamepad.SetActive(true);
                keyboardKeys.SetActive(false);
                schemeKeyboard.SetActive(false);
            } else {
                gamepadButtons.SetActive(false);
                schemeGamepad.SetActive(false);
                keyboardKeys.SetActive(true);
                schemeKeyboard.SetActive(true);
            }
        }

        PlayerInput GetPlayerInput() {
            if (playerInput != null) return playerInput;
            player = PlayerUtils.FindPlayer();
            if (player == null) return null;
            return player.GetComponent<PlayerInput>();
        }
    }
}