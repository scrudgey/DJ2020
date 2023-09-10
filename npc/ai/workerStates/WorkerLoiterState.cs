using System.Collections.Generic;
using System.Linq;
using AI;
using UnityEngine;
using UnityEngine.AI;
public class WorkerLoiterState : WorkerNPCControlState {

    public static readonly string NAV_POINT_KEY = "nav_point_key";
    private TaskNode rootTaskNode;
    WorkerNPCAI ai;
    CharacterController characterController;
    // public WorkerLandmark destination;
    public SpeechTextController speechTextController;
    float socialTime;
    public WorkerLoiterState(WorkerNPCAI ai, CharacterController characterController, SpeechTextController speechTextController) : base(ai) {
        this.speechTextController = speechTextController;
        this.ai = ai;
        this.characterController = characterController;
        socialTime = 5f;
    }
    public override void Enter() {
        base.Enter();
        SetupRootNode();
        socialTime = 5f;
        ai.SetCurrentLandmark(null);
        ai.UnexcludeCurrentLandmark();
    }
    public override void Exit() {
        ai.SetCurrentLandmark(null);
        socialTime = 5f;
        ai.UnexcludeCurrentLandmark();
    }

    void SetupRootNode() {
        ai.destinationLandmark = GetDestination();
        ai.ExcludeLandmark(ai.destinationLandmark);
        float speedCoefficient = Random.Range(0.3f, 0.75f);
        rootTaskNode =
        new Sequence(
         new TaskMoveToKey(owner.transform, NAV_POINT_KEY, new System.Collections.Generic.HashSet<int>(), characterController, arrivalDistance: 1.0f) {
             speedCoefficient = speedCoefficient
         },
         new TaskLambda(() => {
             ai.SetCurrentLandmark(ai.destinationLandmark);
         }),
         new Selector(
            new Sequence(
                new TaskConditional(() => AnyOtherWorkerInMyPlace()),
                new TaskConditional(() => {
                    socialTime -= Time.deltaTime;
                    socialTime = Mathf.Max(0f, socialTime);
                    return socialTime > 0f;
                }),
                new TaskSocialize(speechTextController, null)
            ),
            new TaskTimerDectorator(Random.Range(5f, 30f))
        ),
        new TaskLambda(() => ai.UnexcludeCurrentLandmark())
        );
        rootTaskNode.SetData(NAV_POINT_KEY, ai.destinationLandmark.transform.position);
    }

    bool AnyOtherWorkerInMyPlace() {
        return WorkerLandmark.visitors.Values
                 .Where(value => value == ai.currentLandmark)
                 .Count() > 1;
    }

    WorkerLandmark GetDestination() {
        if (ai.destinationLandmark == ai.landmarkStation || ai.landmarkStation == null) {
            var x = Toolbox.RandomFromList(ai.landmarkPointsOfInterest.Where(landmark => !landmark.isExcluded()).ToList());
            return x;
        } else {
            var x = ai.landmarkStation;
            return x;
        }
    }

    public override PlayerInput Update(ref PlayerInput input) {
        TaskState result = rootTaskNode.Evaluate(ref input);
        if (result == TaskState.success) {
            owner.StateFinished(this);
        }
        return input;
    }

}