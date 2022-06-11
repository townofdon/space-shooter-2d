using UnityEngine;
using TMPro;

using Core;
using Audio;
using Event;
using System.Collections.Generic;

namespace UI {

    public class HighScoreEntry : MonoBehaviour {
        [SerializeField] EventChannelSO eventChannel;
        [Space]
        [SerializeField] GameObject canvas;
        [SerializeField] Transform charContainer;
        [Space]
        [SerializeField][Range(0f, 60f)] float charSelectRate = 1f;
        [SerializeField][Range(0f, 5f)] float charAccel = 1f;
        [SerializeField][Range(0f, 5f)] float charDecel = 2f;
        [SerializeField] AnimationCurve charAccelCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] AnimationCurve charDecelCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [Space]
        [SerializeField][Range(0f, 5f)] float fieldSelectTimeThreshold = 0.7f;
        [SerializeField][Range(0f, 60f)] float fieldSelectRateSlow = 1f;
        [SerializeField][Range(0f, 60f)] float fieldSelectRateFast = 1f;
        [Space]
        [SerializeField] Color deselectedCharColor;
        [SerializeField] Gradient selectedCharGradient;

        UIInputHandler input;

        TextMeshProUGUI[] fields;
        UISquare[] squares;

        string currentChar = "A";
        int fieldIndex = 0;
        int prevCharIndex = 0;
        float colorTime = 0f;
        float charDelta = 0f;
        float charIndex = 0f;

        bool inputDisabled = true;
        bool isAccelerating = false;
        bool isFirstCharChange = false;
        bool didSelect = true;
        bool didSubmit = false;
        bool didFieldChange = false;

        Timer fieldSpeed = new Timer(TimerDirection.Increment);
        Timer fieldChangeSlow = new Timer(TimerDirection.Increment);
        Timer fieldChangeFast = new Timer(TimerDirection.Increment);
        bool isFieldSelectFast = false;

        public void CancelSubmit() {
            didSubmit = false;
            fieldIndex = fields.Length - 1;
            OnFieldChanged();
            SetCharIndexFromCurrent();
            didSelect = true;
        }

        public void EnableInput() {
            inputDisabled = false;
            didFieldChange = false;
        }

        public void DisableInput() {
            inputDisabled = true;
        }

        void Start() {
            input = GetComponent<UIInputHandler>();
            fields = charContainer.GetComponentsInChildren<TextMeshProUGUI>();
            squares = charContainer.GetComponentsInChildren<UISquare>();
            foreach (var field in fields) field.text = "";
            fields[0].text = "A";
            SetCharIndexFromCurrent();
            fieldSpeed.SetDuration(fieldSelectTimeThreshold);
            fieldChangeSlow.SetDuration(fieldSelectRateSlow);
            fieldChangeFast.SetDuration(fieldSelectRateFast);
            DisableInput();
        }

        void Update() {
            if (!canvas.activeSelf) { didSelect = true; didFieldChange = false; return; }
            UpdateChar();
            HandleInput();
            HandleSelectedColor();
            fieldSpeed.Tick();
            fieldChangeSlow.Tick();
            fieldChangeFast.Tick();
            charDelta = Mathf.Clamp(charDelta, -1f, 1f);
            colorTime += Time.deltaTime;
            if (colorTime > 1f) colorTime = 0f;
            if (Mathf.Abs(charDelta) > Mathf.Epsilon) {
                charIndex += Mathf.Sign(charDelta)
                    * (isAccelerating
                        ? charAccelCurve.Evaluate(Mathf.Abs(charDelta))
                        : charDecelCurve.Evaluate(Mathf.Abs(charDelta)))
                    * charSelectRate * Time.deltaTime;
            }
        }

        void HandleSelectedColor() {
            for (int i = 0; i < fields.Length; i++) {
                if (i == fieldIndex) {
                    fields[i].color = selectedCharGradient.Evaluate(colorTime);
                    squares[i].color = selectedCharGradient.Evaluate(colorTime);
                } else {
                    fields[i].color = deselectedCharColor;
                    squares[i].color = deselectedCharColor;
                }
            }
        }

        void OnSubmitName() {
            if (!canvas.activeSelf) return;
            if (didSubmit) return;
            didFieldChange = false;
            didSubmit = true;
            string name = GetFullText();
            eventChannel.OnSubmitName.Invoke(name);
        }

