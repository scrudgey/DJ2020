using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SoftwareButton : MonoBehaviour {
    public Image icon;
    public TextMeshProUGUI caption;
    public TextMeshProUGUI levelCaption;
    public void Initialize(SoftwareState state) {
        icon.sprite = state.template.icon;
        levelCaption.text = state.template.infiniteCharges ? "-" : $"{state.charges}";
    }
    public void Initialize(SoftwareTemplate template) {
        icon.sprite = template.icon;
        levelCaption.text = $"{template.maxCharges}";
    }
    // public void Configure(CyberNode node, CyberGraph graph) {
    //     bool targetIsDatastore = node.type == CyberNodeType.datanode;
    //     bool targetIsLocked = node.lockLevel > 0;
    //     bool targetIsUnknown = node.visibility < NodeVisibility.known;
    //     bool targetHasUnknownEdges = graph.edges[node.idn].Select(neighborId => (node.idn, neighborId)).Any(edge => graph.edgeVisibility[edge] == EdgeVisibility.unknown);
    //     bool dataIsStolen = node.dataStolen;
    //     bool nearestCompromised = graph.GetNearestCompromisedNode(node) != null || node.isManualHackerTarget;
    //     switch (effect.type) {
    //         case SoftwareEffect.Type.scan:
    //             button.interactable = targetIsUnknown || targetHasUnknownEdges;
    //             break;
    //         case SoftwareEffect.Type.download:
    //             button.interactable = targetIsDatastore && !targetIsLocked & !dataIsStolen && nearestCompromised;
    //             break;
    //         case SoftwareEffect.Type.unlock:
    //             button.interactable = targetIsLocked && nearestCompromised;
    //             break;
    //         case SoftwareEffect.Type.compromise:
    //             button.interactable = !targetIsLocked && nearestCompromised;
    //             break;
    //     }
    // }
}

