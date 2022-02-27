using UnityEngine;
using System.Collections.Generic;
// using UnityEngine.AI;

using Enemies;

namespace FSM
{

    namespace Actions
    {

        namespace Enemy
        {
            
            [CreateAssetMenu(menuName = "FSM/Actions/Enemy/FollowPath")]
            public class FollowPath : FSMAction
            {
                public override void Execute (FiniteStateMachine machine) {
                    // empty execution; path follow implementation in Pathfollower
                }

                public override void OnBegin(FiniteStateMachine machine) {
                    // note - path is set and initialized by EnemySpawner via WaveConfig
                    var pathFollower = machine.GetComponent<Pathfollower>();
                    if (pathFollower == null) return;
                    if (!pathFollower.isStarted) {
                        pathFollower.Begin();
                    } else {
                        pathFollower.Resume();
                    }
                }
                public override void OnEnd(FiniteStateMachine machine) {}
            }
        }
    }
}
