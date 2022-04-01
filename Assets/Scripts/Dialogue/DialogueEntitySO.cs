
using UnityEngine;
using UnityEngine.UI;

namespace Dialogue {

    public enum DialogueType {
        Friendly,
        Enemy,
    }

    [CreateAssetMenu(fileName = "DialogueEntity", menuName = "ScriptableObjects/DialogueEntity", order = 0)]
    public class DialogueEntitySO : ScriptableObject {
        [SerializeField] DialogueType _dialogueType;
        [SerializeField] Sprite _entityImage;
        [SerializeField] string _entityName;

        public DialogueType dialogueType => _dialogueType;
        public Sprite entityImage => _entityImage;
        public string entityName => _entityName;
    }
}
