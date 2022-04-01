
using System.Collections.Generic;
using UnityEngine;

namespace Dialogue {

    using UnityEngine;

    [CreateAssetMenu(fileName = "DialogueItem", menuName = "ScriptableObjects/DialogueItem", order = 0)]
    public class DialogueItemSO : ScriptableObject {
        [SerializeField] DialogueEntitySO _dialogueEntity;

        [TextArea(minLines: 3, maxLines: 10)]
        [SerializeField] List<string> _statements = new List<string>();

        public DialogueType dialogueType => _dialogueEntity.dialogueType;
        public Sprite entityImage => _dialogueEntity.entityImage;
        public string entityName => _dialogueEntity.entityName;

        // state
        Queue<string> statements = new Queue<string>();
        string lastStatement = "I have nothing more to say to you, Lord Hyperion!!";

        public void Init() {
            statements.Clear();
            foreach (var statement in _statements) {
                statements.Enqueue(statement);
            }
        }

        public bool HasNextStatement() {
            return statements.Count > 0;
        }

        public string GetNextStatement() {
            if (!HasNextStatement()) return lastStatement;
            lastStatement = statements.Dequeue();
            return lastStatement;
        }
    }
}
