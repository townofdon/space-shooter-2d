using TheKiwiCoder;

public class FollowPath : ActionNode {
    protected override void OnStart() {
        // note - path is set and initialized by EnemySpawner via WaveConfig
        // var pathFollower = machine.GetComponent<Pathfollower>();
        if (context.pathFollower == null) return;
        if (!context.pathFollower.isStarted) {
            context.pathFollower.Begin();
        } else {
            context.pathFollower.Resume();
        }
    }

    protected override void OnStop() {
        if (context.pathFollower == null) return;
        context.pathFollower.Halt();
    }

    protected override State OnUpdate() {
        if (context.pathFollower == null) return State.Failure;
        if (!context.pathFollower.hasWaypoints) return State.Failure;
        if (context.pathFollower.isPathComplete) return State.Success;

        return State.Running;
    }
}
