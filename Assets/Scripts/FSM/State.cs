using System.Collections.Generic;
using UnityEngine;

namespace FSM
{
    [CreateAssetMenu(menuName = "FSM/State")]
    public sealed class State : BaseState
    {
        [SerializeField] List<FSMAction> Action = new List<FSMAction>();
        [SerializeField] List<Transition> Transitions = new List<Transition>();
        [SerializeField][TextArea(3, 20)] string notes;

        public override void Execute(FiniteStateMachine machine)
        {
            foreach (var action in Action)
                action.Execute(machine);

            foreach(var transition in Transitions)
                transition.Execute(machine);
        }

        protected override void OnBegin(FiniteStateMachine machine) {
            foreach (var action in Action)
                action.OnBegin(machine);
        }

        protected override void OnEnd(FiniteStateMachine machine) {
            foreach (var action in Action)
                action.OnEnd(machine);
        }
    }
}
