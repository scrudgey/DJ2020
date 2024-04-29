using System;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class ObjectiveDelta {
    ObjectiveStatus _status;
    public ObjectiveStatus status {
        get { return _status; }
        set {
            _status = value;
            GameManager.I.CheckObjectives(this);
        }
    }
    public Objective.Visibility visibility;

    [JsonConverter(typeof(Objective))]
    public Objective template;

    public Func<Vector3> GetPosition;

    public string targetIdn;
    public bool hasLocation = true;

    public ObjectiveDelta(Objective objective, Func<Vector3> GetPosition, string targetIdn) {
        this.template = objective;
        this.GetPosition = GetPosition;
        this.visibility = objective.visibility; // TODO: ??
        this.targetIdn = targetIdn;
        status = ObjectiveStatus.inProgress;
    }

}
