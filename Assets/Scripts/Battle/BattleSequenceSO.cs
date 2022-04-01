
using System.Collections.Generic;
using UnityEngine;

namespace Battle {

    [CreateAssetMenu(fileName = "BattleSequence 0", menuName = "ScriptableObjects/BattleSequence")]
    public class BattleSequenceSO : ScriptableObject {
        public List<BattleEvent> battleEvents = new List<BattleEvent>();

        [TextArea(minLines: 3, maxLines: 10)]
        public string notes;
    }
}

