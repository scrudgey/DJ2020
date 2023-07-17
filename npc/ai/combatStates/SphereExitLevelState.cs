using AI;
using UnityEngine;
using UnityEngine.AI;
public class SphereExitLevelState : SphereControlState {
    public static readonly string EXIT_POSITION_KEY = "exitPosition";
    private TaskNode rootTaskNode;
    CharacterController characterController;
    public SphereExitLevelState(SphereRobotAI ai, CharacterController characterController) : base(ai) {
        this.characterController = characterController;
    }

    public override void Enter() {
        base.Enter();
        SetupRootNode();
        ExtractionZone zone = Toolbox.RandomFromList(GameObject.FindObjectsOfType<ExtractionZone>());
        Vector3 point = Toolbox.RandomInsideBounds(zone.myCollider);
        rootTaskNode.SetData(EXIT_POSITION_KEY, point);
    }
    void SetupRootNode() {

        rootTaskNode = new Sequence(
            new TaskMoveToKey(owner.transform, EXIT_POSITION_KEY, owner.physicalKeys, characterController),
            new TaskTimerDectorator(1f)
        );
    }
    public override PlayerInput Update(ref PlayerInput input) {
        TaskState result = rootTaskNode.Evaluate(ref input);
        if (result == TaskState.success) {
            owner.StateFinished(this, result);
        }
        return input;
    }
}