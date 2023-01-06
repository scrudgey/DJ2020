using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Objective : ScriptableObject {
    public string title;
    [TextArea(15, 20)]
    public string decsription;
    public bool isOptional;
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
}