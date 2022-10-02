using AI;
using UnityEngine;

[System.Serializable]
public abstract class SphereControlState : AI.IState {
    protected SphereRobotAI owner;
    public SphereControlState(SphereRobotAI handler) {
        this.owner = handler;
    }
    public abstract PlayerInput Update(ref PlayerInput input);
    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void OnObjectPerceived(Collider other) { }
    public virtual void OnNoiseHeard(NoiseComponent noise) { }
    public virtual void OnDamage(Damage damage) { }
    public TaskNode rootTaskNote { get; set; }
}