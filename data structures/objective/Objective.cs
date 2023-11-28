using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public abstract class Objective : ScriptableObject {
    public string title;
    [TextArea(15, 20)]
    public string decsription;
    [JsonConverter(typeof(ScriptableObjectJsonConverter<Sprite>))]
    public Sprite objectiveImage;
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
}