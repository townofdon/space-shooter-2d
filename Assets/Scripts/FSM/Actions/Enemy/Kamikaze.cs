using UnityEngine;
// using UnityEngine.AI;

using Enemies;

namespace FSM
{

    namespace Actions
    {

        namespace Enemy
        {
            
            [CreateAssetMenu(menuName = "FSM/Actions/Enemy/Kamikaze")]
            public class Kamikaze : FSMAction
            {
                public override void Execute(BaseMachine stateMachine)
                {
                    // spawn && set position from first waypoint - separate action?
                    // set path for PathFollower component

                    // var navMeshAgent = stateMachine.GetComponent<NavMeshAgent>();
                    // var patrolPoints = stateMachine.GetComponent<PatrolPoints>();

                    // if (patrolPoints.HasReached(navMeshAgent))
                    //     navMeshAgent.SetDestination(patrolPoints.GetNext().position);
                }

                public override void OnBegin(BaseMachine stateMachine) {
                    var enemy = stateMachine.GetComponent<EnemyShip>();
                    var enemyMovement = stateMachine.GetComponent<EnemyMovement>();
                    enemyMovement.SetKamikaze(true);
                }
                public override void OnEnd(BaseMachine stateMachine) {}
            }
        }
    }
}
