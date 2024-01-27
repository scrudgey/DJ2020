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
    public ObjectiveStatus Status(GameData data) {
        if (data.levelState.delta.failedObjectives.Contains(this)) {
            return ObjectiveStatus.failed;
        } else {
            return EvaluateStatus(data);
        }
    }
    abstract protected ObjectiveStatus EvaluateStatus(GameData data);
    abstract public float Progress(GameData data);

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

    // abstract public Vector3 Location();
}
