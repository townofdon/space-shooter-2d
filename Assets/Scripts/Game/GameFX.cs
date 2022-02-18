using System.Collections;
using UnityEngine;

namespace Game
{    

    public class GameFX : MonoBehaviour
    {
        [SerializeField] ParticleSystem starfield;
        [SerializeField] ParticleSystem starfieldWarp;
        [SerializeField] float starfieldWarpSpeed = 100f;
        [SerializeField] float starfieldWarpAccel = 100f;

        float speedModifier = 1f;

        public void HandleBattleFinished() {
            Debug.Log("YOU WIN!");
            StartCoroutine(IWarpOnBattleFinished());
        }

        IEnumerator IWarpOnBattleFinished() {
            // if (starfield != null) starfield.Stop();
            if (starfieldWarp != null) {
                starfieldWarp.Play();
            }

            // TODO: UPGRADE OPTIONS

            // TODO: LOAD NEW LEVEL

            yield return null;
        }
    }
}
