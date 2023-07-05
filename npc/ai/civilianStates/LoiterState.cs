using System.Linq;
using AI;
using UnityEngine;
using UnityEngine.AI;
public class LoiterState : CivilianNPCControlState {
    public static readonly string NAV_POINT_KEY = "nav_point_key";
    private TaskNode rootTaskNode;
    CivilianNPCAI ai;
    CharacterController characterController;
    public LoiterState(CivilianNPCAI ai, CharacterController characterController) : base(ai) {
        this.ai = ai;
        this.characterController = characterController;
    }
    public override void Enter() {
        base.Enter();
        SetupRootNode();
    }
    void SetupRootNode() {
        Vector3 destination = GetDestination();
        float speedCoefficient = Random.Range(0.3f, 0.75f);
        rootTaskNode =
        new Sequence(
         new TaskMoveToKey(owner.transform, NAV_POINT_KEY, new System.Collections.Generic.HashSet<int>(), characterController, arrivalDistance: 0.5f) {
             speedCoefficient = speedCoefficient
         },
         new TaskTimerDectorator(Random.Range(1f, 3f))
        );
        rootTaskNode.SetData(NAV_POINT_KEY, destination);
        // rootTaskNode = new TaskPatrol(owner.transform, patrolRoute, patrolType, owner.physicalKeys, characterController);
    }

    Vector3 GetDestination() {
        GameObject destinationContainer = Toolbox.RandomFromList(GameObject.FindGameObjectsWithTag("loiter"));
        BoxCollider area = destinationContainer.GetComponent<BoxCollider>();
        return Toolbox.RandomInsideBounds(area);
    }

    public override PlayerInput Update(ref PlayerInput input) {
        TaskState result = rootTaskNode.Evaluate(ref input);
        if (result == TaskState.success) {
            owner.StateFinished(this);
        }
        return input;
    }

}