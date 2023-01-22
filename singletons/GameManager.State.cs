using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KinematicCharacterController;
using UnityEngine;
public partial class GameManager : Singleton<GameManager> {
    public static Action<GameData> OnObjectivesChange;
    int lastObjectivesStatusHashCode;
    public void AddPayDatas(List<PayData> datas) {
        gameData.playerState.payDatas.AddRange(datas);
        CheckObjectives();
    }

    public void AddCredits(int amount) {
        gameData.playerState.credits += amount;
        CheckObjectives();
    }

    public void AddPhysicalKey(int keyId) {
        gameData.playerState.physicalKeys.Add(keyId);
        CheckObjectives();
    }
    public void CompleteAllObjectives() {

    }
    public void CheckObjectives() {
        foreach (Objective objective in gameData.levelState.template.objectives) {
            ObjectiveStatus oldStatus = gameData.levelState.delta.objectivesState[objective];
            ObjectiveStatus newStatus = objective.Status(gameData);
            if (oldStatus != newStatus) {
                uiController.LogMessage($"Objective {objective.title}: {newStatus}");
            }
            gameData.levelState.delta.objectivesState[objective] = newStatus;
        }

        List<ObjectiveStatus> statuses = gameData.levelState.template.objectives
            .Where(objective => !objective.isOptional)
            .Select(objective => objective.Status(gameData)).ToList();

        ObjectiveStatus newTotalStatus;
        if (statuses.Any(status => status == ObjectiveStatus.failed)) {
            newTotalStatus = ObjectiveStatus.failed;
        } else if (statuses.All(status => status == ObjectiveStatus.complete)) {
            newTotalStatus = ObjectiveStatus.complete;
        } else {
            newTotalStatus = ObjectiveStatus.inProgress;
        }

        if (newTotalStatus != gameData.levelState.delta.objectiveStatus) {
            gameData.levelState.delta.objectiveStatus = newTotalStatus;
            // notify watchers
            if (newTotalStatus == ObjectiveStatus.complete) {
                HandleAllObjectivesComplete();
            } else if (newTotalStatus == ObjectiveStatus.failed) {
                HandleObjectiveFailed();
            }
        }

        int newHashCode = statuses.GetHashCode();
        if (lastObjectivesStatusHashCode != newHashCode) {
            OnObjectivesChange?.Invoke(gameData);
        }
        lastObjectivesStatusHashCode = newHashCode;
        // Debug.Log($"level status: {gameData.levelState.delta.objectiveStatus}");
    }

    public void HandleAllObjectivesComplete() {
        if (GameManager.I.isLoadingLevel) return;
        uiController.LogMessage($"Objectives complete, proceed to extraction");
        gameData.levelState.delta.phase = LevelDelta.MissionPhase.extractionSuccess;
        ActivateExtractionPoint();
    }
    public void FailObjective(Objective objective) {
        if (GameManager.I.isLoadingLevel) return;
        gameData.levelState.delta.failedObjectives.Add(objective);
        HandleObjectiveFailed();
    }
    public void HandleObjectiveFailed() {
        if (GameManager.I.isLoadingLevel) return;
        uiController.LogMessage($"Objectives failed, proceed to extraction");
        gameData.levelState.delta.phase = LevelDelta.MissionPhase.extractionFail;
        ActivateExtractionPoint();
    }

    void ActivateExtractionPoint() {
        if (gameData.levelState.plan.extractionPointIdn != "") { // default
            foreach (ExtractionZone zone in GameObject.FindObjectsOfType<ExtractionZone>()) {
                if (zone.data.idn == gameData.levelState.plan.extractionPointIdn) {
                    zone.EnableExtractionZone();
                    return;
                }
            }
        }
        Debug.Log("[extraction] defaulting random extraction point");
        GameObject.FindObjectOfType<ExtractionZone>().EnableExtractionZone();
    }
}