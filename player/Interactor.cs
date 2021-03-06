using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class HighlightableTargetData {
    public Highlightable target;
    public Collider collider;
    public HighlightableTargetData(Highlightable target, Collider collider) {
        this.target = target;
        this.collider = collider;
    }
    static public bool Equality(HighlightableTargetData a, HighlightableTargetData b) {
        if (a == null && b == null) {
            return true;
        } else if (a == null || b == null) {
            return false;
        } else {
            return a.target == b.target && a.collider == b.collider;
        }
    }
}
public class InteractorTargetData : HighlightableTargetData {
    new public Interactive target;
    public InteractorTargetData(Interactive target, Collider collider) : base(target, collider) {
        this.target = target;
    }
}
public class Interactor : MonoBehaviour, IBindable<Interactor>, IInputReceiver {
    public Action<Interactor> OnValueChanged { get; set; }
    public Action<InteractorTargetData> OnActionDone;
    public Dictionary<Collider, Interactive> interactives = new Dictionary<Collider, Interactive>();
    public HighlightableTargetData highlighted = null;
    public Suspiciousness suspiciousness = Suspiciousness.normal;
    public void AddInteractive(Collider other) {
        Interactive interactive = other.GetComponent<Interactive>();
        if (interactive) {
            interactives[other] = interactive;
            interactive.interactor = this;
        }
        RemoveNullInteractives();
        OnValueChanged?.Invoke(this);
    }
    public Suspiciousness GetSuspiciousness() { // TODO: 
        return suspiciousness;
    }
    public void RemoveInteractive(Collider other) {
        if (interactives.ContainsKey(other)) {
            interactives[other].interactor = null;
            interactives.Remove(other);
        }
        RemoveNullInteractives();
        OnValueChanged?.Invoke(this);
    }
    public void RemoveInteractive(Interactive other) {
        foreach (var item in interactives.Where(kvp => kvp.Value == other).ToList()) {
            item.Value.interactor = null;
            interactives.Remove(item.Key);
        }
        OnValueChanged?.Invoke(this);
    }

    public InteractorTargetData ActiveTarget() {
        RemoveNullInteractives();
        if (interactives.Count == 0) {
            return null;
        }
        List<InteractorTargetData> data = new List<InteractorTargetData>();
        foreach (KeyValuePair<Collider, Interactive> kvp in interactives) {
            data.Add(new InteractorTargetData(kvp.Value, kvp.Key));
        }
        return Interactive.TopTarget(data);
    }
    void RemoveNullInteractives() {
        interactives = interactives
            .Where(f => f.Value != null && f.Key != null)
            .ToDictionary(x => x.Key, x => x.Value);
    }

    void OnTriggerEnter(Collider other)
        => AddInteractive(other);

    void OnTriggerExit(Collider other)
        => RemoveInteractive(other);

    // TODO: fix
    public void SetInputs(PlayerInput inputs) {
        // if (inputs.state != CharacterState.wallPress) {
        //     if (inputs.Fire.targetData.highlightableTargetData != highlighted) {
        //         highlighted = inputs.Fire.targetData.highlightableTargetData;
        //         OnValueChanged?.Invoke(this);
        //     }
        // } else {
        //     if (highlighted != null) {
        //         highlighted = null;
        //         OnValueChanged?.Invoke(this);
        //     }
        // }

        // TODO: handle the case when there's a ladder separate from interactives
        if (inputs.actionButtonPressed) {
            InteractorTargetData data = ActiveTarget();
            if (data == null) return;
            data.target.DoAction(this);
            OnActionDone?.Invoke(data);
        }
    }
}
