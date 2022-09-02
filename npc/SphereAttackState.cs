using AI;
using UnityEngine;
using UnityEngine.AI;
public class SphereAttackState : SphereControlState {
    static readonly public string LAST_SEEN_PLAYER_POSITION_KEY = "lastSeenPlayerPosition";
    private TaskNode rootTaskNode;
    public SphereRobotSpeaker speaker;
    readonly float ATTACK_TIMEOUT = 3f;
    readonly float ROUTINE_TIMEOUT = 10f;
    readonly float MAX_SHOOT_RANGE = 10f;
    float timeSinceSawPlayer;
    float changeStateCountDown;
    public GunHandler gunHandler;
    Vector3 lastSeenPlayerPosition;

    public SphereAttackState(SphereRobotAI ai,
                               GunHandler gunHandler) : base(ai) {
        this.gunHandler = gunHandler;
        speaker = owner.GetComponent<SphereRobotSpeaker>();
        if (speaker != null) {
            speaker.DoAttackSpeak();
        }
    }
    public override void Enter() {
        base.Enter();
        changeStateCountDown = ROUTINE_TIMEOUT;
        SetupRootNode();
    }
    void SetupRootNode() {
        rootTaskNode = new Sequence(
            new TaskTimerDectorator(new TaskLookAt(owner.transform) {
                lookType = TaskLookAt.LookType.position,
                key = LAST_SEEN_PLAYER_POSITION_KEY,
                useKey = true
            }, 0.5f),
            new Selector(
                new TaskConditional(() => gunHandler.gunInstance.clip > 0),
                new TaskReload(gunHandler)
            ),
            new Selector(
                new Sequence(
                    new TaskConditional(() => isPlayerVisible()),
                    new TaskShoot(gunHandler)
                ),
                new TaskMoveToKey(owner.transform, LAST_SEEN_PLAYER_POSITION_KEY)
            )
        );
    }

    public bool isPlayerVisible() {
        return Vector3.Distance(owner.transform.position, lastSeenPlayerPosition) < MAX_SHOOT_RANGE && timeSinceSawPlayer < ATTACK_TIMEOUT;
    }

    public override PlayerInput Update(ref PlayerInput input) {
        timeSinceSawPlayer += Time.deltaTime;
        changeStateCountDown -= Time.deltaTime;
        if (changeStateCountDown <= 0) {
            owner.StateFinished(this);
        }
        rootTaskNode.Evaluate(ref input);
        return input;
    }

    public override void OnObjectPerceived(Collider other) {
        if (other.transform.IsChildOf(GameManager.I.playerObject.transform)) {
            changeStateCountDown = ROUTINE_TIMEOUT;
            timeSinceSawPlayer = 0;
            lastSeenPlayerPosition = other.bounds.center;
            rootTaskNode.SetData(LAST_SEEN_PLAYER_POSITION_KEY, lastSeenPlayerPosition);
        }
    }
    public override void OnNoiseHeard(NoiseComponent noise) {
        base.OnNoiseHeard(noise);
        // TODO: more detailed decision making if sound is suspicious
        if (noise.data.player) {
            if (timeSinceSawPlayer > 0.1f) {
                timeSinceSawPlayer = 100f;
                rootTaskNode.SetData(LAST_SEEN_PLAYER_POSITION_KEY, noise.transform.position);
            }
        }
    }
}
