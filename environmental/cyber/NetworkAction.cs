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
    public SoftwareTemplate softwareTemplate;
    public float timer;
    public float lifetime;
    public CyberNode toNode;
    public List<CyberNode> path;
    public float timerRate = 1f;
    public bool complete;
    public PayData payData;
    public void Update(float deltaTime, CyberGraph graph) {
        timer += timerRate * deltaTime;
        if (timer >= lifetime) {
            DoComplete(graph);
        }
    }

    void DoComplete(CyberGraph graph) {
        complete = true;
        if (softwareTemplate.softwareType == SoftwareTemplate.SoftwareType.virus) {
            graph.AddVirusProgram(softwareTemplate.ToVirusProgram(toNode, graph));
        } else {
            foreach (SoftwareEffect effect in softwareTemplate.effects) {
                effect.ApplyToNode(toNode, graph);
            }
        }
    }
}