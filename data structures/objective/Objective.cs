using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public abstract class Objective : ScriptableObject {
    public enum Visibility { unknown, known }
    public Visibility visibility;
    public string title;
    [TextArea(15, 20)]
    public string decsription;
    [JsonConverter(typeof(ScriptableObjectJsonConverter<Sprite>))]
    public Sprite objectiveImage;
    public List<string> potentialSpawnPoints;
    public List<Vector3> spawnPointLocations;
    [Header("bonus reward")]
    public int bonusRewardCredits;
    public int bonusRewardFavors;
    public int bonusRewardSkillpoints;

    public static string ReadableFinalStatus(ObjectiveStatus status) => status switch {
        ObjectiveStatus.canceled => "CANCELED",
        ObjectiveStatus.complete => "COMPLETE",
        ObjectiveStatus.disabled => "N/A",
        ObjectiveStatus.failed => "FAILED",
        ObjectiveStatus.inProgress => "FAILED",
        _ => "NONE"
    };

    public Vector3 SpawnPointLocation(string idn) {
        int index = potentialSpawnPoints.IndexOf(idn);
        return spawnPointLocations[index];
    }

    public abstract ObjectiveDelta ToDelta(LevelState state);

    public virtual string SelectSpawnPointIdn(LevelTemplate template, LevelPlan plan) {
        if (plan.objectiveLocations.ContainsKey(name)) {
            return plan.objectiveLocations[name];
        }
        return Toolbox.RandomFromList(potentialSpawnPoints);
    }

    public void ApplyReward(GameData gameData) {
        Debug.Log($"bonus objective {title} applying reward");
        gameData.playerState.credits += bonusRewardCredits;
        gameData.playerState.favors += bonusRewardFavors;
        gameData.playerState.skillpoints += bonusRewardSkillpoints;
    }
}
