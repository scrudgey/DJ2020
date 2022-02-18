using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HackTargetData {
    public CyberComponent target;
    public HackTargetData(CyberComponent target, Collider collider) {
        this.target = target;
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
    public HackInput ToHackInput() => new HackInput {
        targetNode = GameManager.I.GetCyberNode(target.idn)
    };
}
public class ManualHacker : MonoBehaviour, IBindable<ManualHacker> {
    public Action<ManualHacker> OnValueChanged { get; set; }
    public Action<HackTargetData> OnActionDone;
    public Dictionary<Collider, CyberComponent> cyberComponents = new Dictionary<Collider, CyberComponent>();
    public void AddInteractive(Collider other) {
        CyberComponent component = other.GetComponent<CyberComponent>();
        if (component) {
            cyberComponents[other] = component;
        }
        RemoveNullCyberComponents();
        OnValueChanged?.Invoke(this);
    }
    public void RemoveInteractive(Collider other) {
        if (cyberComponents.ContainsKey(other)) {
            cyberComponents.Remove(other);
        }
        RemoveNullCyberComponents();
        OnValueChanged?.Invoke(this);
    }
    public void RemoveInteractive(CyberComponent other) {
        foreach (var item in cyberComponents.Where(kvp => kvp.Value == other).ToList()) {
            cyberComponents.Remove(item.Key);
        }
        OnValueChanged?.Invoke(this);
    }

    public HackTargetData ActiveTarget() {
        // TODO: weigh the targets in some way, return deterministic
        RemoveNullCyberComponents();
        if (cyberComponents.Count == 0) {
            return null;
        } else return cyberComponents
            .ToList()
            .Select((KeyValuePair<Collider, CyberComponent> kvp) => new HackTargetData(kvp.Value, kvp.Key))
            .First();
    }
    void RemoveNullCyberComponents() => cyberComponents =
        cyberComponents
            .Where(f => f.Value != null && f.Key != null)
            .ToDictionary(x => x.Key, x => x.Value);

    void OnTriggerEnter(Collider other)
        => AddInteractive(other);

    void OnTriggerExit(Collider other)
        => RemoveInteractive(other);

    public void SetInputs(PlayerCharacterInput inputs) {
        if (inputs.useItem) {
            HackTargetData data = ActiveTarget();
            if (data == null) return;

            Debug.Log("doing manual hack");

            HackController.I.HandleHackInput(data.ToHackInput());

            OnActionDone?.Invoke(data);
        }
    }
}
