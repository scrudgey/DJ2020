using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class ManualHackInput {
    public PlayerCharacterInput playerInput;
    public Items.BaseItem activeItem;
}
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
    public HackInput ToManualHackInput() => new HackInput {
        targetNode = GameManager.I.GetCyberNode(target.idn),
        type = HackType.manual
    };
}
public class ManualHacker : MonoBehaviour {
    public Action<HackTargetData> OnActionDone;
    public Dictionary<Collider, CyberComponent> cyberComponents = new Dictionary<Collider, CyberComponent>();
    bool hackToolDeployed;
    public void AddInteractive(Collider other) {
        CyberComponent component = other.GetComponent<CyberComponent>();
        if (component) {
            cyberComponents[other] = component;
        }
        RemoveNullCyberComponents();
        HackController.I.HandleVulnerableManualNodes(GetVulnerableNodes());
    }
    public void RemoveInteractive(Collider other) {
        if (cyberComponents.ContainsKey(other)) {
            cyberComponents.Remove(other);
        }
        RemoveNullCyberComponents();
        HackController.I.HandleVulnerableManualNodes(GetVulnerableNodes());
    }

    public HackTargetData ActiveTarget() {
        RemoveNullCyberComponents();
        if (!hackToolDeployed) {
            return null;
        }
        if (cyberComponents.Count == 0) {
            return null;
        } else return cyberComponents
            .ToList()
            .Where((KeyValuePair<Collider, CyberComponent> kvp) => IsNodeVulnerable(kvp.Value.GetNode()))
            .Select((KeyValuePair<Collider, CyberComponent> kvp) => new HackTargetData(kvp.Value, kvp.Key))
            .DefaultIfEmpty(null)
            .First(); // TODO: weigh the targets in some way, return deterministic
    }

    bool IsNodeVulnerable(CyberNode node) {
        // TODO: compute various checks to see if node is indeed vulnerable
        return hackToolDeployed && !node.compromised;
    }
    void RemoveNullCyberComponents() => cyberComponents =
        cyberComponents
            .Where(f => f.Value != null && f.Key != null)
            .ToDictionary(x => x.Key, x => x.Value);

    void OnTriggerEnter(Collider other) => AddInteractive(other);

    void OnTriggerExit(Collider other) => RemoveInteractive(other);

    public void SetInputs(ManualHackInput inputs) {
        bool refresh = false;
        if (hackToolDeployed != inputs.activeItem.EnablesManualHack()) {
            hackToolDeployed = inputs.activeItem.EnablesManualHack();
            refresh = true;
        }
        if (refresh) {
            HackController.I.HandleVulnerableManualNodes(GetVulnerableNodes());
        }
        if (inputs.playerInput.useItem) {
            HackTargetData data = ActiveTarget();
            if (data == null) return;

            HackController.I.HandleHackInput(data.ToManualHackInput());
            OnActionDone?.Invoke(data);
        }
    }

    public List<CyberNode> GetVulnerableNodes() => cyberComponents
        .ToList()
        .Select((KeyValuePair<Collider, CyberComponent> kvp) => kvp.Value.GetNode())
        .Where(IsNodeVulnerable)
        .ToList();

}
