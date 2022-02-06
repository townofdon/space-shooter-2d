using UnityEngine;

namespace Core {

    // see: https://easings.net/
    public static class Easing
    {
        public static float Linear(float x) {
            return x;
        }

        public static float InQuad(float x) {
            return x * x;
        }
        public static float OutQuad(float x) {
            return 1f - (1f - x) * (1f - x);
        }
        public static float InOutQuad(float x) {
            return x < 0.5f ? 2f * x * x : 1f - Mathf.Pow(-2f * x + 2f, 2f) / 2f;
        }

        public static float InCubic(float x) {
            return x * x * x;
        }
        public static float OutCubic(float x) {
            return 1f - Mathf.Pow(1f - x, 3f);
        }
        public static float InOutCubic(float x) {
            return x < 0.5f ? 4f * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 3f) / 2f;
        }

        public static float easeInQuart(float x) {
            return x * x * x * x;
        }
        public static float easeOutQuart(float x) {
            return 1f - Mathf.Pow(1 - x, 4f);
        }
        public static float easeInOutQuart(float x) {
            return x < 0.5f ? 8f * x * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 4f) / 2f;
        }

        public static float easeInQuint(float x) {
            return x * x * x * x * x;
        }
        public static float easeOutQuint(float x) {
            return 1f - Mathf.Pow(1f - x, 5f);
        }
        public static float easeInOutQuint(float x) {
            return x < 0.5f ? 16f * x * x * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 5f) / 2f;
        }

        public static float easeInExpo(float x) {
            return x == 0f ? 0f : Mathf.Pow(2f, 10f * x - 10f);
        }
        public static float easeOutExpo(float x) {
            return x == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * x);
        }
        public static float easeInOutExpo(float x) {
            return x == 0f
                ? 0f
                : x == 1f
                ? 1f
                : x < 0.5f
                ? Mathf.Pow(2f, 20f * x - 10f) / 2f
                : (2f - Mathf.Pow(2f, -20f * x + 10f)) / 2f;
        }

        public static float easeInBack(float x, float backAmount = 1.70158f) {
            return (backAmount + 1f) * x * x * x - backAmount * x * x;
        }
        public static float easeOutBack(float x, float backAmount = 1.70158f) {
            return 1f + (backAmount + 1f) * Mathf.Pow(x - 1f, 3f) + backAmount * Mathf.Pow(x - 1f, 2f);
        }
        public static float easeInOutBack(float x, float backAmount = 1.70158f, float stabilize = 1.525f) {
            return x < 0.5f
                ? (Mathf.Pow(2f * x, 2f) * (((backAmount * stabilize) + 1f) * 2f * x - (backAmount * stabilize))) / 2f
                : (Mathf.Pow(2f * x - 2f, 2f) * (((backAmount * stabilize) + 1f) * (x * 2f - 2f) + (backAmount * stabilize)) + 2f) / 2f;
        }
    }
}

