using AI;
using UnityEngine;
using UnityEngine.AI;
public class SocializeState : WorldNPCControlState {
    public static readonly string NAV_POINT_KEY = "nav_point_key";
    private TaskNode rootTaskNode;
    WorldNPCAI ai;
    CharacterController characterController;
    SocialGroup socialGroup;
    SpeechTextController speechTextController;
    public WalkToStoreState.StoreType location;
    public SocializeState(WorldNPCAI ai,
            CharacterController characterController,
            SpeechTextController speechTextController,
            SocialGroup socialGroup,
            WalkToStoreState.StoreType location) : base(ai) {
        this.ai = ai;
        this.speechTextController = speechTextController;
        this.characterController = characterController;
        this.socialGroup = socialGroup;
        this.location = location;
    }
    public override void Enter() {
        base.Enter();
        socialGroup.AddMember(ai);
        SetupRootNode();
    }
    public override void Exit() {
        base.Exit();
        socialGroup.RemoveMember(ai);
        speechTextController.HideText();
    }
    void SetupRootNode() {
        float speedCoefficient = Random.Range(0.22f, 0.27f);
        float arrivalDistance = Random.Range(5f, 10f);
        rootTaskNode =
            new Sequence(
                new TaskMoveToKey(owner.transform, NAV_POINT_KEY, new System.Collections.Generic.HashSet<int>(), characterController, arrivalDistance: arrivalDistance) {
                    speedCoefficient = speedCoefficient
                },
                new TaskTimerDectorator(new TaskLookAt(ai.transform) {
                    lookType = TaskLookAt.LookType.position,
                    lookAtPoint = socialGroup.transform.position,
                    reorient = true,
                    useKey = false
                }, 0.2f),
                new TaskTimerDectorator(new TaskSocialize(ai, speechTextController, socialGroup), Random.Range(10f, 45f))
            );
        rootTaskNode.SetData(NAV_POINT_KEY, socialGroup.transform.position);
    }
    public override PlayerInput Update(ref PlayerInput input) {
        TaskState result = rootTaskNode.Evaluate(ref input);
        if (result == TaskState.success) {
            owner.StateFinished(this);
        }
        return input;
    }

}