
using System.Collections.Generic;
using UnityEngine;

using Game;

namespace Battle {

    [CreateAssetMenu(fileName = "BattleSequence 0", menuName = "ScriptableObjects/BattleSequence")]
    public class BattleSequenceSO : ScriptableObject {
        [SerializeField][Tooltip("Spawn at or above selected difficulty")] GameDifficulty _spawnDifficulty = GameDifficulty.Easy;
        public List<BattleEvent> battleEvents = new List<BattleEvent>();

        public GameDifficulty spawnDifficulty => _spawnDifficulty;

        [TextArea(minLines: 3, maxLines: 10)]
        public string notes;
    }
}

