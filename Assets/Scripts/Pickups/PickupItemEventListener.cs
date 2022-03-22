using UnityEngine;
using UnityEngine.Events;

namespace Pickups {

    public class PickupItemEventListener : MonoBehaviour {
        public PickupItemEvent Event;
        public UnityEvent<PickupType, float> Response;

        private void OnEnable() {
            Event.RegisterListener(this);
        }

        private void OnDisable() {
            Event.UnregisterListener(this);
        }

        public void OnEventRaised(PickupType pickupType, float value) {
            Response.Invoke(pickupType, value);
        }
    }
}

