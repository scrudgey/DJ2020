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
public class Interactor : MonoBehaviour, IBindable<Interactor> {
    public Action<Interactor> OnValueChanged { get; set; }

    public HashSet<InteractorTargetData> interactives = new HashSet<InteractorTargetData>();

    public Action<InteractorTargetData> OnActionDone;
    public void AddInteractive(Collider other) {
        Interactive interactive = other.GetComponent<Interactive>();
        if (interactive) {
            interactives.Add(new InteractorTargetData(interactive, other));
        }
        OnValueChanged?.Invoke(this);
    }
    public void RemoveInteractive(Collider other) {
        Interactive interactive = other.GetComponent<Interactive>();
        if (interactive) {
            interactives.RemoveWhere(data => data.collider == other);
        }
        OnValueChanged?.Invoke(this);
    }
    public void RemoveInteractive(Interactive other) {
        if (other) {
            interactives.RemoveWhere(data => data.target == other);
        }
        OnValueChanged?.Invoke(this);
    }

    public InteractorTargetData ActiveTarget() {
        if (interactives.Count == 0) {
            return null;
        }
        return interactives
        .Where(interactive => interactive.target.priority > 0)
        .OrderBy(interactive => interactive.target.priority)
        .FirstOrDefault();
    }

    void OnTriggerEnter(Collider other) {
        AddInteractive(other);
    }
    void OnTriggerExit(Collider other) {
        RemoveInteractive(other);
    }

    public void SetInputs(ref PlayerCharacterInput inputs) {
        // TODO: handle the case when there's a ladder separate from interactives
        if (inputs.actionButtonPressed) {
            InteractorTargetData data = ActiveTarget();
            if (data == null) return;
            data.target.DoAction(this);
            OnActionDone?.Invoke(data);
        }
    }
}
