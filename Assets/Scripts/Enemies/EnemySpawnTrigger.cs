using UnityEngine;

using Battle;

namespace Enemies {

    public class EnemySpawnTrigger : BattlePlayer {
        [SerializeField] float triggerPosY = 0f;

        bool didTrigger;

        void Start() {
            Init();
        }

        void Update() {
            if (didTrigger) return;
            if (transform.position.y <= triggerPosY) {
                didTrigger = true;
                StartBattle();
            }
        }
    }
}
