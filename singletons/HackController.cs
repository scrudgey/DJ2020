using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class HackInput {
    public CyberNode targetNode;
}
public class HackController : Singleton<HackController> {
    public class HackData {
        public CyberNode node;
        public float timer;
        public float lifetime;
    }
    public Action OnValueChanged { get; set; }

    // public CyberNode currentHackTarget;
    public float currentHackTimer;
    public List<HackData> targets = new List<HackData>();
    public void HandleHackInput(HackInput input) {
        if (!targets.Any(t => t.node == input.targetNode)) {
            Debug.Log("handling hack input");
            HackData data = new HackData {
                node = input.targetNode,
                timer = 0f,
                lifetime = 2f
            };
            targets.Add(data);
            OnValueChanged?.Invoke();
        }
    }

    void Update() {
        List<HackData> done = new List<HackData>();
        foreach (HackData data in targets) {
            data.timer += Time.deltaTime;
            if (data.timer > data.lifetime) {
                GameManager.I.SetCyberNodeState(data.node, true);
                done.Add(data);
            }
        }
        if (targets.Count > 0) {
            targets = targets.Except(done).ToList();
            OnValueChanged?.Invoke();
        }
    }
}
