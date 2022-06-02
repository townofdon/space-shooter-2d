
using UnityEngine;

namespace Audio {

    public class AudioSourcePlayer : MonoBehaviour {
        [SerializeField] bool playOnAwake = true;
        [SerializeField] AudioSource source;
        [SerializeField] Sound sound;

        void Awake() {
            if (!playOnAwake) return;
            sound.Init(this, null, source);
            sound.Stop();
        }

        void Start() {
            if (!playOnAwake) return;
            sound.Play();
        }
    }
}
