using AI;
using UnityEngine;
using UnityEngine.AI;


public class SearchDirectionState : SphereControlState {
    static readonly public string SEARCH_POSITION_KEY = "investigatePosition";
    readonly float ROUTINE_TIMEOUT = 5f;
    float changeStateCountDown;
    private Vector3 searchDirection;
    private TaskNode rootTaskNode;
    public SearchDirectionState(SphereRobotAI ai, Damage damage, bool doIntro = true) : base(ai) {
        if (damage != null) {
            searchDirection = -1f * damage.direction;
        } else {
            RandomSearchDirection();
        }
        SetupRootNode(doIntro);
        rootTaskNode.SetData(SEARCH_POSITION_KEY, searchDirection);
    }
    public SearchDirectionState(SphereRobotAI ai, NoiseComponent noise, bool doIntro = true) : base(ai) {
        if (noise != null) {
            searchDirection = (noise.transform.position - owner.transform.position).normalized;
            searchDirection.y = 0;
        } else {
            RandomSearchDirection();
        }
        SetupRootNode(doIntro);
        rootTaskNode.SetData(SEARCH_POSITION_KEY, noise.transform.position);
    }
    void RandomSearchDirection() {
        searchDirection = Random.insideUnitSphere;
        searchDirection.y = 0;
        searchDirection = searchDirection.normalized;
    }

    public override void Enter() {
        changeStateCountDown = ROUTINE_TIMEOUT;
        owner.navMeshPath = new NavMeshPath();
    }

    void SetupRootNode(bool intro) {
        // TODO problem: changing search direction
        Vector3 leftDirection = Quaternion.Euler(0, -45, 0) * searchDirection;
        Vector3 rightDirection = Quaternion.Euler(0, 45, 0) * searchDirection;

        // TODO: if from noise, stop and turn head toward noise for a split second first...?
        if (intro) {
            rootTaskNode = new Sequence(
                new TaskTimerDectorator(new Sequence(
                    // look
                    new TaskTimerDectorator(new TaskLookAt() {
                        lookType = TaskLookAt.LookType.direction,
                        lookAt = searchDirection
                    }, 1f),
                    // look left
                    new TaskTimerDectorator(new TaskLookAt() {
                        lookType = TaskLookAt.LookType.direction,
                        lookAt = leftDirection
                    }, 1f),
                    // look right
                    new TaskTimerDectorator(new TaskLookAt() {
                        lookType = TaskLookAt.LookType.direction,
                        lookAt = rightDirection
                    }, 1f)
                ), 3f),
                new TaskMoveToKey(owner.transform, SEARCH_POSITION_KEY) {
                    headBehavior = TaskMoveToKey.HeadBehavior.search,
                    speedCoefficient = 0.5f
                }
            );
        } else {
            rootTaskNode = new TaskMoveToKey(owner.transform, SEARCH_POSITION_KEY) {
                headBehavior = TaskMoveToKey.HeadBehavior.search,
                speedCoefficient = 2f
            };
        }

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
        // TODO: more detailed decision making if sound is suspicious
        if (noise.data.player) {
            searchDirection = noise.transform.position;
            changeStateCountDown = ROUTINE_TIMEOUT;
            rootTaskNode.SetData(SEARCH_POSITION_KEY, searchDirection);
        }
    }
}