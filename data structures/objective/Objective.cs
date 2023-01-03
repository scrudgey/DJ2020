using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Objective : ScriptableObject {
    public string title;
    [TextArea(15, 20)]
    public string decsription;
    public bool isOptional;
    public Sprite objectiveImage;
    abstract public ObjectiveStatus Status(GameData data);
    abstract public float Progress(GameData data);
}