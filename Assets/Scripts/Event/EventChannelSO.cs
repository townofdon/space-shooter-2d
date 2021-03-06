
using UnityEngine;

using Weapons;
using Dialogue;
using UI;

namespace Event {
    // delegate types
    public delegate void VoidEvent();
    public delegate void IntEvent(int value);
    public delegate void FloatEvent(float value);
    public delegate void BoolEvent(bool value);
    public delegate void StringEvent(string value);
    // enemy-specific
    public delegate void EnemyDeathEvent(int instanceId, int points, bool isCountableEnemy = true);
    public delegate void BossSpawnEvent(int instanceId);
    // weapon-specific
    public delegate void WeaponAmmoEvent(WeaponType weaponType, int value);
    public delegate void WeaponEvent(WeaponType weaponType);
    // dialogue-specific
    public delegate void DialogueEvent(DialogueItemSO dialogueItem);
    public delegate void HintEvent(HintSO hint, string currentControlScheme);
    // high-score
    public delegate void HighScoreSubmitEvent(string name, int score);
    public delegate void HighScoresFetchEvent(HighScore[] highScores);

    public class VoidEventHandler {
        event VoidEvent ev;
        public void Subscribe(VoidEvent action) { ev += action; }
        public void Unsubscribe(VoidEvent action) { ev -= action; }
        public void Invoke() { if (ev != null) ev.Invoke(); }
    }

    public class IntEventHandler {
        event IntEvent ev;
        public void Subscribe(IntEvent action) { ev += action; }
        public void Unsubscribe(IntEvent action) { ev -= action; }
        public void Invoke(int value) { if (ev != null) ev.Invoke(value); }
    }

    public class FloatEventHandler {
        event FloatEvent ev;
        public void Subscribe(FloatEvent action) { ev += action; }
        public void Unsubscribe(FloatEvent action) { ev -= action; }
        public void Invoke(float value) { if (ev != null) ev.Invoke(value); }
    }

    public class BoolEventHandler {
        event BoolEvent ev;
        public void Subscribe(BoolEvent action) { ev += action; }
        public void Unsubscribe(BoolEvent action) { ev -= action; }
        public void Invoke(bool value) { if (ev != null) ev.Invoke(value); }
    }

    public class StringEventHandler {
        event StringEvent ev;
        public void Subscribe(StringEvent action) { ev += action; }
        public void Unsubscribe(StringEvent action) { ev -= action; }
        public void Invoke(string value) { if (ev != null) ev.Invoke(value); }
    }

    public class EnemyDeathEventHandler {
        event EnemyDeathEvent ev;
        public void Subscribe(EnemyDeathEvent action) { ev += action; }
        public void Unsubscribe(EnemyDeathEvent action) { ev -= action; }
        public void Invoke(int instanceId, int points, bool isCountableEnemy = true) { if (ev != null) ev.Invoke(instanceId, points, isCountableEnemy); }
    }

    public class BossSpawnEventHandler {
        event BossSpawnEvent ev;
        public void Subscribe(BossSpawnEvent action) { ev += action; }
        public void Unsubscribe(BossSpawnEvent action) { ev -= action; }
        public void Invoke(int instanceId) { if (ev != null) ev.Invoke(instanceId); }
    }

    public class WeaponAmmoEventHandler {
        event WeaponAmmoEvent ev;
        public void Subscribe(WeaponAmmoEvent action) { ev += action; }
        public void Unsubscribe(WeaponAmmoEvent action) { ev -= action; }
        public void Invoke(WeaponType weaponType, int value) { if (ev != null) ev.Invoke(weaponType, value); }
    }

    public class WeaponEventHandler {
        event WeaponEvent ev;
        public void Subscribe(WeaponEvent action) { ev += action; }
        public void Unsubscribe(WeaponEvent action) { ev -= action; }
        public void Invoke(WeaponType weaponType) { if (ev != null) ev.Invoke(weaponType); }
    }

