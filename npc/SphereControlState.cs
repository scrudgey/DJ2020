using UnityEngine;

public abstract class SphereControlState : IState {
    protected SphereRobotAI owner;
    public SphereControlState(SphereRobotAI handler) {
        this.owner = handler;
        // this.slewTime = 1f;
    }
    public abstract PlayerInput getInput();
    public virtual void Enter() { }
    public virtual void Update() {
        // if (slewTime > 0) {
        //     slewTime -= Time.deltaTime;
        // }
    }
    public virtual void Exit() { }
    public virtual void OnObjectPerceived(Collider other) { }
    public virtual void OnNoiseHeard(NoiseComponent noise) { }
    // protected float slewTime;
}