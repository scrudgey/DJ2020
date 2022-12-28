using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
// [CreateAssetMenu(menuName = "ScriptableObjects/Objective")]

public abstract class Objective : ScriptableObject {
    public string title;
    public string decsription;
    public bool isOptional;
    abstract public ObjectiveStatus Status(GameData data);
    abstract public float Progress(GameData data);
}