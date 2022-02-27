using UnityEngine;

using Enemies;

namespace FSM
{
    namespace Enemy {

        [CreateAssetMenu(menuName = "FSM/Decisions/Enemy/IsPathFollowComplete")]
        public class IsPathFollowComplete : Decision
        {
            public override bool Decide(BaseMachine stateMachine)
            {
                var pathFollower = stateMachine.GetComponent<Pathfollower>();
                if (pathFollower == null) return true;

                // can add other interrupts here
                // pathFollower.Halt();

                return pathFollower.isPathComplete;
            }
        }
    }
}