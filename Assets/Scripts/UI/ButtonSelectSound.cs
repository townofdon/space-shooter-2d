using UnityEngine;
using UnityEngine.EventSystems;

using Audio;

namespace UI {

    public class ButtonSelectSound : MonoBehaviour, ISelectHandler, IDeselectHandler {

        void ISelectHandler.OnSelect(BaseEventData data) {
            OnSelect();
        }

        void IDeselectHandler.OnDeselect(BaseEventData data) { }

        void OnSelect() {
            AudioManager.current.PlaySound("MenuFocus");
        }
    }
}
