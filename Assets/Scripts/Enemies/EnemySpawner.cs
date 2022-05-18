using UnityEngine;

using Battle;

namespace Enemies {

    public class EnemySpawner : BattlePlayer {
        void Start() {
            Init();
            StartBattle();
        }
    }
}
