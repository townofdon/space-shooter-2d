
using UnityEngine;

namespace Game {

    public class Notes : MonoBehaviour {
        [SerializeField][TextArea(50, 100)] string notes;
    }
}
