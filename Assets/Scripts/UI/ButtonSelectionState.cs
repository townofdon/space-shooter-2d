using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// using Core;

namespace UI {

    public class ButtonSelectionState : MonoBehaviour, ISelectHandler, IDeselectHandler {
        GameObject caretObj;
        Image caret;
        TextMeshProUGUI text;

        bool _isSelected = false;
        public bool isSelected => _isSelected;

        Button button;
        UIButton uiButton;

        void OnDisable() {
            _isSelected = false;
        }

        void OnEnable() {
            if (EventSystem.current.currentSelectedGameObject == gameObject) OnSelect();
        }

        void Awake() {
            button = GetComponent<Button>();
            caretObj = transform.Find("Caret").gameObject;
            caret = caretObj.GetComponent<Image>();
            uiButton = new UIButton(button);
            caret.color = uiButton.mainColor;
        }

        void ISelectHandler.OnSelect(BaseEventData data) {
            OnSelect();
        }

        void IDeselectHandler.OnDeselect(BaseEventData data) {
            OnDeselect();
        }

        void OnSelect() {
            _isSelected = true;
            caret.gameObject.SetActive(true);
            uiButton.SetTextColorInherit();
        }

        void OnDeselect() {
            _isSelected = false;
            caret.gameObject.SetActive(false);
            uiButton.SetTextColorInitial();
        }

        // [SerializeField] bool debug = false;
        // Vector3 rectPosition;
        // Vector3 pointLeftCenter;
        // RectTransform rectTransform;
        // GameObject caretObj;
        // ButtonSelectionCaret caret;

        // enum ButtonCorner {
        //     BottomLeft,
        //     TopLeft,
        //     TopRight,
        //     BottomRight,
        // }
        // Vector3[] corners = new Vector3[4];

        // void Start() {
        //     button = GetComponent<Button>();
        //     rectTransform = GetComponent<RectTransform>();
        //     uiButton = new UIButton(button);

        //     CalcPosition();
        //     caretObj = Instantiate(caretPrefab, pointLeftCenter, Quaternion.identity);
        //     caret = caretObj.GetComponent<ButtonSelectionCaret>();
        //     caret.SetPosition(pointLeftCenter);
        //     caret.SetColor(uiButton.mainColor);
        // }

        // void Update() {
        //     if (rectPosition == rectTransform.position) return;
        //     CalcPosition();
        //     caret.SetPosition(pointLeftCenter);
        // }

        // void CalcPosition() {
        //     // despite the name, GetWorldCorners returns coordinates in screen space
        //     rectTransform.GetWorldCorners(corners);
        //     rectPosition = rectTransform.position;
        //     pointLeftCenter = Utils.GetCamera().ScreenToWorldPoint(
        //         rectTransform.position
        //         + Vector3.left * GetButtonWidth() * 0.5f
        //         + Vector3.up * GetButtonHeight() * 0.5f
        //     );
        // }

        // float GetButtonWidth() {
        //     return corners[(int)ButtonCorner.TopRight].x - corners[(int)ButtonCorner.BottomLeft].x;
        // }

        // float GetButtonHeight() {
        //     return corners[(int)ButtonCorner.TopLeft].y - corners[(int)ButtonCorner.BottomLeft].y;
        // }

        // void OnDrawGizmos() {
        //     if (!debug) return;
        //     Gizmos.DrawSphere(pointLeftCenter, .2f);
        //     Gizmos.color = Color.red;
        //     Gizmos.DrawSphere(corners[(int)ButtonCorner.BottomLeft], .2f);
        //     Gizmos.color = Color.yellow;
        //     Gizmos.DrawSphere(corners[(int)ButtonCorner.TopLeft], .2f);
        //     Gizmos.color = Color.green;
        //     Gizmos.DrawSphere(corners[(int)ButtonCorner.TopRight], .2f);
        //     Gizmos.color = Color.blue;
        //     Gizmos.DrawSphere(corners[(int)ButtonCorner.BottomRight], .2f);
        // }
    }
}
