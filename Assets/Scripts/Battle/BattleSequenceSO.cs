using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Enemies;

namespace Battle {

    public enum BattleEventType {
        Wave,
        Formation,
        Boss,
        SpawnEnemy,
        DestroyAllEnemiesPresent,
        WaitForArbitraryTime,
        WaitUntilEnemiesDestroyed,
        WaitUntilWaveSpawnFinished,
        ArbitraryEvent,
        EventLabel,
        // ChangeMusic,
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

        public bool Skip => skip;
        public BattleEventType Type => type;
        public WaveConfigSO Wave => wave;
        public float ArbitraryTime => arbitraryTime;
        public int AllowableEnemiesLeft => allowableEnemiesLeft;
        public UnityEvent ArbitraryEvent => arbitraryEvent;
    }

    [CreateAssetMenu(fileName = "BattleSequence 0", menuName = "ScriptableObjects/BattleSequence")]
    public class BattleSequence : ScriptableObject {
        public List<BattleEvent> battleEvents = new List<BattleEvent>();
    }
}

