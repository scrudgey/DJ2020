using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class BurgleTargetData {
    public Burglar burglar;
    public AttackSurface target;
    public BurgleTargetData(AttackSurface target, Collider collider, Burglar burglar) {
        this.target = target;
        this.burglar = burglar;
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
    // public HackInput ToManualHackInput() => new HackInput {
    //     targetNode = GameManager.I.GetCyberNode(target.idn),
    //     type = HackType.manual
    // };
}
public class Burglar : MonoBehaviour {
    public CharacterController characterController;
    public SphereCollider sphereCollider;
    public CapsuleCollider characterCollider;
    bool hackToolDeployed;
    public Dictionary<Collider, AttackSurface> cyberComponents = new Dictionary<Collider, AttackSurface>();
    public void AddInteractive(Collider other) {
        AttackSurface component = other.GetComponent<AttackSurface>();
        if (component) {
            cyberComponents[other] = component;
        }
        RemoveNullCyberComponents();
        // HackController.I.HandleVulnerableManualNodes(GetVulnerableNodes());
    }
    public void RemoveInteractive(Collider other) {
        if (cyberComponents.ContainsKey(other)) {
            cyberComponents.Remove(other);
        }
        RemoveNullCyberComponents();
        // HackController.I.HandleVulnerableManualNodes(GetVulnerableNodes());
    }

    public BurgleTargetData ActiveTarget() {
        RemoveNullCyberComponents();
        if (cyberComponents.Count == 0) {
            return null;
        } else return cyberComponents
            .ToList()
            .Where((KeyValuePair<Collider, AttackSurface> kvp) => IsNodeVulnerable(kvp.Value))
            .Select((KeyValuePair<Collider, AttackSurface> kvp) => new BurgleTargetData(kvp.Value, kvp.Key, this))
            .DefaultIfEmpty(null)
            .First(); // TODO: weigh the targets in some way, return deterministic
    }

    bool IsNodeVulnerable(AttackSurface node) {
        return Vector3.Distance(node.transform.position, transform.position) < sphereCollider.radius;
    }
    void RemoveNullCyberComponents() => cyberComponents =
        cyberComponents
            .Where(f => f.Value != null && f.Key != null)
            .ToDictionary(x => x.Key, x => x.Value);

    void OnTriggerEnter(Collider other) => AddInteractive(other);

    void OnTriggerExit(Collider other) => RemoveInteractive(other);

    public void SetInputs(ManualHackInput inputs) {
        bool refresh = false;
        if (inputs.activeItem != null && hackToolDeployed != inputs.activeItem.EnablesBurglary()) {
            hackToolDeployed = inputs.activeItem.EnablesBurglary();
            refresh = true;
        }
        if (refresh) {
            // HackController.I.HandleVulnerableManualNodes(GetVulnerableNodes());
        }
        if (inputs.playerInput.useItem) {
            BurgleTargetData data = ActiveTarget();
            if (data == null) return;
            characterController.TransitionToState(CharacterState.burgle);
            GameManager.I.StartBurglar(data);
        }
    }

    public List<AttackSurface> GetVulnerableNodes() => cyberComponents
        .ToList()
        .Select((KeyValuePair<Collider, AttackSurface> kvp) => kvp.Value)
        .Where(IsNodeVulnerable)
        .ToList();

    void Update() {
        AttackSurface node = GetVulnerableNodes().DefaultIfEmpty(null).FirstOrDefault();
        float radius = GameManager.I?.gameData?.playerState?.hackRadius ?? 1.5f;
        sphereCollider.radius = radius;
    }

}
