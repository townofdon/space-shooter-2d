using UnityEngine;

namespace FSM
{
    public abstract class FSMAction : ScriptableObject
    {
        public abstract void Execute(BaseMachine stateMachine);

        public abstract void OnBegin(BaseMachine stateMachine);

        public abstract void OnEnd(BaseMachine stateMachine);
    }
}