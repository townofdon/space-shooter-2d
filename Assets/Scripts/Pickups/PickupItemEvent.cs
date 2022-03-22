using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pickups {

    [CreateAssetMenu(fileName = "PickupItemEvent", menuName = "ScriptableObjects/PickupItemEvent", order = 0)]
    public class PickupItemEvent : ScriptableObject {
        private List<PickupItemEventListener> listeners = new List<PickupItemEventListener>();

        public void Raise(PickupType pickupType, float value) {
            for (int i = listeners.Count - 1; i >= 0; i--)
                listeners[i].OnEventRaised(pickupType, value);
        }

        public void RegisterListener(PickupItemEventListener listener) {
            listeners.Add(listener);
        }

        public void UnregisterListener(PickupItemEventListener listener) {
            listeners.Remove(listener);
        }
    }
}

