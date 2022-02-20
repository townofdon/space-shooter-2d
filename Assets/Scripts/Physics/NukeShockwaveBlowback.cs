using UnityEngine;

using Core;

namespace Physics
{

  public class NukeShockwaveBlowback : MonoBehaviour
  {
      [Header("Nuke Shockwave Blowback")][Space]
      [SerializeField] AnimationCurve shockwaveVelocity = AnimationCurve.EaseInOut(0f, 0.75f, 1f, -0.75f);
      [SerializeField] float shockwaveDuration = 1f;

      // components
      Rigidbody2D rb;
      Animator anim;

      // state
      Timer shockwavePositionTimer = new Timer(TimerDirection.Increment, TimerStep.FixedDeltaTime);

      // state - velocity
      Vector2 initialVelocity;
      Vector2 blastbackDirection;

      void Awake() {
          rb = GetComponentInParent<Rigidbody2D>();
          shockwavePositionTimer.SetDuration(shockwaveDuration);
          shockwavePositionTimer.End();
          if (rb != null) initialVelocity = rb.velocity;
      }

      void FixedUpdate() {
          if (rb == null) return;
          rb.velocity = shockwavePositionTimer.active
              ? Vector2.Lerp(
                  rb.velocity + blastbackDirection * shockwaveVelocity.Evaluate(shockwavePositionTimer.value),
                  initialVelocity,
                  shockwavePositionTimer.value)
              : rb.velocity;
          shockwavePositionTimer.Tick();
      }

      void OnTriggerEnter2D(Collider2D other) {
          if (shockwavePositionTimer.active) return;
          if (other.tag == UTag.NukeShockwave) {
              initialVelocity = rb.velocity;
              shockwavePositionTimer.Start();
              blastbackDirection = (transform.position - other.transform.position).normalized;
              return;
          }
      }
  }
}

