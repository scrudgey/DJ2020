using AI;
using UnityEngine;

[System.Serializable]
public abstract class CivilianNPCControlState : AI.IState {
    protected CivilianNPCAI owner;
    public CivilianNPCControlState(CivilianNPCAI handler) {
        this.owner = handler;
    }
    public abstract PlayerInput Update(ref PlayerInput input);
    public virtual void Enter() { }
    public virtual void Exit() { }
    public TaskNode rootTaskNote { get; set; }
}