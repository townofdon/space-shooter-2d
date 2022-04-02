using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI {

    public class UIButton {
        Image _backgroundImage;
        Image _image;
        TextMeshProUGUI _text;
        Button _button;
        Color _initialButtonColor;
        Color _initialTextColor;
        Color _initialImageColor;

        public Button button => _button;
        public TextMeshProUGUI text => _text;

        // state
        bool disabled; // note - this only controls the APPEARANCE of being disabled; button is still interactable

        public UIButton(Button initialButton) {
            _button = initialButton;
            _backgroundImage = _button.GetComponent<Image>();
            _text = _button.GetComponentInChildren<TextMeshProUGUI>();
            _image = GetInnerImage();
            _initialButtonColor = _backgroundImage.color;
            if (_text != null) _initialTextColor = _text.color;
            if (_image != null) _initialImageColor = _image.color;
        }

        public void SetTextColorInherit() {
            if (_text == null) return;
            _text.color = _backgroundImage.color;
        }

        public void SetTextColorInitial() {
            if (_text == null) return;
            _text.color = _initialTextColor;
        }

        public void Enable() {
            _backgroundImage.color = _initialButtonColor;
            if (_text != null) _text.color = _initialTextColor;
            if (_image != null) _image.color = _initialImageColor;
        }

        public void Disable() {
            _backgroundImage.color = _button.colors.disabledColor;
            if (_text != null) _text.color = _button.colors.disabledColor;
            if (_image != null) _image.color = _button.colors.disabledColor;
        }

        Image GetInnerImage() {
            if (_button.transform.childCount == 0) return null;
            Transform child = _button.transform.GetChild(0);
            return child.GetComponent<Image>();
        }
    }
}
