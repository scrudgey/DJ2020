using System;
using UnityEngine;

public class ObjectiveDelta {
    public ObjectiveStatus status;
    public Objective.Visibility visibility;
    public Objective template;
    public Func<Vector3> GetPosition;
    public ObjectiveDelta(Objective objective, Func<Vector3> GetPosition) {
        this.template = objective;
        this.GetPosition = GetPosition;
        this.visibility = objective.visibility; // TODO: ??
        status = ObjectiveStatus.inProgress;
    }
}
