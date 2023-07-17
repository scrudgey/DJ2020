using AI;
using UnityEngine;
using UnityEngine.AI;


public class FollowTheLeaderState : SphereControlState {
    static readonly public string SEARCH_POSITION_KEY = "investigatePosition";
    private TaskNode rootTaskNode;
    public TaskFollowTarget.HeadBehavior headBehavior;
    CharacterController characterController;
    public FollowTheLeaderState(SphereRobotAI ai, GameObject leader, CharacterController characterController, TaskFollowTarget.HeadBehavior headBehavior = TaskFollowTarget.HeadBehavior.normal) : base(ai) {
        this.headBehavior = headBehavior;
        this.characterController = characterController;
        SetupRootNode(leader);
    }

    public override void Enter() {
        owner.navMeshPath = new NavMeshPath();
    }

    void SetupRootNode(GameObject leader) {
        rootTaskNode = new TaskFollowTarget(owner.transform, leader, owner.physicalKeys, characterController) {
            headBehavior = headBehavior
        };
    }

    public override PlayerInput Update(ref PlayerInput input) {
        TaskState result = rootTaskNode.Evaluate(ref input);
        if (result == TaskState.success) {
            owner.StateFinished(this, result);
        }
        return input;
    }
}