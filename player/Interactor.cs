using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class InteractorTargetData {
    public Interactive target;
    public Collider collider;
    public InteractorTargetData(Interactive target, Collider collider) {
        this.target = target;
        this.collider = collider;
    }
}
public class Interactor : MonoBehaviour {
    public HashSet<InteractorTargetData> interactives = new HashSet<InteractorTargetData>();

    public Action<Interactor> OnValueChanged;
    public void AddInteractive(Collider other) {
        Interactive interactive = other.GetComponent<Interactive>();
        if (interactive) {
            interactives.Add(new InteractorTargetData(interactive, other));
        }
        OnValueChanged(this);
    }
    public void RemoveInteractive(Collider other) {
        Interactive interactive = other.GetComponent<Interactive>();
        if (interactive) {
            interactives.RemoveWhere(data => data.collider == other);
        }
        OnValueChanged(this);
    }

    public InteractorTargetData ActiveTarget() {
        if (interactives.Count == 0) {
            return null;
        }
        return interactives.OrderBy(interactive => interactive.target.priority).First();
    }

    void OnTriggerEnter(Collider other) {
        AddInteractive(other);
    }
    void OnTriggerExit(Collider other) {
        RemoveInteractive(other);
    }
}
