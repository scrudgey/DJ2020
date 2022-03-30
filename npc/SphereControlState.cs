using UnityEngine;

public abstract class SphereControlState : IState {
    protected SphereRobotAI owner;
    public SphereControlState(SphereRobotAI handler) {
        this.owner = handler;
    }
    public abstract PlayerInput Update();
    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void OnObjectPerceived(Collider other) { }
    public virtual void OnNoiseHeard(NoiseComponent noise) { }
}