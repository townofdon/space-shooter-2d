using UnityEngine;

using Event;
using Player;

namespace UI {

    public class UpgradeScreen : MonoBehaviour {
        [SerializeField] GameObject shipRed;
        [SerializeField] GameObject shipYellow;
        [SerializeField] GameObject shipBlue;
        [SerializeField] GameObject shipGreen;

        [Space]

        [SerializeField] PlayerStateSO playerState;
        [SerializeField] EventChannelSO eventChannel;

        void Start() {
            if (playerState.shipColor == PlayerShipColor.Red) shipRed.SetActive(true);
            if (playerState.shipColor == PlayerShipColor.Yellow) shipYellow.SetActive(true);
            if (playerState.shipColor == PlayerShipColor.Blue) shipBlue.SetActive(true);
            if (playerState.shipColor == PlayerShipColor.Green) shipGreen.SetActive(true);
            eventChannel.OnShowUpgradePanel.Invoke();
        }
    }
}