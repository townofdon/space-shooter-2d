
using UnityEngine;

using Core;

namespace Player {
    public class PlayerUtils {

        static GameObject cachedPlayerGO;
        static PlayerGeneral cachedPlayerGeneral;
        static Timer cacheBustInterval = new Timer(TimerDirection.Decrement, TimerStep.DeltaTime, 0.2f);

        public static PlayerGeneral FindPlayer() {
            if (cachedPlayerGeneral != null && cachedPlayerGeneral.isAlive && cachedPlayerGeneral.isActiveAndEnabled && cachedPlayerGeneral.gameObject.activeSelf) {
                return cachedPlayerGeneral;
            }
            if (cacheBustInterval.active) {
                cacheBustInterval.Tick();
                return null;
            }
            cacheBustInterval.Start();
            cachedPlayerGO = GameObject.FindGameObjectWithTag(UTag.Player);
            if (cachedPlayerGO == null) return null;
            cachedPlayerGeneral = cachedPlayerGO.GetComponentInParent<PlayerGeneral>();
            return cachedPlayerGeneral;
        }

        public static void InvalidateCache() {
            cacheBustInterval.End();
        }
    }
}
