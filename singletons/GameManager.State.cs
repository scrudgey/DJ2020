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

    public void CheckObjectives() {
        List<ObjectiveStatus> statuses = gameData.levelState.template.objectives
            .Where(objective => !objective.isOptional)
            .Select(objective => objective.Status(gameData)).ToList();



        ObjectiveStatus newStatus;

        if (statuses.Any(status => status == ObjectiveStatus.failed)) {
            newStatus = ObjectiveStatus.failed;
        } else if (statuses.All(status => status == ObjectiveStatus.complete)) {
            newStatus = ObjectiveStatus.complete;
        } else {
            newStatus = ObjectiveStatus.inProgress;
        }

        if (newStatus != gameData.levelState.delta.objectiveStatus) {
            gameData.levelState.delta.objectiveStatus = newStatus;
            // notify watchers
            if (newStatus == ObjectiveStatus.complete) {
                HandleAllObjectivesComplete();
            } else if (newStatus == ObjectiveStatus.failed) {
                HandleObjectiveFailed();
            }
        }

        int newHashCode = statuses.GetHashCode();
        if (lastObjectivesStatusHashCode != newHashCode) {
            OnObjectivesChange?.Invoke(gameData);
        }
        lastObjectivesStatusHashCode = newHashCode;
        Debug.Log($"level status: {gameData.levelState.delta.objectiveStatus}");
    }

    void HandleAllObjectivesComplete() {
        foreach (ExtractionZone zone in GameObject.FindObjectsOfType<ExtractionZone>()) {
            zone.EnableExtractionZone();
        }
    }
    void HandleObjectiveFailed() {
        Debug.Log("objectives failed!");
    }
}