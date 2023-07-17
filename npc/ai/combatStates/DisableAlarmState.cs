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
        LevelState levelData = GameManager.I.gameData.levelState;
        if (GameManager.I.levelRadioTerminal() != null) {
            HQReport report = new HQReport {
                reporter = owner.gameObject,
                desiredAlarmState = HQReport.AlarmChange.cancelAlarm,
                locationOfLastDisturbance = owner.getLocationOfInterest(),
                timeOfLastContact = Time.time,
                lifetime = 5f,
                speechText = "HQ respond. All clear."
            };
            rootTaskNode = new TaskTimerDectorator(new TaskRadioHQ(owner, speechTextController, owner.alertHandler, report), 6.5f);
        } else {
            rootTaskNode = new TaskSucceed();
        }
    }

    public override PlayerInput Update(ref PlayerInput input) {
        TaskState result = rootTaskNode.Evaluate(ref input);
        if (result == TaskState.success || result == TaskState.failure) {
            owner.StateFinished(this, result);
        }
        return input;
    }
}