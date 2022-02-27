using UnityEngine;

namespace FSM
{
    [System.Serializable]
    public sealed class Transition
    {
        public Decision Decision;
        public BaseState TrueState;
        public BaseState FalseState;

        public void Execute(FiniteStateMachine machine)
        {
            if(Decision.Decide(machine) && !(TrueState is RemainInState))
                machine.SetState(TrueState);
            else if(!(FalseState is RemainInState))
                machine.SetState(FalseState);
        }
    }
}