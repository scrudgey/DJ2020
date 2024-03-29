using System.Collections.Generic;
using AI;
using UnityEngine;
using UnityEngine.AI;

public class CivilianReactToAttackState : CivilianNPCControlState {
    readonly static float ROUTINE_TIMEOUT = 60f;
    readonly static HashSet<int> myKeys = new HashSet<int>();
    static readonly public string DAMAGE_SOURCE_KEY = "damageSourceKey";
    static readonly public string COVER_POSITION_KEY = "coverPositionKey";
    public enum AttackType { none, damage, gunshots }
    AttackType type;
    float changeStateCountDown;
    private TaskNode rootTaskNode;
    private SpeechTextController speechTextController;
    CharacterController characterController;

    public CivilianReactToAttackState(CivilianNPCAI ai, SpeechTextController speechTextController, Damage damage, CharacterController characterController) : base(ai) {
        this.speechTextController = speechTextController;
        this.type = AttackType.damage;
        this.characterController = characterController;
        Vector3 damageSourcePosition = ai.transform.position + -10f * damage.direction;
        Vector3 coverPosition = ai.transform.position + 10f * damage.direction;
        float initialPause = Random.Range(0.5f, 2f);
        SetupRootNode(initialPause: initialPause); // enough to time out hitstun
        rootTaskNode.SetData(DAMAGE_SOURCE_KEY, damageSourcePosition);
        rootTaskNode.SetData(COVER_POSITION_KEY, coverPosition);
    }
    public CivilianReactToAttackState(CivilianNPCAI ai, SpeechTextController speechTextController, NoiseComponent noise, CharacterController characterController) : base(ai) {
        this.speechTextController = speechTextController;
        this.type = AttackType.gunshots;
        this.characterController = characterController;
        Vector3 damageSourcePosition = noise.transform.position;
        Vector3 coverPosition = (noise.transform.position - ai.transform.position).normalized;// * -5f;
        coverPosition.y = ai.transform.position.y;
        coverPosition *= -5f;
        coverPosition += ai.transform.position;
        float initialPause = Random.Range(0.5f, 2f);
        SetupRootNode(initialPause: initialPause);
        rootTaskNode.SetData(DAMAGE_SOURCE_KEY, damageSourcePosition);
        rootTaskNode.SetData(COVER_POSITION_KEY, coverPosition);
    }

    public override void Enter() {
        changeStateCountDown = ROUTINE_TIMEOUT;
        owner.navMeshPath = new NavMeshPath();
    }

    void SetupRootNode(float initialPause = 1f) {
        rootTaskNode = new Sequence(
                new TaskTimerDectorator(new TaskLookAt(owner.transform) {
                    lookType = TaskLookAt.LookType.position,
                    key = DAMAGE_SOURCE_KEY,
                    useKey = true,
                    reorient = true
                }, initialPause),
                new TaskMoveToKey(owner.transform, COVER_POSITION_KEY, myKeys, characterController) {
                    headBehavior = TaskMoveToKey.HeadBehavior.search,
                    speedCoefficient = 2f
                },
                new TaskTimerDectorator(new TaskLookAt(owner.transform) {
                    lookType = TaskLookAt.LookType.position,
                    key = DAMAGE_SOURCE_KEY,
                    useKey = true,
                    reorient = true
                }, 1.5f)
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
        } else if (result == TaskState.failure) {
            owner.StateFinished(this);
        }
        if (input.moveDirection.magnitude > 0.1) {
            input.armsRaised = true;
        }
        return input;
    }
}