using AI;
using UnityEngine;
using UnityEngine.AI;
public class SphereAttackRoutine : SphereControlState {
    static readonly public string LAST_SEEN_PLAYER_POSITION_KEY = "lastSeenPlayerPosition";
    private TaskNode rootTaskNode;
    public SphereRobotSpeaker speaker;
    readonly float ATTACK_TIMEOUT = 3f;
    readonly float ROUTINE_TIMEOUT = 10f;
    readonly float MAX_SHOOT_RANGE = 5f;
    float timeSinceSawPlayer;
    float changeStateCountDown;
    public GunHandler gunHandler;
    Vector3 lastSeenPlayerPosition;

    public SphereAttackRoutine(SphereRobotAI ai,
                               GunHandler gunHandler) : base(ai) {
        this.gunHandler = gunHandler;
        speaker = owner.GetComponent<SphereRobotSpeaker>();
        speaker.DoAttackSpeak();
    }
    public override void Enter() {
        base.Enter();
        changeStateCountDown = ROUTINE_TIMEOUT;
        SetupRootNode();
    }
    void SetupRootNode() {
        rootTaskNode = new Sequence(
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

    public override PlayerInput Update() {
        timeSinceSawPlayer += Time.deltaTime;
        changeStateCountDown -= Time.deltaTime;
        if (changeStateCountDown <= 0) {
            owner.RoutineFinished(this);
        }

        PlayerInput input = new PlayerInput();
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

}
