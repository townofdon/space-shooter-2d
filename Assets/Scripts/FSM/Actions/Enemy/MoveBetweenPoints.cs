using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.AI;

using Enemies;

namespace FSM
{

    namespace Actions
    {

        namespace Enemy
        {

            [CreateAssetMenu(menuName = "FSM/Actions/Enemy/MoveBetweenPoints")]
            public class MoveBetweenPoints : FSMAction
            {
                [SerializeField] List<Transform> movePoints = new List<Transform>();

                public override void Execute(FiniteStateMachine machine)
                {
                    // spawn && set position from first waypoint - separate action?
                    // set path for PathFollower component

                    // var navMeshAgent = stateMachine.GetComponent<NavMeshAgent>();
                    // var patrolPoints = stateMachine.GetComponent<PatrolPoints>();

                    // if (patrolPoints.HasReached(navMeshAgent))
                    //     navMeshAgent.SetDestination(patrolPoints.GetNext().position);
                }

                public override void OnBegin(FiniteStateMachine machine)
                {
                    var enemy = machine.GetComponent<EnemyShip>();
                    var enemyMovement = machine.GetComponent<EnemyMovement>();

                    // 
                }

                public override void OnEnd(FiniteStateMachine machine) { }
            }
        }
    }
}


