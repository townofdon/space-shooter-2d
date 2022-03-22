using System.Collections;
using UnityEngine;

// TODO: REMOVE

namespace Enemies
{
    public abstract class BaseEnemyBehaviour {
        // protected enum State {
        //     Idle,
        //     Running,
        //     Finished,
        // }
        // protected State state = State.Idle;
        // protected bool started = false;

        public abstract IEnumerator Execute();

        // public virtual void OnStart() {
        //     started = true;
        //     state = State.Running;
        // }
        // public virtual void OnEnd() {
        //     state = State.Finished;
        // }
    }
}
