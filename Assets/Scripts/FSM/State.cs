using System.Collections.Generic;
using UnityEngine;

namespace FSM
{
    [CreateAssetMenu(menuName = "FSM/State")]
    public sealed class State : BaseState
    {
        public List<FSMAction> Action = new List<FSMAction>();
        public List<Transition> Transitions = new List<Transition>();

        public override void Execute(BaseMachine machine)
        {
            foreach (var action in Action)
                action.Execute(machine);

            foreach(var transition in Transitions)
                transition.Execute(machine);
        }

        protected override void OnBegin(BaseMachine machine) {
            foreach (var action in Action)
                action.OnBegin(machine);
        }

        protected override void OnEnd(BaseMachine machine) {
            foreach (var action in Action)
                action.OnEnd(machine);
        }
    }
}