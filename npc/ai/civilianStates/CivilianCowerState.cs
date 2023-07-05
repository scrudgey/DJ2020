using System.Collections.Generic;
using AI;
using UnityEngine;
using UnityEngine.AI;

public class CivilianCowerState : CivilianNPCControlState {
    readonly float ROUTINE_TIMEOUT = 60f;
    float changeStateCountDown;
    private TaskNode rootTaskNode;
    CharacterController characterController;

    public CivilianCowerState(CivilianNPCAI ai, CharacterController characterController) : base(ai) {
        this.characterController = characterController;
        SetupRootNode(); // enough to time out hitstun
    }

    public override void Enter() {
        // changeStateCountDown = ROUTINE_TIMEOUT;
        changeStateCountDown = Random.Range(3f, 10f);
        owner.navMeshPath = new NavMeshPath();
    }

    void SetupRootNode() {
        rootTaskNode = new TaskCower();
    }

    public override PlayerInput Update(ref PlayerInput input) {
        changeStateCountDown -= Time.deltaTime;
        if (changeStateCountDown <= 0) {
            owner.StateFinished(this);
        }
        TaskState result = rootTaskNode.Evaluate(ref input);
        if (result == TaskState.success) {
            owner.StateFinished(this);
        } else if (result == TaskState.failure) {
            owner.StateFinished(this);
        }
        return input;
    }
}