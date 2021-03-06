using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class HackInput {
    public CyberNode targetNode;
    public HackType type;
}
public enum HackType { none, network, manual }
public class HackController : Singleton<HackController>, IBindable<HackController> {
    public class HackData {
        public CyberNode node;
        public float timer;
        public float lifetime;
        public HackType type;
        public bool done;
    }

    public AudioSource audioSource;
    public AudioClip hackStarted;
    public AudioClip hackFinished;
    public AudioClip hackInProgress;
    public Action<HackController> OnValueChanged { get; set; }
    public float hackInProgressTimer;
    public List<HackData> targets = new List<HackData>();
    public List<CyberNode> vulnerableManualNodes = new List<CyberNode>();
    public CyberNode vulnerableNetworkNode = null;
    // public LineRenderer lineRenderer;
    // public Material wireUnfurlMaterial;
    // public Material wireAttachedMaterial;

    // private readonly AnimationCurve fatWidth = AnimationCurve.Constant(0f, 1f, 1f);
    // private readonly AnimationCurve thinWidth = AnimationCurve.Constant(0f, 1f, 0.1f);

    void Awake() {
        vulnerableNetworkNode = null;
        // lineRenderer.enabled = false;
    }
    public void HandleHackInput(HackInput input) {
        if (targets.Count >= GameManager.I.gameData.playerData.maxConcurrentNetworkHacks)
            return;
        if (!targets.Any(t => t.node == input.targetNode)) {
            HackData data = new HackData {
                node = input.targetNode,
                timer = 0f,
                lifetime = 5f,
                type = input.type
            };
            targets.Add(data);
            OnValueChanged?.Invoke(this);
            audioSource.PlayOneShot(hackStarted);
        }
    }
    public void HandleVulnerableNetworkNode(CyberNode input) {
        vulnerableNetworkNode = input;
        OnValueChanged?.Invoke(this);
    }
    public void HandleVulnerableManualNodes(List<CyberNode> input) {
        vulnerableManualNodes = input;
        OnValueChanged?.Invoke(this);
    }

    void Update() {
        foreach (HackData data in targets) {
            data.timer += Time.deltaTime * GameManager.I.gameData.playerData.hackSpeedCoefficient;
            if (data.timer > data.lifetime) {
                GameManager.I.SetCyberNodeState(data.node, true);
                data.done = true;
                audioSource.PlayOneShot(hackFinished);
            }
            // TODO: abort manual hacks if player moves out of range
            if (data.type == HackType.manual) {
                UpdateManualHack(data);
            }
        }
        List<HackData> done = targets.Where(x => x.done).ToList();
        if (targets.Count > 0) {
            targets = targets.Except(done).ToList();
            hackInProgressTimer += Time.deltaTime;
            if (hackInProgressTimer > 1f) {
                audioSource.PlayOneShot(hackInProgress);
                hackInProgressTimer = 0f;
            }
            OnValueChanged?.Invoke(this);
        } else {
            hackInProgressTimer = 0f;
        }
    }

    void UpdateManualHack(HackData data) {
        // really ugly
        Vector3 playerPos = GameManager.I.playerObject.transform.position + new Vector3(0f, 1f, 0f);
        Vector3[] points = new Vector3[2];
        points = new Vector3[]{
                    data.node.position,
                    playerPos
                };

        // this is weird, and indicates that state should be handled by manual hacker?
        float radius = GameManager.I?.gameData?.playerData.hackRadius ?? 1.5f;
        if (Vector3.Distance(points[0], points[1]) > radius) {
            data.done = true;
        }
    }
}
