using AI;
using UnityEngine;

[System.Serializable]
public abstract class WorldNPCControlState : AI.IState {
    protected WorldNPCAI owner;
    public WorldNPCControlState(WorldNPCAI handler) {
        this.owner = handler;
    }
    public abstract PlayerInput Update(ref PlayerInput input);
    public virtual void Enter() { }
    public virtual void Exit() { }
    public TaskNode rootTaskNote { get; set; }
}