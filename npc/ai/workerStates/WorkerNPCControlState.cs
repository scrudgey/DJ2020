using AI;
using UnityEngine;

[System.Serializable]
public abstract class WorkerNPCControlState : AI.IState {
    protected WorkerNPCAI owner;
    public WorkerNPCControlState(WorkerNPCAI handler) {
        this.owner = handler;
    }
    public abstract PlayerInput Update(ref PlayerInput input);
    public virtual void Enter() { }
    public virtual void Exit() { }
    public TaskNode rootTaskNote { get; set; }
}