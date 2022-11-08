using AI;
using UnityEngine;
using UnityEngine.AI;
public class SphereHoldAtGunpointState : SphereControlState {
    static readonly public string LAST_SEEN_PLAYER_POSITION_KEY = "lastSeenPlayerPosition";
    static readonly float AGGRESSION_THRESHOLD = 2.5f;
    public SpeechTextController speechTextController;
    float timeSinceSawPlayer;
    Vector3 lastSeenPlayerPosition;
    float integratedPlayerMovement;
    float totalPlayerMovement;

    public SphereHoldAtGunpointState(SphereRobotAI ai) : base(ai) {
        speechTextController = owner.GetComponentInChildren<SpeechTextController>();
    }
    public override void Enter() {
        base.Enter();
        lastSeenPlayerPosition = Vector3.zero;
        integratedPlayerMovement = 1.5f;
        speechTextController.Say("<color=#ff4757>Freeze! Don't move!</color>");
    }
    public bool isPlayerVisible() {
        return timeSinceSawPlayer < 0.15f;
    }
    public bool isPlayerSuspicious() {
        return !isPlayerVisible() || integratedPlayerMovement > AGGRESSION_THRESHOLD;
    }
    public override PlayerInput Update(ref PlayerInput input) {
        timeSinceSawPlayer += Time.deltaTime;

        // Debug.Log($"gunpoint: {timeSinceSawPlayer} {integratedPlayerMovement}");
        if (isPlayerVisible()) {
            if (integratedPlayerMovement <= 0) {
                owner.StateFinished(this);
            } else if (isPlayerSuspicious()) {
                SuspicionRecord record = new SuspicionRecord() {
                    content = "fled from questioning",
                    suspiciousness = Suspiciousness.aggressive,
                    lifetime = 60f,
                    maxLifetime = 60f
                };
                GameManager.I.AddSuspicionRecord(record);
                owner.StateFinished(this);
            }
            if (integratedPlayerMovement > 0) {
                integratedPlayerMovement -= Time.deltaTime * 1.5f;
            }
        } else {
            // apply suspicion record
            // transfer to attack state
            SuspicionRecord record = new SuspicionRecord() {
                content = "fled from questioning",
                suspiciousness = Suspiciousness.aggressive,
                lifetime = 60f,
                maxLifetime = 60f
            };
            GameManager.I.AddSuspicionRecord(record);
            owner.StateFinished(this);
            integratedPlayerMovement += 1.5f * Time.deltaTime;
        }
        input.lookAtPosition = lastSeenPlayerPosition;
        input.snapToLook = true;
        input.aimWeapon = true;

        // TODO: set gun hold
        return input;
    }

    public override void OnObjectPerceived(Collider other) {
        if (other.transform.IsChildOf(GameManager.I.playerObject.transform)) {
            if (lastSeenPlayerPosition != Vector3.zero) {
                float amountOfMotion = (other.bounds.center - lastSeenPlayerPosition).magnitude;
                integratedPlayerMovement += amountOfMotion;
                totalPlayerMovement += amountOfMotion;
            }

            timeSinceSawPlayer = 0;
            lastSeenPlayerPosition = other.bounds.center;
        }
    }
    public override void OnNoiseHeard(NoiseComponent noise) {
        base.OnNoiseHeard(noise);
        if (noise.data.player) {
            if (timeSinceSawPlayer > 0.2f) {
                timeSinceSawPlayer = 100f;
                // rootTaskNode.SetData(LAST_SEEN_PLAYER_POSITION_KEY, noise.transform.position);
            }
            if (noise.data.suspiciousness > Suspiciousness.normal || noise.data.isFootsteps) {
                Vector3 searchDirection = noise.transform.position;
                // rootTaskNode.SetData(SEARCH_POSITION_KEY, searchDirection);
            }
        }
    }

}
