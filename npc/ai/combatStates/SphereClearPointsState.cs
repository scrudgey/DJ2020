using System.Linq;
using AI;
using UnityEngine;
using UnityEngine.AI;
public class SphereClearPointsState : SphereControlState {
    public static readonly string NAV_POINT_KEY = "nav_point_key";

    private TaskNode rootTaskNode;
    SphereRobotAI ai;
    CharacterController characterController;
    ClearPoint targetPoint;
    ClearPoint[] allClearPoints;
    public SphereClearPointsState(SphereRobotAI ai, CharacterController characterController, ClearPoint[] allClearPoints) : base(ai) {
        this.ai = ai;
        this.characterController = characterController;
        this.allClearPoints = allClearPoints;
    }
    public override void Enter() {
        base.Enter();
        SetupRootNode();
    }
    void SetupRootNode() {
        targetPoint = GetClosestUncheckedClearpoint();
        Debug.Log($"Target point: {targetPoint}");
        rootTaskNode = new TaskUntilFailRepeater(new Sequence(
            new TaskConditional(() => targetPoint != null),
            new TaskMoveToKey(owner.transform, NAV_POINT_KEY, owner.physicalKeys, characterController) {
                headBehavior = TaskMoveToKey.HeadBehavior.search,
                speedCoefficient = 1f
            },
            new TaskLambda(() => {
                Debug.Log($"target point {targetPoint} cleared");
                targetPoint.cleared = true;
                targetPoint = GetClosestUncheckedClearpoint();
                rootTaskNode.SetData(NAV_POINT_KEY, targetPoint.transform.position);
            })
        ));
        rootTaskNode.SetData(NAV_POINT_KEY, targetPoint.transform.position);
    }

    public override PlayerInput Update(ref PlayerInput input) {
        TaskState result = rootTaskNode.Evaluate(ref input);
        if (result == TaskState.success || result == TaskState.failure) {
            owner.StateFinished(this);
        }
        return input;
    }
    ClearPoint GetClosestUncheckedClearpoint() {
        return allClearPoints
                .Where(point => !point.cleared)
                .OrderByDescending(point => Vector3.Distance(point.transform.position, owner.transform.position))
                .FirstOrDefault();
    }
}