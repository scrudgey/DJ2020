using AI;
using UnityEngine;
using UnityEngine.AI;


public class SearchDirectionState : SphereControlState {
    static readonly public string SEARCH_POSITION_KEY = "investigatePosition";
    readonly float ROUTINE_TIMEOUT = 60f;
    float changeStateCountDown;
    private Vector3 searchDirection;
    private TaskNode rootTaskNode;
    TaskTimerDectorator lookAround;
    public SearchDirectionState(SphereRobotAI ai, Damage damage, bool doIntro = true, float speedCoefficient = 0.5f) : base(ai) {
        if (damage != null) {
            searchDirection = -1f * damage.direction;
        } else {
            RandomSearchDirection();
        }
        SetupRootNode(doIntro, speedCoefficient);
        rootTaskNode.SetData(SEARCH_POSITION_KEY, searchDirection);
    }
    public SearchDirectionState(SphereRobotAI ai, NoiseComponent noise, bool doIntro = true, float speedCoefficient = 0.5f) : base(ai) {
        if (noise != null) {
            searchDirection = (noise.transform.position - owner.transform.position).normalized;
            searchDirection.y = ai.transform.position.y;
        } else {
            RandomSearchDirection();
        }
        SetupRootNode(doIntro, speedCoefficient);
        rootTaskNode.SetData(SEARCH_POSITION_KEY, noise.transform.position);

    }
    public SearchDirectionState(SphereRobotAI ai, Vector3 position, bool doIntro = true, float speedCoefficient = 0.5f) : base(ai) {
        if (position != Vector3.zero) {
            searchDirection = (position - owner.transform.position).normalized;
            searchDirection.y = ai.transform.position.y;
        } else {
            RandomSearchDirection();
        }
        SetupRootNode(doIntro, speedCoefficient);
        rootTaskNode.SetData(SEARCH_POSITION_KEY, position);
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

    void SetupRootNode(bool intro, float speedCoefficient) {

        // TODO problem: changing search direction
        Vector3 leftDirection = Quaternion.Euler(0, -45, 0) * searchDirection;
        Vector3 rightDirection = Quaternion.Euler(0, 45, 0) * searchDirection;

        if (intro) {
            lookAround = new TaskTimerDectorator(new Sequence(
                   // look
                   new TaskTimerDectorator(new TaskLookAt(owner.transform) {
                       lookType = TaskLookAt.LookType.direction,
                       lookAtPoint = searchDirection
                   }, 1f),
                   // look left
                   new TaskTimerDectorator(new TaskLookAt(owner.transform) {
                       lookType = TaskLookAt.LookType.direction,
                       lookAtPoint = leftDirection
                   }, 1f),
                   // look right
                   new TaskTimerDectorator(new TaskLookAt(owner.transform) {
                       lookType = TaskLookAt.LookType.direction,
                       lookAtPoint = rightDirection
                   }, 1f)
               ), 3f);

            rootTaskNode = new Sequence(lookAround,
                new TaskMoveToKey(owner.transform, SEARCH_POSITION_KEY, owner.physicalKeys, arrivalDistance: 1f) {
                    headBehavior = TaskMoveToKey.HeadBehavior.search,
                    speedCoefficient = speedCoefficient,
                },
                new TaskTimerDectorator(new TaskLookAt(owner.transform) {
                    lookType = TaskLookAt.LookType.position,
                    key = SEARCH_POSITION_KEY,
                    useKey = true,
                    headBehavior = TaskLookAt.HeadBehavior.search
                }, 2f)
            );
        } else {
            rootTaskNode = new Sequence(
                new TaskMoveToKey(owner.transform, SEARCH_POSITION_KEY, owner.physicalKeys, arrivalDistance: 1f) {
                    headBehavior = TaskMoveToKey.HeadBehavior.search,
                    speedCoefficient = speedCoefficient
                },
                new TaskTimerDectorator(new TaskLookAt(owner.transform) {
                    lookType = TaskLookAt.LookType.position,
                    key = SEARCH_POSITION_KEY,
                    useKey = true,
                    headBehavior = TaskLookAt.HeadBehavior.search
                }, 3f)
            );
        }
    }

    public override PlayerInput Update(ref PlayerInput input) {
        changeStateCountDown -= Time.deltaTime;
        if (changeStateCountDown <= 0) {
            owner.StateFinished(this);
        }
        TaskState result = rootTaskNode.Evaluate(ref input);
        if (result == TaskState.success) {
            owner.StateFinished(this);
        }
        return input;
    }

    public override void OnNoiseHeard(NoiseComponent noise) {
        base.OnNoiseHeard(noise);
        // TODO: more detailed decision making if sound is suspicious
        if (noise.data.player) {
            if (noise.data.suspiciousness > Suspiciousness.normal || noise.data.isFootsteps) {
                searchDirection = noise.transform.position;
                changeStateCountDown = ROUTINE_TIMEOUT;
                rootTaskNode.SetData(SEARCH_POSITION_KEY, searchDirection);
                if (lookAround != null)
                    lookAround.Abort();
            }
        }
    }
}