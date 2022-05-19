using UnityEngine;

using Battle;

namespace Enemies {

    public class EnemySpawnTrigger : BattlePlayer {
        [Space]
        [Header("Trigger Settings")]
        [SerializeField] float triggerPosY = 0f;
        [SerializeField] WaveConfigSO wave;

        bool didTrigger;

        void Start() {
            Init();
            AddWaveIfPresent();
        }

        void Update() {
            if (didTrigger) return;
            if (transform.position.y <= triggerPosY) {
                didTrigger = true;
                StartBattle();
            }
        }

        void AddWaveIfPresent() {
            if (wave == null) return;
            BattleEvent battleEvent = new BattleEvent();
            battleEvent.SetWave(wave);
            BattleSequenceSO sequence = new BattleSequenceSO();
            sequence.AddBattleEvent(battleEvent);
            AddBattleSequence(sequence);
        }
    }
}
