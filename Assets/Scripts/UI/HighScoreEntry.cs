using UnityEngine;
using TMPro;

using Core;
using Audio;

namespace UI {

    public class HighScoreEntry : MonoBehaviour {
        [SerializeField] Transform charContainer;
        [Space]
        [SerializeField][Range(0f, 60f)] float charSelectRate = 1f;
        [SerializeField][Range(0f, 5f)] float charAccel = 1f;
        [SerializeField][Range(0f, 5f)] float charDecel = 2f;
        [SerializeField] AnimationCurve charAccelCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] AnimationCurve charDecelCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
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

        bool isAccelerating = false;
        bool isFirstCharChange = false;
        bool didSelect = false;

        void Start() {
            input = GetComponent<UIInputHandler>();
            fields = charContainer.GetComponentsInChildren<TextMeshProUGUI>();
            squares = charContainer.GetComponentsInChildren<UISquare>();
            foreach (var field in fields) field.text = "";
            fields[0].text = "A";
            SetCharIndexFromCurrent();
        }

        void Update() {
            UpdateChar();
            HandleInput();
            HandleSelectedColor();
            charDelta = Mathf.Clamp(charDelta, -1f, 1f);
            colorTime += Time.deltaTime;
            if (colorTime > 1f) colorTime = 0f;
            fieldIndex = Mathf.Clamp(fieldIndex, 0, fields.Length);
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

        void OnSubmit() {
            Debug.Log(GetFullText());
        }

        void HandleInput() {
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

            if (!didSelect && input.isSubmitting && fieldIndex >= fields.Length) {
                didSelect = true;
                OnSubmit();
                return;
            }

            if (!didSelect && (input.move.x > 0.9f || input.isSubmitting)) {
                didSelect = true;
                fieldIndex++;
                charDelta = 0f;
                prevCharIndex = -1;
                isFirstCharChange = false;
                SetCharIndexFromCurrent(currentChar == "" ? "" : "A");
                PlaySoundCommitChar();
                return;
            }
            if (!didSelect && (input.move.x < -0.9f || input.isCanceling) && fieldIndex > 0) {
                didSelect = true;
                fieldIndex--;
                charDelta = 0f;
                prevCharIndex = -1;
                isFirstCharChange = false;
                SetCharIndexFromCurrent();
                PlaySoundCommitChar();
                return;
            }
            if (Mathf.Abs(input.move.x) <= Mathf.Epsilon && !input.isSubmitting && !input.isCanceling) {
                didSelect = false;
                return;
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
            AudioManager.current.PlaySound("MenuSwitch");
        }

        void PlaySoundCommitChar() {
            AudioManager.current.PlaySound("DialogueChip");
        }

        void UpdateCharText() {
            if (fieldIndex < 0 || fieldIndex >= fields.Length) return;
            currentChar = fields[fieldIndex].text = chars[GetCharIndex()];
        }

        void SetCharIndexFromCurrent(string defaultValue = "") {
            if (fieldIndex < fields.Length) {
                currentChar = fields[fieldIndex].text;
                if (currentChar == "") currentChar = defaultValue;
            } else {
                currentChar = "";
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

        string[] chars = new string[] {
            "",
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
