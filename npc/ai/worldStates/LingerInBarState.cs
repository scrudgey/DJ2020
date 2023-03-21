using AI;
using UnityEngine;
using UnityEngine.AI;
public class LingerInBarState : WorldNPCControlState {
    public static readonly string NAV_POINT_KEY = "nav_point_key";
    private TaskNode rootTaskNode;
    WorldNPCAI ai;
    CharacterController characterController;
    public LingerInBarState(WorldNPCAI ai, CharacterController characterController) : base(ai) {
        this.ai = ai;
        this.characterController = characterController;
    }
    public override void Enter() {
        base.Enter();
        SetupRootNode();
    }
    void SetupRootNode() {
        Vector3 destination = GetDestination();
        rootTaskNode =
            new Sequence(
                new TaskMoveToKey(owner.transform, NAV_POINT_KEY, new System.Collections.Generic.HashSet<int>(), characterController, arrivalDistance: 2f) {
                    speedCoefficient = 0.25f
                },
                new TaskTimerDectorator(Random.Range(3f, 10f))
            );
        rootTaskNode.SetData(NAV_POINT_KEY, destination);
    }

    Vector3 GetDestination() {
        GameObject barObject = GameObject.Find("barDestination");
        BoxCollider area = barObject.GetComponent<BoxCollider>();
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