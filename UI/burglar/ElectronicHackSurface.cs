using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElectronicHackSurface : AttackSurfaceElement, INodeBinder<PowerNode>, INodeBinder<CyberNode>, INodeBinder<AlarmNode> {
    PowerNode INodeBinder<PowerNode>.node { get; set; }
    CyberNode INodeBinder<CyberNode>.node { get; set; }
    AlarmNode INodeBinder<AlarmNode>.node { get; set; }
    public List<AttackSurfaceGraphWire> cyberWires;
    public List<AttackSurfaceGraphWire> alarmWires;
    public List<AttackSurfaceGraphWire> powerWires;
    public Camera myCamera;
    [HideInInspector]
    public RenderTexture renderTexture;
    void INodeBinder<CyberNode>.HandleNodeChange() {

    }
    void INodeBinder<CyberNode>.NodeBindInitialize() {
        CyberNode node = ((INodeBinder<CyberNode>)this).node;
        List<CyberNode> neighbors = GameManager.I.gameData.levelState.delta.cyberGraph.Neighbors(node); // 0, 1
        List<AttackSurfaceGraphWire> wires = cyberWires.OrderBy(item => Random.Range(0f, 1f)).ToList();
        for (int i = 0; i < wires.Count; i++) {
            if (i < neighbors.Count) {
                wires[i].gameObject.SetActive(true);
                wires[i].Initialize(node.idn, neighbors[i].idn);
            } else {
                wires[i].gameObject.SetActive(false);
            }
        }
    }
    void INodeBinder<PowerNode>.HandleNodeChange() {

    }
    void INodeBinder<PowerNode>.NodeBindInitialize() {
        PowerNode node = ((INodeBinder<PowerNode>)this).node;
        List<PowerNode> neighbors = GameManager.I.gameData.levelState.delta.powerGraph.Neighbors(node); // 0, 1
        List<AttackSurfaceGraphWire> wires = powerWires.OrderBy(item => Random.Range(0f, 1f)).ToList();

        for (int i = 0; i < wires.Count; i++) {
            if (i < neighbors.Count) {
                wires[i].gameObject.SetActive(true);
                wires[i].Initialize(node.idn, neighbors[i].idn);
            } else {
                wires[i].gameObject.SetActive(false);
            }
        }
    }
    void INodeBinder<AlarmNode>.HandleNodeChange() {

    }
    void INodeBinder<AlarmNode>.NodeBindInitialize() {
        AlarmNode node = ((INodeBinder<AlarmNode>)this).node;
        List<AlarmNode> neighbors = GameManager.I.gameData.levelState.delta.alarmGraph.Neighbors(node); // 0, 1
        List<AttackSurfaceGraphWire> wires = alarmWires.OrderBy(item => Random.Range(0f, 1f)).ToList();
        for (int i = 0; i < wires.Count; i++) {
            if (i < neighbors.Count) {
                wires[i].gameObject.SetActive(true);
                wires[i].Initialize(node.idn, neighbors[i].idn);
            } else {
                wires[i].gameObject.SetActive(false);
            }
        }
    }
    public void Start() {
        myCamera.enabled = false;
        renderTexture = new RenderTexture(1250, 750, 16, RenderTextureFormat.Default);
        myCamera.targetTexture = renderTexture;
    }

    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleSingleClick(activeTool, data);
        return BurglarAttackResult.None with {
            changeCamera = myCamera
        };
    }
}
