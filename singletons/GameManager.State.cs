using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KinematicCharacterController;
using UnityEngine;
public partial class GameManager : Singleton<GameManager> {
    public static Action<List<ObjectiveDelta>, ObjectiveDelta, bool> OnObjectivesChange;
    public static Action<LootData, GameData> OnLootChange;
    public static Action<PayData, GameData> OnPayDataChange;
    public static Action<int, String> OnItemPickup;
    public static Action<StatusUpdateData> OnGameStateChange;
    int lastObjectivesStatusHashCode;
    int lastOptionalObjectiveStatusHashCode;

    public void AddPayDatas(PayData data, Vector3 position) {
        gameData.levelState.delta.levelAcquiredPaydata.Add(data);
        OnPayDataChange?.Invoke(data, gameData);

        StatusUpdateData.StatusType statusType = data.type == PayData.DataType.password ? StatusUpdateData.StatusType.passwordData : StatusUpdateData.StatusType.data;
        OnGameStateChange?.Invoke(new StatusUpdateData() {
            type = statusType,
            increment = 1,
            originLocation = position
        });
    }

    public void RemovePayData(PayData data, Vector3 position) {
        gameData.levelState.delta.levelAcquiredPaydata.Remove(data);
        OnPayDataChange?.Invoke(data, gameData);
        StatusUpdateData.StatusType statusType = data.type == PayData.DataType.password ? StatusUpdateData.StatusType.passwordData : StatusUpdateData.StatusType.data;
        OnGameStateChange?.Invoke(new StatusUpdateData() {
            type = statusType,
            increment = -1,
            originLocation = position
        });
        // TODO: reverse status icon fly toward target
    }

    public void AddCredits(int amount, Vector3 position) {
        gameData.levelState.delta.levelAcquiredCredits += amount;
        // OnItemPickup?.Invoke(1, $"{amount}");
        OnGameStateChange?.Invoke(new StatusUpdateData() {
            type = StatusUpdateData.StatusType.credit,
            increment = amount,
            originLocation = position
        });
    }
    public void CollectLoot(LootData data, Vector3 position) {
        gameData.levelState.delta.levelAcquiredLoot.Add(data);
        OnLootChange?.Invoke(data, gameData);
        OnGameStateChange?.Invoke(new StatusUpdateData() {
            type = StatusUpdateData.StatusType.loot,
            increment = 1,
            originLocation = position
        });
    }
    public void AddKey(int keyId, KeyType type, Vector3 position) {
        AddKeyToState(keyId, type);
        OnGameStateChange?.Invoke(new StatusUpdateData() {
            type = StatusUpdateData.StatusType.key,
            increment = 1,
            originLocation = position
        });
    }
    public void AddKeyToState(int keyId, KeyType keyType) {
        gameData.levelState.delta.keys.Add(new KeyData(keyType, keyId));
    }


    public void CheckObjectives(ObjectiveDelta changedDelta) {
        if (gameData.phase != GamePhase.levelPlay) return;
        CheckOptionalObjectives(changedDelta);
        CheckRequiredObjectives(changedDelta);
    }
    void CheckOptionalObjectives(ObjectiveDelta changedDelta) {
        List<ObjectiveStatus> statuses = gameData.levelState.delta.optionalObjectiveDeltas
                    .Select(objective => objective.status).ToList();
        int newHashCode = Toolbox.ListHashCode<ObjectiveStatus>(statuses);
        if (lastOptionalObjectiveStatusHashCode != newHashCode) {
            OnObjectivesChange?.Invoke(gameData.levelState.delta.optionalObjectiveDeltas, changedDelta, true);
        }
        lastOptionalObjectiveStatusHashCode = newHashCode;
    }
    void CheckRequiredObjectives(ObjectiveDelta changedDelta) {
        List<ObjectiveStatus> statuses = gameData.levelState.delta.objectiveDeltas
            .Select(objective => objective.status).ToList();
        ObjectiveStatus newTotalStatus;

        if (statuses.Any(status => status == ObjectiveStatus.failed)) {
            newTotalStatus = ObjectiveStatus.failed;
        } else if (statuses.All(status => status == ObjectiveStatus.complete)) {
            newTotalStatus = ObjectiveStatus.complete;
        } else {
            newTotalStatus = ObjectiveStatus.inProgress;
        }

        if (newTotalStatus != gameData.levelState.delta.missionStatus) {
            gameData.levelState.delta.missionStatus = newTotalStatus;
            if (newTotalStatus == ObjectiveStatus.complete) {
                HandleAllObjectivesComplete();
            } else if (newTotalStatus == ObjectiveStatus.failed) {
                HandleObjectiveFailed();
            }
        }

        int newHashCode = Toolbox.ListHashCode<ObjectiveStatus>(statuses);
        if (lastObjectivesStatusHashCode != newHashCode) {
            OnObjectivesChange?.Invoke(gameData.levelState.delta.objectiveDeltas, changedDelta, false);
        }
        lastObjectivesStatusHashCode = newHashCode;
    }

    public void HandleAllObjectivesComplete() {
        if (GameManager.I.isLoadingLevel) return;
        if (playerCharacterController.state == CharacterState.burgle)
            CloseBurglar();
        uiController.LogMessage($"Objectives complete, proceed to extraction");
        gameData.levelState.delta.phase = LevelDelta.MissionPhase.extractionSuccess;
        ExtractionZone zone = ActivateExtractionPoint();
        if (gameData.levelState.template.showExtractionCutscene)
            CutsceneManager.I.StartCutscene(new ExtractionZoneCutscene(zone));
    }

    public void FailObjective(Objective objective) {
        if (GameManager.I.isLoadingLevel) return;
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

    ExtractionZone ActivateExtractionPoint() {
        Debug.Log($"activate extraction point plan: {gameData.levelState.plan.extractionPointIdn}");
        if (gameData.levelState.plan.extractionPointIdn != "") { // default
            foreach (ExtractionZone zone in GameObject.FindObjectsOfType<ExtractionZone>()) {
                Debug.Log($"activate extraction point check zone {zone.data.idn}");
                if (zone.data.idn == gameData.levelState.plan.extractionPointIdn) {
                    Debug.Log($"enabling extraction zone: {zone}");
                    zone.EnableExtractionZone();
                    if (zone.ContainsPlayerLocation(playerObject.transform.position)) {
                        zone.HandlePlayerActivation();
                    }
                    return zone;
                }
            }
        }
        Debug.Log("[extraction] defaulting random extraction point");
        ExtractionZone z = GameObject.FindObjectOfType<ExtractionZone>();
        if (z != null) {
            z.EnableExtractionZone();
        }
        return z;
    }
}