using UnityEngine;

namespace Game {
    public class GameColours : MonoBehaviour, ISerializationCallbackReceiver {

        public Color _inspectorBattleEventWave;
        public Color _inspectorBattleEventFormation;
        public Color _inspectorBattleEventBoss;
        public Color _inspectorBattleEventWait;
        public Color _inspectorBattleEventEvent;
        public Color _inspectorBattleEventLabel;

        public static Color inspectorBattleEventWave;
        public static Color inspectorBattleEventFormation;
        public static Color inspectorBattleEventBoss;
        public static Color inspectorBattleEventWait;
        public static Color inspectorBattleEventEvent;
        public static Color inspectorBattleEventLabel;

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            inspectorBattleEventWave = _inspectorBattleEventWave == null ? Color.black : _inspectorBattleEventWave;
            inspectorBattleEventFormation = _inspectorBattleEventFormation == null ? Color.black : _inspectorBattleEventFormation;
            inspectorBattleEventBoss = _inspectorBattleEventBoss == null ? Color.black : _inspectorBattleEventBoss;
            inspectorBattleEventWait = _inspectorBattleEventWait == null ? Color.black : _inspectorBattleEventWait;
            inspectorBattleEventEvent = _inspectorBattleEventEvent == null ? Color.black : _inspectorBattleEventEvent;
            inspectorBattleEventLabel = _inspectorBattleEventLabel == null ? Color.black : _inspectorBattleEventLabel;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() {

        }
    }
}
