using UnityEngine;

namespace Enemies {

    public class MinespawnerBaseStateMachineBehaviour : StateMachineBehaviour {

        protected MineSpawner mineSpawner;

        protected void FindAndSetMinespawnerComponent(Animator animator) {
            if (mineSpawner != null) return;
            mineSpawner = animator.GetComponentInParent<MineSpawner>();
        }
    }
}
