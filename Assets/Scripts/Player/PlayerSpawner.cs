
using Game;
using UnityEngine;

namespace Player {

    public class PlayerSpawner : MonoBehaviour {

        void Start() {
            GameManager.current.RespawnPlayerShip();
            PlayerUtils.InvalidateCache();
        }
    }
}

