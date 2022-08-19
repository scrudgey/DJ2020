using AI;
using UnityEngine;
using UnityEngine.AI;


public class ReactToAttackState : SphereControlState {
    static readonly public string DAMAGE_SOURCE_KEY = "damageSourceKey";
    static readonly public string COVER_POSITION_KEY = "coverPositionKey";
    readonly float ROUTINE_TIMEOUT = 5f;
    float changeStateCountDown;
    private TaskNode rootTaskNode;

    public ReactToAttackState(SphereRobotAI ai, Damage damage) : base(ai) {
        Vector3 damageSourcePosition = ai.transform.position + -10f * damage.direction;
        Vector3 coverPosition = ai.transform.position + 10f * damage.direction;
        SetupRootNode(damage, damageSourcePosition, coverPosition);
    }

    public override void Enter() {
        changeStateCountDown = ROUTINE_TIMEOUT;
        owner.navMeshPath = new NavMeshPath();
    }

    void SetupRootNode(Damage damage, Vector3 damageSourcePosition, Vector3 coverPosition) {
        rootTaskNode = new Sequence(
                    new TaskTimerDectorator(new TaskLookAt() {
                        lookType = TaskLookAt.LookType.position,
                        key = DAMAGE_SOURCE_KEY,
                        useKey = true
                    }, 0.1f),
                    new TaskMoveToKey(owner.transform, COVER_POSITION_KEY) {
                        headBehavior = TaskMoveToKey.HeadBehavior.search,
                        speedCoefficient = 2f
                    }
                );
        rootTaskNode.SetData(DAMAGE_SOURCE_KEY, damageSourcePosition);
        rootTaskNode.SetData(COVER_POSITION_KEY, coverPosition);
    }

    public override PlayerInput Update(ref PlayerInput input) {
        changeStateCountDown -= Time.deltaTime;
        if (changeStateCountDown <= 0) {
            owner.StateFinished(this);
        }
        rootTaskNode.Evaluate(ref input);
        return input;
    }

    public override void OnNoiseHeard(NoiseComponent noise) {
        base.OnNoiseHeard(noise);
        if (noise.data.player) {
            if (noise.data.suspiciousness > Suspiciousness.normal) {

            }
        }
    }
}