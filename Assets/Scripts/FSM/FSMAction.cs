using UnityEngine;

namespace FSM
{
    public abstract class FSMAction : ScriptableObject
    {
        public abstract void Execute(FiniteStateMachine machine);

        public abstract void OnBegin(FiniteStateMachine machine);

        public abstract void OnEnd(FiniteStateMachine machine);
    }
}