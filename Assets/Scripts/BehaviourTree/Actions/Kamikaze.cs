using TheKiwiCoder;

public class Kamikaze : ActionNode {
    protected override void OnStart() {
        if (context.movement == null) return;
        context.movement.SetKamikaze(true);
    }

    protected override void OnStop() { }

    protected override State OnUpdate() {
        if (context.movement == null) return State.Failure;
        if (context.player == null || !context.player.isAlive) return State.Failure;
        return State.Running;
    }
}
