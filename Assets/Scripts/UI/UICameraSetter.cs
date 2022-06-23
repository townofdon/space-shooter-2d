using UnityEngine;

using Core;

namespace UI {

    public class UICameraSetter : MonoBehaviour {
        [SerializeField] Canvas canvas;

        void Awake() {
            canvas.worldCamera = Utils.GetCamera();
        }
    }
}