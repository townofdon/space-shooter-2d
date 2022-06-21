using UnityEngine;

using Core;

namespace UI {

    public class TutorialBillboard : MonoBehaviour {
        [SerializeField] Canvas canvas;

        void Start() {
            canvas.worldCamera = Utils.GetCamera();
        }
    }
}