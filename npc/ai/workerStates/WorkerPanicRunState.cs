using System.Collections.Generic;
using AI;
using UnityEngine;
using UnityEngine.AI;
public class WorkerPanicRunState : WorkerNPCControlState {
    static readonly string SEARCH_POSITION_KEY = "investigatePosition";
    static readonly HashSet<int> physicalKeys = new HashSet<int>();
    float changeStateCountDown;
    private Vector3 searchDirection;
    private TaskNode rootTaskNode;
    CharacterController characterController;
    public WorkerPanicRunState(WorkerNPCAI ai, CharacterController characterController) : base(ai) {
        this.characterController = characterController;
        RandomSearchDirection();
        SetupRootNode();
        rootTaskNode.SetData(SEARCH_POSITION_KEY, searchDirection);
    }
    void RandomSearchDirection() {
        searchDirection = Random.insideUnitSphere;
        searchDirection.y = 0;
        searchDirection = searchDirection.normalized * Random.Range(5f, 10f) + characterController.transform.position;
    }

    public override void Enter() {
        changeStateCountDown = Random.Range(60f, 120f);
        owner.navMeshPath = new NavMeshPath();
    }
    void SetupRootNode() {
        rootTaskNode = new Sequence(
            new TaskMoveToKey(owner.transform, SEARCH_POSITION_KEY, physicalKeys, characterController, arrivalDistance: 2f) {
                headBehavior = TaskMoveToKey.HeadBehavior.search,
                speedCoefficient = 2f
            },
            new TaskTimerDectorator(new TaskLookAt(owner.transform) {
                lookType = TaskLookAt.LookType.position,
                key = SEARCH_POSITION_KEY,
                useKey = true,
                headBehavior = TaskLookAt.HeadBehavior.search
            }, Random.Range(0f, 3f))
        );
    }

    public override PlayerInput Update(ref PlayerInput input) {
        changeStateCountDown -= Time.deltaTime;
        if (changeStateCountDown <= 0) {
            owner.StateFinished(this);
        }
        TaskState result = rootTaskNode.Evaluate(ref input);
        if (result == TaskState.success) {
            owner.StateFinished(this);
        }
        input.armsRaised = true;
        return input;
    }
}