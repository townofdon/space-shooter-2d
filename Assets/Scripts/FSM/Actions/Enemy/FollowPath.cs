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
                public override void Execute (BaseMachine stateMachine) {
                    // empty execution; path follow implementation in Pathfollower
                }

                public override void OnBegin(BaseMachine stateMachine) {
                    // note - path is set and initialized by EnemySpawner via WaveConfig
                    var pathFollower = stateMachine.GetComponent<Pathfollower>();
                    if (pathFollower == null) return;
                    if (!pathFollower.isStarted) {
                        pathFollower.Begin();
                    } else {
                        pathFollower.Resume();
                    }
                }
                public override void OnEnd(BaseMachine stateMachine) {}

                // [SerializeField] Transform path;

                // public override void Execute(BaseStateMachine stateMachine)
                // {
                //     var pathFollower = stateMachine.GetComponent<Pathfollower>();
                //     if (pathFollower == null) return;

                //     if (!pathFollower.isStarted) {
                //         pathFollower.SetWaypoints(GetWaypoints());
                //         pathFollower.Init();
                //     }
                //     // spawn && set position from first waypoint - separate action?
                //     // set path for PathFollower component

                //     // var navMeshAgent = stateMachine.GetComponent<NavMeshAgent>();
                //     // var patrolPoints = stateMachine.GetComponent<PatrolPoints>();

                //     // if (patrolPoints.HasReached(navMeshAgent))
                //     //     navMeshAgent.SetDestination(patrolPoints.GetNext().position);
                // }

                // public Transform GetStartingWaypoint() {
                //     return path.GetChild(0);
                // }

                // public List<Transform> GetWaypoints() {
                //     var waypoints = new List<Transform>();
                //     foreach (Transform child in path)
                //     {
                //         waypoints.Add(child);
                //     }
                //     return waypoints;
                // }
            }
        }
    }
}
