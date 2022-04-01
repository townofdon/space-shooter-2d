
using UnityEngine;

using Weapons;
using Dialogue;

namespace Event {
    // delegate types
    public delegate void VoidEvent();
    public delegate void IntEvent(int value);
    public delegate void FloatEvent(float value);
    // enemy-specific
    public delegate void EnemyDeathEvent(int instanceId, int points);
    public delegate void BossSpawnEvent(int instanceId);
    // weapon-specific
    public delegate void WeaponAmmoEvent(WeaponType weaponType, int value);
    public delegate void WeaponUpgradeEvent(WeaponType weaponType);
    // dialogue-specific
    public delegate void DialogueEvent(DialogueItemSO dialogueItem);
    public delegate void HintEvent(HintSO hint, string currentControlScheme);

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

    public class EnemyDeathEventHandler {
        event EnemyDeathEvent ev;
        public void Subscribe(EnemyDeathEvent action) { ev += action; }
        public void Unsubscribe(EnemyDeathEvent action) { ev -= action; }
        public void Invoke(int instanceId, int points) { if (ev != null) ev.Invoke(instanceId, points); }
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

    public class WeaponUpgradeEventHandler {
        event WeaponUpgradeEvent ev;
        public void Subscribe(WeaponUpgradeEvent action) { ev += action; }
        public void Unsubscribe(WeaponUpgradeEvent action) { ev -= action; }
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

    [CreateAssetMenu(fileName = "EventChannel", menuName = "ScriptableObjects/EventChannel")]
    public class EventChannelSO : ScriptableObject {

        public VoidEventHandler OnWinLevel = new VoidEventHandler();
        public VoidEventHandler OnPlayerDeath = new VoidEventHandler();

        public FloatEventHandler OnPlayerTakeHealth = new FloatEventHandler();
        public FloatEventHandler OnPlayerTakeMoney = new FloatEventHandler();
        public FloatEventHandler OnScorePoints = new FloatEventHandler();

        public WeaponAmmoEventHandler OnTakeAmmo = new WeaponAmmoEventHandler();
        public WeaponUpgradeEventHandler OnUpgradeWeapon = new WeaponUpgradeEventHandler();

        public EnemyDeathEventHandler OnEnemyDeath = new EnemyDeathEventHandler();
        public BossSpawnEventHandler OnBossSpawn = new BossSpawnEventHandler();

        public VoidEventHandler OnGotoMainMenu = new VoidEventHandler();
        public VoidEventHandler OnPause = new VoidEventHandler();
        public VoidEventHandler OnUnpause = new VoidEventHandler();
        public VoidEventHandler OnShowDebug = new VoidEventHandler();
        public VoidEventHandler OnHideDebug = new VoidEventHandler();

        public DialogueEventHandler OnShowDialogue = new DialogueEventHandler();
        public VoidEventHandler OnDismissDialogue = new VoidEventHandler();
        public VoidEventHandler OnAnyKeyPress = new VoidEventHandler();

        public HintEventHandler OnShowHint = new HintEventHandler();
    }
}

