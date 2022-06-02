
using UnityEngine;

using Game;
using Event;
using UI;

namespace Damage {

    public class ScorePointsOnDeath : MonoBehaviour {

        [SerializeField][Range(0, 10000)] int pointsWhenKilledByPlayer = 50;
        [SerializeField] GameObject pointsToastPrefab;
        [SerializeField] bool showPointsToast = true;

        [Space]

        [SerializeField] EventChannelSO eventChannel;

        DamageableBehaviour damageableBehaviour;

        void OnEnable() {
            if (damageableBehaviour == null) return;
            damageableBehaviour.OnDeathEvent += OnDeath;
        }

        void OnDisable() {
            if (damageableBehaviour == null) return;
            damageableBehaviour.OnDeathEvent -= OnDeath;
        }

        void Awake() {
            damageableBehaviour = GetComponent<DamageableBehaviour>();
        }

        void OnDeath(DamageType damageType, bool isDamageByPlayer) {
            if (!isDamageByPlayer || damageType == DamageType.InstakillQuiet) return;
            if (pointsWhenKilledByPlayer <= 0) return;

            int points = (int)(pointsWhenKilledByPlayer * GameUtils.GetPointsMod());
            eventChannel.OnEnemyDeath.Invoke(0, points, false);
            SpawnPointsToast(points);
        }

        void SpawnPointsToast(int points = 0) {
            if (!showPointsToast) return;
            if (pointsToastPrefab == null) return;
            if (points <= 0) return;
            GameObject instance = Instantiate(pointsToastPrefab, transform.position, Quaternion.identity);
            PointsToast pointsToast = instance.GetComponent<PointsToast>();
            pointsToast.SetPoints(points);
        }
    }
}
