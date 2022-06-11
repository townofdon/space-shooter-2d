using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Core;

namespace UI {

    public class TextToast : MonoBehaviour {

        [SerializeField] float lifetime = 2f;
        [SerializeField] float driftMag = 50f;
        [SerializeField] AnimationCurve driftY = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] Gradient gradient;
        [SerializeField] TextMeshProUGUI textField;

        Vector3 position;
        float t = 0;

        void Start() {
            position = Utils.GetCamera().WorldToScreenPoint(transform.position);
            textField.rectTransform.position = position;
        }

        void Update() {
            textField.rectTransform.position = position + Vector3.up * driftMag * driftY.Evaluate(t / lifetime);
            textField.color = gradient.Evaluate(t / lifetime);
            t += Time.deltaTime;
            if (t > lifetime) {
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
        }
    }
}
