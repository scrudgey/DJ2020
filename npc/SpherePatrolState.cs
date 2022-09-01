using AI;
using UnityEngine;
using UnityEngine.AI;
public class SpherePatrolState : SphereControlState {
    private TaskNode rootTaskNode;
    private PatrolRoute patrolRoute;

    float waitTimer;
    float waitCheckTimer;

    float waitProbability = 0.1f;
    LoHi waitInterval = new LoHi(3f, 10f);
    float waitCheckInterval = 2.5f;
    public SpherePatrolState(SphereRobotAI ai, PatrolRoute patrolRoute) : base(ai) {
        this.patrolRoute = patrolRoute;
    }
    public override void Enter() {
        base.Enter();
        SetupRootNode();
    }
    void SetupRootNode() {
        rootTaskNode = new TaskPatrol(owner.transform, patrolRoute);
    }

    public override PlayerInput Update(ref PlayerInput input) {
        waitCheckTimer += Time.deltaTime;
        if (waitCheckTimer > waitCheckInterval) {
            waitCheckTimer -= waitCheckInterval;
            DoWaitCheck();
        }
        if (waitTimer > 0f) {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0) {
                rootTaskNode.SetData(TaskPatrol.WAIT_KEY, false);
            }
        }

        rootTaskNode.Evaluate(ref input);
        return input;
    }

    void DoWaitCheck() {
        if (Random.Range(0f, 1f) < waitProbability) {
            rootTaskNode.SetData(TaskPatrol.WAIT_KEY, true);
            waitTimer = waitInterval.GetRandomInsideBound();
        }
    }
}