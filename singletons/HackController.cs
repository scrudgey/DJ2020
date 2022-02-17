using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class HackInput {
    public CyberNode targetNode;
}
public class HackController : Singleton<HackController>, IBindable<HackController> {
    public class HackData {
        public CyberNode node;
        public float timer;
        public float lifetime;
    }

    public AudioSource audioSource;
    public AudioClip hackStarted;
    public AudioClip hackFinished;
    public AudioClip hackInProgress;
    public Action<HackController> OnValueChanged { get; set; }
    public float hackInProgressTimer;
    public List<HackData> targets = new List<HackData>();
    public void HandleHackInput(HackInput input) {
        if (targets.Count >= GameManager.I.gameData.playerData.maxConcurrentNetworkHacks)
            return;
        if (!targets.Any(t => t.node == input.targetNode)) {
            // Debug.Log("handling hack input");
            HackData data = new HackData {
                node = input.targetNode,
                timer = 0f,
                lifetime = 5f
            };
            targets.Add(data);
            OnValueChanged?.Invoke(this);
            audioSource.PlayOneShot(hackStarted);
        }
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
        }
        if (targets.Count > 0) {
            targets = targets.Except(done).ToList();
            OnValueChanged?.Invoke(this);
            hackInProgressTimer += Time.deltaTime;
            if (hackInProgressTimer > 1f) {
                audioSource.PlayOneShot(hackInProgress);
                hackInProgressTimer = 0f;
            }
        } else {
            hackInProgressTimer = 0f;
        }
    }
}
