using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Core;
using Damage;

namespace UI {

    public enum HealthBarState {
        FillingUp,
        ShowingHealth,
    }

    public class HealthBarUI : MonoBehaviour {

        [SerializeField] float initFillTime = 1f;

        [Space]

        [SerializeField] Canvas canvas;
        [SerializeField] Slider healthSlider;

        // cached
        DamageableBehaviour actor;
        // RectTransform healthSliderTransform;

        // state
        Timer initFill = new Timer(TimerDirection.Increment);
        HealthBarState state;
        // Vector3 position;
        float health;

        void Start() {
            actor = GetComponentInParent<DamageableBehaviour>();
            if (actor == null || !actor.isAlive) { Destroy(gameObject); return; }
            // healthSliderTransform = healthSlider.GetComponent<RectTransform>();
            SetHealth(0);
            DrawHealth();
            canvas.enabled = true;
            StartCoroutine(IInitFill());
        }

        void Update() {
            if (actor == null || !actor.isAlive) { Destroy(gameObject); return; }
            DrawHealth();
            if (state == HealthBarState.ShowingHealth) {
                SetHealth(actor.healthPct);
            }
        }

        void DrawHealth() {
            // position = Utils.GetCamera().WorldToScreenPoint(transform.position);
            // healthSliderTransform.position = position;
            healthSlider.value = health;
        }

        void SetHealth(float value) {
            health = value;
        }

        IEnumerator IInitFill() {
            initFill.SetDuration(initFillTime);
            initFill.Start();
            while (initFill.active) {
                SetHealth(Easing.easeInOutQuart(initFill.value));
                initFill.Tick();
                yield return null;
            }
            state = HealthBarState.ShowingHealth;
        }
    }
}