    public class DialogueEventHandler {
        event DialogueEvent ev;
        public void Subscribe(DialogueEvent action) { ev += action; }
        public void Unsubscribe(DialogueEvent action) { ev -= action; }
        public void Invoke(DialogueItemSO dialogueItem) { if (ev != null) ev.Invoke(dialogueItem); }
    }

    public class HintEventHandler {
        event HintEvent ev;
        public void Subscribe(HintEvent action) { ev += action; }
        public void Unsubscribe(HintEvent action) { ev -= action; }
        public void Invoke(HintSO hint, string currentControlScheme = "Keyboard&Mouse") { if (ev != null) ev.Invoke(hint, currentControlScheme); }
    }

    public class HighScoreSubmitEventHandler {
        event HighScoreSubmitEvent ev;
        public void Subscribe(HighScoreSubmitEvent action) { ev += action; }
        public void Unsubscribe(HighScoreSubmitEvent action) { ev -= action; }
        public void Invoke(string name, int score) { if (ev != null) ev.Invoke(name, score); }
    }

    public class HighScoresFetchEventHandler {
        event HighScoresFetchEvent ev;
        public void Subscribe(HighScoresFetchEvent action) { ev += action; }
        public void Unsubscribe(HighScoresFetchEvent action) { ev -= action; }
        public void Invoke(HighScore[] highScores) { if (ev != null) ev.Invoke(highScores); }
    }

    [CreateAssetMenu(fileName = "EventChannel", menuName = "ScriptableObjects/EventChannel")]
    public class EventChannelSO : ScriptableObject {

        public BoolEventHandler OnWinLevel = new BoolEventHandler();
        public VoidEventHandler OnPlayerDeath = new VoidEventHandler();
        public VoidEventHandler OnSpawnXtraLife = new VoidEventHandler();

        public FloatEventHandler OnPlayerTakeHealth = new FloatEventHandler();
        public FloatEventHandler OnPlayerTakeMoney = new FloatEventHandler();
        public FloatEventHandler OnScorePoints = new FloatEventHandler();
        public VoidEventHandler OnXtraLife = new VoidEventHandler();
        public StringEventHandler OnSubmitName = new StringEventHandler();
        public VoidEventHandler OnSkipName = new VoidEventHandler();
        public HighScoreSubmitEventHandler OnSubmitHighScore = new HighScoreSubmitEventHandler();
        public HighScoresFetchEventHandler OnFetchHighScores = new HighScoresFetchEventHandler();

        public WeaponAmmoEventHandler OnTakeAmmo = new WeaponAmmoEventHandler();
        public WeaponEventHandler OnUpgradeWeapon = new WeaponEventHandler();
        public WeaponEventHandler OnOutOfAmmo = new WeaponEventHandler();

        public VoidEventHandler OnBattleTriggerCrossed = new VoidEventHandler();
        public VoidEventHandler OnDestroyAllEnemies = new VoidEventHandler();
        public EnemyDeathEventHandler OnEnemyDeath = new EnemyDeathEventHandler();
        public VoidEventHandler OnEnemySpawn = new VoidEventHandler();
        public BossSpawnEventHandler OnBossSpawn = new BossSpawnEventHandler();

        public VoidEventHandler OnGotoMainMenu = new VoidEventHandler();
        public VoidEventHandler OnPause = new VoidEventHandler();
        public VoidEventHandler OnUnpause = new VoidEventHandler();
        public VoidEventHandler OnShowDebug = new VoidEventHandler();
        public VoidEventHandler OnHideDebug = new VoidEventHandler();
        public VoidEventHandler OnShowVictory = new VoidEventHandler();
        public VoidEventHandler OnHideVictory = new VoidEventHandler();
        public VoidEventHandler OnShowUpgradePanel = new VoidEventHandler();

        public DialogueEventHandler OnShowDialogue = new DialogueEventHandler();
        public VoidEventHandler OnDismissDialogue = new VoidEventHandler();
        public VoidEventHandler OnAnyKeyPress = new VoidEventHandler();

        public HintEventHandler OnShowHint = new HintEventHandler();

        public VoidEventHandler OnDisableAllSpawners = new VoidEventHandler();
    }
}

