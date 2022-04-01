
using UnityEngine;
using UnityEngine.Events;

using Enemies;
using Dialogue;

namespace Battle {

    public enum BattleEventType {
        Wave,
        Boss,
        DestroyAllEnemiesPresent,
        WaitForArbitraryTime,
        WaitUntilEnemiesDestroyed,
        WaitUntilWaveSpawnFinished,
        WaitUntilBossDestroyed,
        ArbitraryEvent,
        EventLabel,
        PlayMusic,
        StopMusic,
        ShowDialogue,
        ShowHint,
    }

    [System.Serializable]
    public class BattleEvent {
        [SerializeField] bool skip;
        [SerializeField] BattleEventType type;
        [SerializeField] string eventLabel;
        // could be a wave, a formation, a boss, incoming asteroids etc.
        [SerializeField] WaveConfigSO wave;
        [SerializeField] GameObject formation;
        [SerializeField] GameObject boss;
        [SerializeField] float arbitraryTime = 0f;
        [SerializeField] int allowableEnemiesLeft = 0;
        [SerializeField] UnityEvent arbitraryEvent;
        [SerializeField] DialogueItemSO _dialogueItem;
        [SerializeField] HintSO _hint;

        public bool Skip => skip;
        public BattleEventType Type => type;
        public WaveConfigSO Wave => wave;
        public float ArbitraryTime => arbitraryTime;
        public int AllowableEnemiesLeft => allowableEnemiesLeft;
        public UnityEvent ArbitraryEvent => arbitraryEvent;
        public DialogueItemSO dialogueItem => _dialogueItem;
        public HintSO hint => _hint;
    }
}
