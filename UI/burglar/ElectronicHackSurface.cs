using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElectronicHackSurface : AttackSurfaceElement, INodeBinder<PowerNode>, INodeBinder<CyberNode>, INodeBinder<AlarmNode> {
    PowerNode INodeBinder<PowerNode>.node { get; set; }
    CyberNode INodeBinder<CyberNode>.node { get; set; }
    AlarmNode INodeBinder<AlarmNode>.node { get; set; }
    [Header("config")]
    public DoorLock doorlockChipTarget;
    [Header("circuits")]
    public List<CircuitLayoutView> circuitLayouts;
    CircuitLayoutView selectedCircuitLayout;

    public Camera myCamera;
    [HideInInspector]
    public RenderTexture renderTexture;


    void MaybeSelectCircuit() {
        if (selectedCircuitLayout == null) {
            selectedCircuitLayout = Toolbox.RandomFromList(circuitLayouts);
        }
    }
    void INodeBinder<CyberNode>.HandleNodeChange() {

    }
    void INodeBinder<PowerNode>.HandleNodeChange() {

    }
    void INodeBinder<AlarmNode>.HandleNodeChange() {

    }

    void INodeBinder<CyberNode>.NodeBindInitialize() {
        CyberNode node = ((INodeBinder<CyberNode>)this).node;
        MaybeSelectCircuit();
        selectedCircuitLayout.InitializeCyber(node);
    }

    void INodeBinder<PowerNode>.NodeBindInitialize() {
        PowerNode node = ((INodeBinder<PowerNode>)this).node;
        MaybeSelectCircuit();
        selectedCircuitLayout.InitializePower(node);
    }

    void INodeBinder<AlarmNode>.NodeBindInitialize() {
        AlarmNode node = ((INodeBinder<AlarmNode>)this).node;
        MaybeSelectCircuit();
        selectedCircuitLayout.InitializeAlarm(node);
    }
    public void Start() {
        MaybeSelectCircuit();
        selectedCircuitLayout.InitializeDoorlockChip(doorlockChipTarget);

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
