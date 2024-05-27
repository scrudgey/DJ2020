using System.Collections.Generic;
using Easings;
using UnityEngine;
public class VirusProgram {
    enum State { wait, move }

    public CyberGraph graph;
    public Vector3 position;
    public CyberNode currentNode;
    public int maxHops;
    public int hops;
    public int duplication;
    public float timer;
    public List<SoftwareEffect> effects;

    public bool updated;
    public bool complete;
    public bool duplicate;

    public LoHi waitAtNodeTimeRange;
    public LoHi transitTimeRange;

    float waitAtNodeTime;
    float transitTime;

    CyberNode toNode;
    State state;
    Vector3 displacementNormalized;
    float displacementMagnitude;

    public void Update(float deltaTime) {
        timer += deltaTime;

        if (waitAtNodeTime == 0) waitAtNodeTime = waitAtNodeTimeRange.GetRandomInsideBound();
        if (transitTime == 0) transitTime = transitTimeRange.GetRandomInsideBound();

        if (state == State.wait) {
            if (timer > waitAtNodeTime) {
                timer = 0;
                updated = true;

                foreach (SoftwareEffect effect in effects) {
                    effect.ApplyToNode(currentNode, graph);
                }

                if (hops <= 0) {
                    // Debug.Log($"virus program complete");
                    complete = true;
                } else {
                    // Debug.Log($"virus program hopping: {hops}");
                    hops--;
                    SetDestination();
                    if (duplication > 0) {
                        duplicate = true;
                    }
                }
            }
        } else if (state == State.move) {
            if (timer < transitTime) {
                position = currentNode.position + (float)PennerDoubleAnimation.Linear(timer, 0, displacementMagnitude, transitTime) * displacementNormalized;
            } else {
                updated = true;
                position = toNode.position;
                state = State.wait;
                currentNode = toNode;
                timer = 0f;
                waitAtNodeTime = waitAtNodeTimeRange.GetRandomInsideBound();
            }
        }
    }

    public void SetDestination() {
        state = State.move;
        toNode = Toolbox.RandomFromList(graph.Neighbors(currentNode));
        Vector3 displacement = (toNode.position - currentNode.position);
        displacementNormalized = displacement.normalized;
        displacementMagnitude = displacement.magnitude;
        transitTime = transitTimeRange.GetRandomInsideBound();
    }

    public VirusProgram Duplicate() {
        duplication--;

        VirusProgram newVirus = new VirusProgram() {
            graph = graph,
            position = position,
            currentNode = currentNode,
            hops = maxHops,
            maxHops = maxHops,
            duplication = duplication,
            timer = Random.Range(0, waitAtNodeTime),
            effects = effects,
            state = State.wait,
            waitAtNodeTimeRange = waitAtNodeTimeRange,
            transitTimeRange = transitTimeRange,
            waitAtNodeTime = waitAtNodeTimeRange.GetRandomInsideBound(),
            transitTime = transitTimeRange.GetRandomInsideBound()
        };
        // newVirus.SetDestination();
        // graph.AddVirusProgram(newVirus);
        return newVirus;
    }
}