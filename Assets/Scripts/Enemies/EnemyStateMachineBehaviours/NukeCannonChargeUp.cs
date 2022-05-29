using UnityEngine;

namespace Enemies {

    public class NukeCannonChargeUp : StateMachineBehaviour {
        [SerializeField] AnimationCurve animSpeed = AnimationCurve.Linear(0f, 1f, 1f, 1f);
        [SerializeField] float lifetime = 2f;

        EnemyShooter shooter;
        float t = 0f;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            shooter = animator.GetComponentInParent<EnemyShooter>();
            shooter.ChargeUp();
            t = 0f;
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            animator.speed = animSpeed.Evaluate(t / lifetime);
            t += Time.deltaTime;
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    
        //}

        // OnStateMove is called right after Animator.OnAnimatorMove()
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that processes and affects root motion
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK()
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that sets up animation IK (inverse kinematics)
        //}
    }
}

