using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapons;

namespace Event {
    // delegate types
    public delegate void VoidEvent();
    public delegate void IntEvent(int value);
    public delegate void FloatEvent(float value);
    // enemy-specific
    public delegate void EnemyDeathEvent(int instanceId, int points);
    // weapon-specific
    public delegate void WeaponAmmoEvent(WeaponType weaponType, int value);
    public delegate void WeaponUpgradeEvent(WeaponType weaponType);

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
    }
}