        void HandleInput() {
            if (!canvas.activeSelf) return;
            if (inputDisabled) return;
            if (didSubmit) return;
            if (input.move.y > 0.8f) {
                if (!isFirstCharChange) {
                    isFirstCharChange = true;
                    charIndex = Mathf.Floor(charIndex) + 1f;
                    charDelta = 0f;
                    return;
                }
                charDelta += charAccel * Time.deltaTime;
                isAccelerating = charDelta > 0;
                return;
            }
            if (input.move.y < -0.8f) {
                if (!isFirstCharChange) {
                    isFirstCharChange = true;
                    charIndex = Mathf.Floor(charIndex) == charIndex ? charIndex - Mathf.Epsilon : Mathf.Floor(charIndex) - Mathf.Epsilon;
                    charDelta = 0f;
                    return;
                }
                charDelta -= charAccel * Time.deltaTime;
                isAccelerating = charDelta < 0;
                return;
            }

            isFirstCharChange = false;

            if (charDelta > Mathf.Epsilon) {
                isAccelerating = false;
                charDelta -= charDecel * Time.deltaTime;
                charDelta = Mathf.Max(charDelta, 0f);
            } else if (charDelta < Mathf.Epsilon) {
                isAccelerating = false;
                charDelta += charDecel * Time.deltaTime;
                charDelta = Mathf.Min(charDelta, 0f);
            }

            // handle backspace
            if (!didSelect && (input.backSpace.isPressed || input.isCanceling)) {
                didSelect = true;
                fields[GetFieldIndex()].text = " ";
                fieldIndex--;
                currentChar = fields[GetFieldIndex()].text = " ";
                OnFieldChanged();
                SetCharIndexFromCurrent();
                input.backSpace.AcknowledgePress();
                return;
            }

            // handle keyboard press
            if (!didSelect && input.keyPress.isPressed && IsValidChar(input.keyPress.text) && fieldIndex < fields.Length) {
                currentChar = fields[fieldIndex].text = input.keyPress.text;
                fieldIndex++;
                OnFieldChanged();
                // SetCharIndexFromCurrent(currentChar == " " ? " " : "A");
                SetCharIndexFromCurrent();
                input.keyPress.AcknowledgePress();
                return;
            }

            // handle submit
            if (!didSelect && !didSubmit && input.isSubmitting && didFieldChange) {
                didSelect = true;
                OnSubmitName();
                return;
            }

            // handle next field
            if (CanSelect() && input.move.x > 0.9f) {
                fieldIndex++;
                OnFieldChanged();
                // SetCharIndexFromCurrent(currentChar == " " ? " " : "A");
                SetCharIndexFromCurrent();
                return;
            }

            // handle prev field
            if (CanSelect() && input.move.x < -0.9f && fieldIndex > 0) {
                fieldIndex--;
                OnFieldChanged();
                SetCharIndexFromCurrent();
                return;
            }

            // handle no input
            if (Mathf.Abs(input.move.x) <= Mathf.Epsilon && !input.isSubmitting && !input.isCanceling) {
                didSelect = false;
                fieldSpeed.Start();
                fieldChangeSlow.End();
                fieldChangeFast.End();
                return;
            }
        }

        bool CanSelect() {
            if (didSelect) return false;
            if (fieldSpeed.active && fieldChangeSlow.active) return false;
            if (fieldChangeFast.active) return false;
            return true;
        }

        void OnFieldChanged() {
            didFieldChange = true;
            charDelta = 0f;
            prevCharIndex = -1;
            isFirstCharChange = false;
            fieldIndex = Mathf.Clamp(fieldIndex, 0, fields.Length);
            PlaySoundCommitChar();
            if (fieldSpeed.active) {
                if (!fieldChangeSlow.active) fieldChangeSlow.Start();
            } else {
                if (!fieldChangeFast.active) fieldChangeFast.Start();
            }
        }

        void UpdateChar(int delta = 1) {
            if (prevCharIndex == GetCharIndex()) return;
            prevCharIndex = GetCharIndex();
            colorTime = 0f;
            UpdateCharText();
            PlaySoundSelectChar();
        }

        void PlaySoundSelectChar() {
            if (canvas.activeSelf) AudioManager.current.PlaySound("MenuSwitch");
        }

        void PlaySoundCommitChar() {
            if (canvas.activeSelf) AudioManager.current.PlaySound("DialogueChip");
        }

