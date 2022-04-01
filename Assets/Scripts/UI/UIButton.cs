using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI {

    public class UIButton {
        Image image;
        TextMeshProUGUI text;
        Button button;
        Color initialButtonColor;
        Color initialTextColor;

        // state
        bool disabled; // note - this only controls the APPEARANCE of being disabled; button is still interactable

        public UIButton(Button initialButton) {
            button = initialButton;
            image = button.GetComponent<Image>();
            text = button.GetComponentInChildren<TextMeshProUGUI>();
            initialButtonColor = image.color;
            initialTextColor = text.color;
        }

        public void SetTextColorInherit() {
            text.color = image.color;
        }

        public void SetTextColorInitial() {
            text.color = initialTextColor;
        }

        public void Enable() {
            image.color = initialButtonColor;
            text.color = initialTextColor;
        }

        public void Disable() {
            image.color = button.colors.disabledColor;
            text.color = button.colors.disabledColor;
        }
    }
}
