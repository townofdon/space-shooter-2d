using UnityEngine;

namespace FSM
{
    public class BaseState : ScriptableObject
    {
        protected bool started = false;
        protected bool running = false;

        public void Begin(BaseMachine machine) {
            started = true;
            running = true;
            OnBegin(machine);
        }

        public void End(BaseMachine machine) {
            running = false;
            OnEnd(machine);
        }

        public virtual void Execute(BaseMachine machine) {}
        protected virtual void OnBegin(BaseMachine machine) {}
        protected virtual void OnEnd(BaseMachine machine) {}
    }
}
