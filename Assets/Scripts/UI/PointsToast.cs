using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Core;

namespace UI {

    public class PointsToast : MonoBehaviour {

        [SerializeField] bool isNegative = false;
        [SerializeField] float lifetime = 2f;
        [SerializeField] float driftMag = 50f;
        [SerializeField] AnimationCurve driftY = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] Gradient gradient;
        [SerializeField] TextMeshProUGUI text;

        Vector3 position;
        float t = 0;
        int points = 0;

        public void SetPoints(int value) {
            points = value;
        }

        void Start() {
            position = Utils.GetCamera().WorldToScreenPoint(transform.position);
            text.rectTransform.position = position;
            text.text = (isNegative ? "-" : "") + points.ToString();
            if (points <= 0) {
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
        }

        void Update() {
            text.rectTransform.position = position + Vector3.up * driftMag * driftY.Evaluate(t / lifetime);
            text.color = gradient.Evaluate(t / lifetime);
            t += Time.deltaTime;
            if (t > lifetime) {
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
        }
    }
}
