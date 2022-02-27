using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class ManualHackInput {
    public PlayerInput playerInput;
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
    public SphereCollider sphereCollider;
    public Action<HackTargetData> OnActionDone;
    public Dictionary<Collider, CyberComponent> cyberComponents = new Dictionary<Collider, CyberComponent>();
    bool hackToolDeployed;
    public LineRenderer lineRenderer;
    public Material wireUnfurlMaterial;
    public Material wireAttachedMaterial;
    private readonly AnimationCurve fatWidth = AnimationCurve.Constant(0f, 1f, 1f);
    private readonly AnimationCurve thinWidth = AnimationCurve.Constant(0f, 1f, 0.05f);
    float timer;
    public Color wireColor;
    public AudioSource audioSource;
    public AudioClip wireDeploy;
    public AudioClip wireAttach;
    bool deploySoundPlayed;
    bool attachSoundPlayed;
    void Start() {
        audioSource = Toolbox.SetUpAudioSource(gameObject);
    }
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
        if (inputs.activeItem != null && hackToolDeployed != inputs.activeItem.EnablesManualHack()) {
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

    void Update() {
        CyberNode node = GetVulnerableNodes().DefaultIfEmpty(null).FirstOrDefault();
        if (node != null) {
            timer += Time.deltaTime;
            UpdateWire(node);
        } else {
            timer = 0f;
            deploySoundPlayed = false;
            attachSoundPlayed = false;
            lineRenderer.enabled = false;
        }
        float radius = GameManager.I?.gameData?.playerData?.hackRadius ?? 1.5f;
        sphereCollider.radius = radius;
    }
    void UpdateWire(CyberNode node) {
        Vector3 playerPos = transform.position;
        lineRenderer.enabled = true;
        Vector3[] points = new Vector3[2];
        if (timer < 0.25) {
            Vector3 direction = node.position - playerPos;
            float length = 0.15f + 0.65f * (timer / 0.25f);
            points = new Vector3[]{
                    playerPos,
                    playerPos + length * direction.normalized
                };
            lineRenderer.material = wireUnfurlMaterial;
            lineRenderer.widthCurve = fatWidth;
            lineRenderer.startColor = Color.white;
            lineRenderer.endColor = Color.white;

            if (!deploySoundPlayed) {
                deploySoundPlayed = true;
                Toolbox.RandomizeOneShot(audioSource, wireDeploy);
            }
        } else {
            points = new Vector3[]{
                    node.position,
                    playerPos
                };
            lineRenderer.material = wireAttachedMaterial;
            lineRenderer.widthCurve = thinWidth;
            lineRenderer.startColor = wireColor;
            lineRenderer.endColor = wireColor;

            if (!attachSoundPlayed) {
                attachSoundPlayed = true;
                Toolbox.RandomizeOneShot(audioSource, wireAttach);
            }
        }
        lineRenderer.SetPositions(points);
        lineRenderer.positionCount = 2;
    }
}
