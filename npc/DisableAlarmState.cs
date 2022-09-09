using AI;
using UnityEngine;
using UnityEngine.AI;


public class DisableAlarmState : SphereControlState {
    private TaskNode rootTaskNode;
    private SpeechTextController speechTextController;

    public DisableAlarmState(SphereRobotAI ai, SpeechTextController speechTextController) : base(ai) {
        this.speechTextController = speechTextController;
        SetupRootNode();
    }

    void SetupRootNode() {
        LevelData levelData = GameManager.I.gameData.levelData;
        if (GameManager.I.levelHQTerminal() != null && levelData.alarm) {
            HQReport report = new HQReport {
                reporter = owner.gameObject,
                desiredAlarmState = false,
                locationOfLastDisturbance = owner.getLocationOfInterest(),
                timeOfLastContact = Time.time,
                lifetime = 6f,
                speechText = "HQ respond. All clear."
            };
            rootTaskNode = new TaskTimerDectorator(new TaskRadioHQ(owner, speechTextController, owner.alertHandler, report), 6.5f);
        } else {
            rootTaskNode = new TaskSucceed();
        }
    }

    public override PlayerInput Update(ref PlayerInput input) {
        TaskState result = rootTaskNode.Evaluate(ref input);
        if (result == TaskState.success) {
            owner.StateFinished(this);
        } else if (result == TaskState.failure) {
            owner.StateFinished(this);
        }
        return input;
    }
}