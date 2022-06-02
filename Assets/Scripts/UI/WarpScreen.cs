using System.Collections;
using UnityEngine;

using Player;
using Game;
using Audio;

namespace UI {

    public class WarpScreen : MonoBehaviour {
        [SerializeField] float timeToUpgradeScreen = 5f;

        [Space]

        [SerializeField] GameObject shipRed;
        [SerializeField] GameObject shipYellow;
        [SerializeField] GameObject shipBlue;
        [SerializeField] GameObject shipGreen;

        [Space]

        [SerializeField] PlayerStateSO playerState;
        // [SerializeField] EventChannelSO eventChannel;

        IEnumerator Start() {
            if (playerState.shipColor == PlayerShipColor.Red) shipRed.SetActive(true);
            if (playerState.shipColor == PlayerShipColor.Yellow) shipYellow.SetActive(true);
            if (playerState.shipColor == PlayerShipColor.Blue) shipBlue.SetActive(true);
            if (playerState.shipColor == PlayerShipColor.Green) shipGreen.SetActive(true);
            AudioManager.current.PlaySound("ship-whoosh");

            yield return new WaitForSeconds(timeToUpgradeScreen);

            GameManager.current.GotoUpgradeScene();
        }
    }
}