using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class NetworkAction {
    public string title;
    public SoftwareEffect effect;
    public float timer;
    public float lifetime;
    public CyberNode toNode;
    public bool fromPlayerNode;
    public List<CyberNode> path;
    public float timerRate = 1f;
    public bool complete;
    public PayData payData;
    public void Update(float deltaTime) {
        timer += timerRate * deltaTime;
        if (timer >= lifetime) {
            DoComplete();
        }
    }

    void DoComplete() {
        complete = true;
        effect.ApplyToNode(toNode);
    }
}