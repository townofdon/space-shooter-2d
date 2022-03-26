
using UnityEngine;

using Core;

namespace Player {
    public class PlayerUtils {

        static GameObject cachedPlayerGO;
        static PlayerGeneral cachedPlayerGeneral;
        static Timer cacheBustInterval = new Timer();

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
            cachedPlayerGeneral = cachedPlayerGO.GetComponent<PlayerGeneral>();
            return cachedPlayerGeneral;
        }
    }
}
