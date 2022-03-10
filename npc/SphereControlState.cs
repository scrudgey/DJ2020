using UnityEngine;
public abstract class SphereControlState : IState {
    protected SphereRobotAI owner;
    public SphereControlState(SphereRobotAI handler) { this.owner = handler; }
    public abstract PlayerInput getInput();
    public virtual void Enter() { }
    public abstract void Update();
    public virtual void Exit() { }
    public virtual void OnObjectPerceived(Collider other) { }
}