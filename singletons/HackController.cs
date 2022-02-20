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
    void Awake() {
        vulnerableNetworkNode = null;
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
    // public void HandleVulnerability(ICyberVulnerabilityExploiter exploiter) {
    //     List<CyberNode> vulnerableNodes = exploiter.GetVulnerableNodes();
    // }
    public void HandleVulnerableNetworkNode(CyberNode input) {
        vulnerableNetworkNode = input;
        // Debug.Log("handle vulnerable network node");
        OnValueChanged?.Invoke(this);
    }
    public void HandleVulnerableManualNodes(List<CyberNode> input) {
        vulnerableManualNodes = input;
        // Debug.Log("handle vulnerable manual node");
        OnValueChanged?.Invoke(this);
    }


    void Update() {
        List<HackData> done = new List<HackData>();
        foreach (HackData data in targets) {
            data.timer += Time.deltaTime * GameManager.I.gameData.playerData.hackSpeedCoefficient;
            if (data.timer > data.lifetime) {
                GameManager.I.SetCyberNodeState(data.node, true);
                done.Add(data);
                audioSource.PlayOneShot(hackFinished);
            }
            // TODO: abort manual hacks if player moves out of range
        }
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
}
