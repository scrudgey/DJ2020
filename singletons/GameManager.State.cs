using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KinematicCharacterController;
using UnityEngine;
public partial class GameManager : Singleton<GameManager> {
    public static Action<GameData, Dictionary<Objective, ObjectiveStatus>> OnObjectivesChange;
    public static Action<LootData, GameData> OnLootChange;
    public static Action<PayData, GameData> OnPayDataChange;
    public static Action<int, String> OnItemPickup;
    public static Action<StatusUpdateData> OnGameStateChange;
    int lastObjectivesStatusHashCode;

    public void AddPayDatas(PayData data, Vector3 position) {
        gameData.levelState.delta.levelAcquiredPaydata.Add(data);
        CheckObjectives();
        OnPayDataChange?.Invoke(data, gameData);
        OnGameStateChange?.Invoke(new StatusUpdateData() {
            type = StatusUpdateData.StatusType.data,
            increment = 1,
            originLocation = position
        });
    }

    public void AddCredits(int amount, Vector3 position) {
        // gameData.playerState.credits += amount;
        gameData.levelState.delta.levelAcquiredCredits += amount;
        CheckObjectives();
        OnItemPickup?.Invoke(1, $"{amount}");
        OnGameStateChange?.Invoke(new StatusUpdateData() {
            type = StatusUpdateData.StatusType.credit,
            increment = amount,
            originLocation = position
        });
    }
    public void CollectLoot(LootData data, Vector3 position) {
        gameData.levelState.delta.levelAcquiredLoot.Add(data);
        OnLootChange?.Invoke(data, gameData);
        CheckObjectives();
        OnGameStateChange?.Invoke(new StatusUpdateData() {
            type = StatusUpdateData.StatusType.loot,
            increment = 1,
            originLocation = position
        });
    }
    public void AddKey(int keyId, DoorLock.LockType type, Vector3 position) {
        switch (type) {
            case DoorLock.LockType.physical:
                AddPhysicalKey(keyId);
                break;
            case DoorLock.LockType.keycard:
                AddKeyCard(keyId);
                break;
        }
        OnGameStateChange?.Invoke(new StatusUpdateData() {
            type = StatusUpdateData.StatusType.key,
            increment = 1,
            originLocation = position
        });
    }
    public void AddPhysicalKey(int keyId) {
        gameData.playerState.physicalKeys.Add(keyId);
        CheckObjectives();
        OnItemPickup?.Invoke(0, $"{keyId}");
    }
    public void AddKeyCard(int keyId) {
        gameData.playerState.keycards.Add(keyId);
        CheckObjectives();
        OnItemPickup?.Invoke(0, $"{keyId}");
    }
    public void CompleteAllObjectives() {

    }
    public void CheckObjectives() {
        if (gameData.phase != GamePhase.levelPlay) return;
        Dictionary<Objective, ObjectiveStatus> changedObjectiveStatuses = new Dictionary<Objective, ObjectiveStatus>();

        foreach (Objective objective in gameData.levelState.template.objectives) {
            ObjectiveStatus oldStatus = gameData.levelState.delta.objectivesState[objective];
            ObjectiveStatus newStatus = objective.Status(gameData);
            if (oldStatus != newStatus) {
                changedObjectiveStatuses[objective] = newStatus;
            }
            gameData.levelState.delta.objectivesState[objective] = newStatus;
        }

        List<ObjectiveStatus> statuses = gameData.levelState.template.objectives
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

        int newHashCode = Toolbox.ListHashCode<ObjectiveStatus>(statuses);
        if (lastObjectivesStatusHashCode != newHashCode) {
            OnObjectivesChange?.Invoke(gameData, changedObjectiveStatuses);
        }
        lastObjectivesStatusHashCode = newHashCode;
    }

    public void HandleAllObjectivesComplete() {
        if (GameManager.I.isLoadingLevel) return;
        if (playerCharacterController.state == CharacterState.burgle)
            CloseBurglar();
        uiController.LogMessage($"Objectives complete, proceed to extraction");
        gameData.levelState.delta.phase = LevelDelta.MissionPhase.extractionSuccess;
        ActivateExtractionPoint();
        uiController.DisplayObjectiveCompleteMessage("Objectives\ncomplete", "proceed to\nextraction point");
    }
    public void FailObjective(Objective objective) {
        if (GameManager.I.isLoadingLevel) return;
        gameData.levelState.delta.failedObjectives.Add(objective);
        HandleObjectiveFailed();
    }
    public void HandleObjectiveFailed() {
        if (GameManager.I.isLoadingLevel) return;
        if (playerCharacterController.state == CharacterState.burgle) {
            CloseBurglar();
        }
        uiController.LogMessage($"Objectives failed, proceed to extraction");
        gameData.levelState.delta.phase = LevelDelta.MissionPhase.extractionFail;
        ActivateExtractionPoint();
        uiController.DisplayObjectiveCompleteMessage("Objectives\nfailed", "proceed to\nextraction point");
    }

    void ActivateExtractionPoint() {
        if (gameData.levelState.plan.extractionPointIdn != "") { // default
            foreach (ExtractionZone zone in GameObject.FindObjectsOfType<ExtractionZone>()) {
                if (zone.data.idn == gameData.levelState.plan.extractionPointIdn) {
                    zone.EnableExtractionZone();
                    if (zone.ContainsPlayerLocation(playerObject.transform.position)) {
                        zone.HandlePlayerActivation();
                    }
                    return;
                }
            }
        }
        Debug.Log("[extraction] defaulting random extraction point");
        GameObject.FindObjectOfType<ExtractionZone>()?.EnableExtractionZone();
    }
}