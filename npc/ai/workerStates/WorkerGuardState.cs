using System.Collections.Generic;
using AI;
using UnityEngine;
using UnityEngine.AI;

public class WorkerGuardState : WorkerNPCControlState {
    static readonly public string GUARD_POSITION_KEY = "guardPositionKey";
    static readonly public string LOOK_POSITION_KEY = "lookPositionKey";
    private TaskNode rootTaskNode;
    CharacterController characterController;
    HashSet<int> myKeys = new HashSet<int>();

    public WorkerGuardState(WorkerNPCAI ai, CharacterController characterController, Vector3 guardPosition, Vector3 lookPosition) : base(ai) {
        this.characterController = characterController;
        SetupRootNode();
        rootTaskNode.SetData(GUARD_POSITION_KEY, guardPosition);
        rootTaskNode.SetData(LOOK_POSITION_KEY, lookPosition);
    }

    public override void Enter() {
        owner.navMeshPath = new NavMeshPath();
    }

    void SetupRootNode() {
        LevelState levelData = GameManager.I.gameData.levelState;
        rootTaskNode = new Sequence(
                new TaskMoveToKey(owner.transform, GUARD_POSITION_KEY, myKeys, characterController) {
                    headBehavior = TaskMoveToKey.HeadBehavior.search,
                    speedCoefficient = 2f
                },
                new TaskLookAt(owner.transform) {
                    lookType = TaskLookAt.LookType.position,
                    key = LOOK_POSITION_KEY,
                    useKey = true,
                    reorient = true
                }
            );
    }

    public override PlayerInput Update(ref PlayerInput input) {
        TaskState result = rootTaskNode.Evaluate(ref input);
        return input;
    }
}