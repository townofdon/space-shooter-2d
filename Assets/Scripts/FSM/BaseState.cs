using UnityEngine;

namespace FSM
{
    public class BaseState : ScriptableObject
    {
        protected bool started = false;
        protected bool running = false;

        public void Begin(FiniteStateMachine machine) {
            started = true;
            running = true;
            OnBegin(machine);
        }

        public void End(FiniteStateMachine machine) {
            running = false;
            OnEnd(machine);
        }

        public virtual void Execute(FiniteStateMachine machine) {}
        protected virtual void OnBegin(FiniteStateMachine machine) {}
        protected virtual void OnEnd(FiniteStateMachine machine) {}
    }
}
