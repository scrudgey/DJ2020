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

    static public bool Equality(InteractorTargetData a, InteractorTargetData b) {
        if (a == null && b == null) {
            return true;
        } else if (a == null || b == null) {
            return false;
        } else {
            return a.target == b.target && a.collider == b.collider;
        }
    }
}
public class Interactor : MonoBehaviour, IBindable<Interactor> {
    public Action<Interactor> OnValueChanged { get; set; }

    public HashSet<InteractorTargetData> interactives = new HashSet<InteractorTargetData>();
    public InteractorTargetData highlighted = null;
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
            interactive.DisableOutline();
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
        interactives.RemoveWhere(interactive => interactive == null);
        if (interactives.Count == 0) {
            return null;
        }
        return Interactive.TopTarget(interactives);
    }

    void OnTriggerEnter(Collider other) {
        AddInteractive(other);
    }
    void OnTriggerExit(Collider other) {
        RemoveInteractive(other);
    }

    public void SetInputs(ref PlayerCharacterInput inputs) {
        if (inputs.Fire.targetData.interactorData != highlighted) {
            highlighted = inputs.Fire.targetData.interactorData;
            OnValueChanged?.Invoke(this);
        }

        // TODO: handle the case when there's a ladder separate from interactives
        if (inputs.actionButtonPressed) {
            InteractorTargetData data = ActiveTarget();
            if (data == null) return;
            data.target.DoAction(this);
            OnActionDone?.Invoke(data);
        }

    }
}
