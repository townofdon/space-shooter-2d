using UnityEngine;

using Event;

namespace Battle {

    public class BattleTrigger : MonoBehaviour {
        [SerializeField] EventChannelSO eventChannel;
        [SerializeField] float triggerPointY = 0f;

        bool isTriggered = false;

        void Update() {
            if (isTriggered) return;
            if (transform.position.y <= triggerPointY) {
                Debug.Log("TRIGGER CROSSED");
                eventChannel.OnBattleTriggerCrossed.Invoke();
                Destroy(gameObject);
            }
        }
    }
}
