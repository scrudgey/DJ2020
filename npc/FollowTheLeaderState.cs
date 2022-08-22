using AI;
using UnityEngine;
using UnityEngine.AI;


public class FollowTheLeaderState : SphereControlState {
    static readonly public string SEARCH_POSITION_KEY = "investigatePosition";
    private TaskNode rootTaskNode;
    public TaskFollowTarget.HeadBehavior headBehavior;
    public FollowTheLeaderState(SphereRobotAI ai, GameObject leader, TaskFollowTarget.HeadBehavior headBehavior = TaskFollowTarget.HeadBehavior.normal) : base(ai) {
        this.headBehavior = headBehavior;
        SetupRootNode(leader);
    }

    public override void Enter() {
        owner.navMeshPath = new NavMeshPath();
    }

    void SetupRootNode(GameObject leader) {
        rootTaskNode = new TaskFollowTarget(owner.transform, leader) {
            headBehavior = headBehavior
        };
    }

    public override PlayerInput Update(ref PlayerInput input) {
        TaskState result = rootTaskNode.Evaluate(ref input);
        if (result == TaskState.success) {
            owner.StateFinished(this);
        }
        return input;
    }
}