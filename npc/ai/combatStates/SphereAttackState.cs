using AI;
using UnityEngine;
using UnityEngine.AI;
public class SphereAttackState : SphereControlState {
    static readonly public string LAST_SEEN_PLAYER_POSITION_KEY = "lastSeenPlayerPosition";
    private TaskNode rootTaskNode;
    // public SphereRobotSpeaker speaker;
    SpeechTextController speechTextController;
    readonly float ATTACK_TIMEOUT = 3f;
    readonly float ROUTINE_TIMEOUT = 10f;
    readonly float MAX_SHOOT_RANGE = 20f;
    float timeSinceSawPlayer;
    float changeStateCountDown;
    public GunHandler gunHandler;
    Vector3 lastSeenPlayerPosition;
    CharacterController characterController;
    public SphereAttackState(SphereRobotAI ai, GunHandler gunHandler, CharacterController characterController, SpeechTextController speechTextController) : base(ai) {
        this.gunHandler = gunHandler;
        this.characterController = characterController;
        // speaker = owner.GetComponentInChildren<SphereRobotSpeaker>();
        this.speechTextController = speechTextController;
        if (speechTextController != null) {
            speechTextController.SayAttack();
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
                new TaskConditional(() => gunHandler.gunInstance.delta.clip > 0),
                new TaskReload(gunHandler)
            ),
            new Selector(
                new Sequence(
                    new TaskConditional(() => isPlayerVisible()),
                    new TaskShoot(gunHandler)
                ),
                new TaskMoveToKey(owner.transform, LAST_SEEN_PLAYER_POSITION_KEY, owner.physicalKeys, characterController)
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
            owner.StateFinished(this, TaskState.success);
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
