using AI;
using UnityEngine;
using UnityEngine.AI;
public class SocializeState : WorldNPCControlState {
    public static readonly string NAV_POINT_KEY = "nav_point_key";
    private TaskNode rootTaskNode;
    WorldNPCAI ai;
    CharacterController characterController;
    SocialGroup socialGroup;
    public WalkToStoreState.StoreType location;
    public SocializeState(WorldNPCAI ai, CharacterController characterController, SocialGroup socialGroup, WalkToStoreState.StoreType location) : base(ai) {
        this.ai = ai;
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
    }
    void SetupRootNode() {
        rootTaskNode =
            new Sequence(
                new TaskMoveToKey(owner.transform, NAV_POINT_KEY, new System.Collections.Generic.HashSet<int>(), characterController, arrivalDistance: 1.5f) {
                    speedCoefficient = 0.25f
                },
                new TaskTimerDectorator(new TaskLookAt(socialGroup.transform) {
                    reorient = true
                }, 0.2f),
                new TaskTimerDectorator(Random.Range(10f, 20f))
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