        void UpdateCharText() {
            if (fieldIndex < 0 || fieldIndex >= fields.Length) return;
            currentChar = fields[fieldIndex].text = chars[GetCharIndex()];
        }

        void SetCharIndexFromCurrent(string defaultValue = " ") {
            if (fieldIndex < fields.Length) {
                currentChar = fields[fieldIndex].text;
                if (currentChar == " ") currentChar = defaultValue;
            } else {
                currentChar = " ";
            }
            charIndex = Mathf.Max(FindCharIndex(currentChar), 0) + Mathf.Epsilon;
        }

        int GetCharIndex() {
            if (charIndex < 0) {
                charIndex = chars.Length - 1 + Mathf.Epsilon;
            } else if (charIndex >= chars.Length) {
                charIndex = Mathf.Epsilon;
            }
            return Mathf.Clamp(Mathf.FloorToInt(charIndex % chars.Length), 0, chars.Length - 1);
        }

        int GetFieldIndex() {
            return Mathf.Clamp(fieldIndex, 0, fields.Length - 1);
        }

        string GetFullText() {
            string fullText = "";
            foreach (var field in fields) fullText += field.text;
            return fullText.Trim();
        }

        int FindCharIndex(string search) {
            for (int i = 0; i < chars.Length; i++) {
                if (chars[i] == search) return i;
            }
            return -1;
        }

        bool IsValidChar(string ch) {
            for (int i = 0; i < chars.Length; i++) {
                if (ch == chars[i]) return true;
            }
            return false;
        }

        Dictionary<KeyCode, string> keyCodeLookup = new Dictionary<KeyCode, string>() {
            { KeyCode.Space, " "},
            { KeyCode.A, "A"},
            { KeyCode.B, "B"},
            { KeyCode.C, "C"},
            { KeyCode.D, "D"},
            { KeyCode.E, "E"},
            { KeyCode.F, "F"},
            { KeyCode.G, "G"},
            { KeyCode.H, "H"},
            { KeyCode.I, "I"},
            { KeyCode.J, "J"},
            { KeyCode.K, "K"},
            { KeyCode.L, "L"},
            { KeyCode.M, "M"},
            { KeyCode.N, "N"},
            { KeyCode.O, "O"},
            { KeyCode.P, "P"},
            { KeyCode.Q, "Q"},
            { KeyCode.R, "R"},
            { KeyCode.S, "S"},
            { KeyCode.T, "T"},
            { KeyCode.U, "U"},
            { KeyCode.V, "V"},
            { KeyCode.W, "W"},
            { KeyCode.X, "X"},
            { KeyCode.Y, "Y"},
            { KeyCode.Z, "Z"},
            { KeyCode.Alpha0, "0"},
            { KeyCode.Alpha1, "1"},
            { KeyCode.Alpha2, "2"},
            { KeyCode.Alpha3, "3"},
            { KeyCode.Alpha4, "4"},
            { KeyCode.Alpha5, "5"},
            { KeyCode.Alpha6, "6"},
            { KeyCode.Alpha7, "7"},
            { KeyCode.Alpha8, "8"},
            { KeyCode.Alpha9, "9"},
            { KeyCode.Keypad0, "0"},
            { KeyCode.Keypad1, "1"},
            { KeyCode.Keypad2, "2"},
            { KeyCode.Keypad3, "3"},
            { KeyCode.Keypad4, "4"},
            { KeyCode.Keypad5, "5"},
            { KeyCode.Keypad6, "6"},
            { KeyCode.Keypad7, "7"},
            { KeyCode.Keypad8, "8"},
            { KeyCode.Keypad9, "9"},
            { KeyCode.Minus, "-"},
            { KeyCode.KeypadMinus, "-"},
            { KeyCode.Underscore, "_"},
            { KeyCode.Pipe, "|"},
            { KeyCode.Slash, "/"},
            { KeyCode.Backslash, "\\"},
        };

        string[] chars = new string[] {
            " ",
            "A",
            "B",
            "C",
            "D",
            "E",
            "F",
            "G",
            "H",
            "I",
            "J",
            "K",
            "L",
            "M",
            "N",
            "O",
            "P",
            "Q",
            "R",
            "S",
            "T",
            "U",
            "V",
            "W",
            "X",
            "Y",
            "Z",
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "-",
            "_",
            "|",
            "/",
            "\\",
        };
    }
}
