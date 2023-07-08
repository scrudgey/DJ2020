using System.Collections.Generic;
using AI;
using UnityEngine;
using UnityEngine.AI;
public class WorkerHeldAtGunpointState : WorkerNPCControlState {
    static readonly string PLAYER_POSITION_KEY = "playerPosition";
    private TaskNode rootTaskNode;
    CharacterController characterController;
    GameObject playerObject;
    GunHandler playerGunHandler;
    float changeStateImpulse = 0;
    float timeSinceISawPlayer = 0;
    bool playerHasGunOut;
    readonly static float CHANGE_STATE_THRESHOLD = 1f;
    public WorkerHeldAtGunpointState(WorkerNPCAI ai, CharacterController characterController, GameObject playerObject) : base(ai) {
        this.characterController = characterController;
        this.playerObject = playerObject;
        this.playerGunHandler = playerObject.GetComponentInChildren<GunHandler>();
        SetupRootNode();
    }

    void SetupRootNode() {
        rootTaskNode = new Sequence(
            new TaskConditional(() => timeSinceISawPlayer < 1f && playerHasGunOut),
            new TaskLookAt(owner.transform) {
                lookType = TaskLookAt.LookType.position,
                key = PLAYER_POSITION_KEY,
                useKey = true,
                reorient = true
            }
        );
    }

    public override PlayerInput Update(ref PlayerInput input) {
        rootTaskNode.SetData(PLAYER_POSITION_KEY, playerObject.transform.position);
        TaskState result = rootTaskNode.Evaluate(ref input);
        if (result == TaskState.failure) {
            changeStateImpulse += Time.deltaTime;
        } else if (changeStateImpulse > 0) {
            changeStateImpulse -= Time.deltaTime;
        }
        if (changeStateImpulse >= CHANGE_STATE_THRESHOLD) {
            owner.StateFinished(this);
        }
        timeSinceISawPlayer += Time.deltaTime;
        input.armsRaised = true;
        return input;
    }

    public override void OnPlayerPerceived() {
        base.OnPlayerPerceived();
        timeSinceISawPlayer = 0;
        playerHasGunOut = GameManager.I.GetTotalSuspicion() >= Suspiciousness.suspicious;
    }
}