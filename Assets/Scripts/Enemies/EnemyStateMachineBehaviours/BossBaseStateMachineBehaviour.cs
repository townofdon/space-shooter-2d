using UnityEngine;

namespace Enemies {

    public class BossBaseStateMachineBehaviour : StateMachineBehaviour {

        protected BossBeamLaserShooter shooter;

        protected void FindAndSetShooterComponent(Animator animator) {
            if (shooter != null) return;
            shooter = animator.GetComponentInParent<BossBeamLaserShooter>();
        }
    }
}
