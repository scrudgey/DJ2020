using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SoftwareButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public bool initializeOnStart;
    public CyberdeckUIController cyberdeckController;
    public Button button;
    public SoftwareEffect effect;
    [Header("UI")]
    public Sprite scanIcon;
    public Sprite downloadIcon;
    public Sprite unlockIcon;
    public Sprite compromiseIcon;
    public Image icon;
    public TextMeshProUGUI caption;
    public TextMeshProUGUI levelCaption;
    void Start() {
        if (initializeOnStart) {
            Initialize(cyberdeckController);
        }
    }
    public void Initialize(CyberdeckUIController cyberdeckController) {
        this.cyberdeckController = cyberdeckController;
        switch (effect.type) {
            case SoftwareEffect.Type.scan:
                icon.sprite = scanIcon;
                break;
            case SoftwareEffect.Type.download:
                icon.sprite = downloadIcon;
                break;
            case SoftwareEffect.Type.unlock:
                icon.sprite = unlockIcon;
                break;
            case SoftwareEffect.Type.compromise:
                icon.sprite = compromiseIcon;
                break;
        }
        caption.text = effect.name;
        levelCaption.text = effect.level.ToString();
    }

    public void Configure(CyberNode node, CyberGraph graph) {
        bool targetIsDatastore = node.type == CyberNodeType.datanode;
        bool targetIsLocked = node.lockLevel > 0;
        bool targetIsUnknown = node.visibility < NodeVisibility.mapped;
        bool dataIsStolen = node.dataStolen;
        bool nearestCompromised = graph.GetNearestCompromisedNode(node) != null || node.isManualHackerTarget;
        switch (effect.type) {
            case SoftwareEffect.Type.scan:
                button.interactable = targetIsUnknown;
                break;
            case SoftwareEffect.Type.download:
                button.interactable = targetIsDatastore && !targetIsLocked & !dataIsStolen && nearestCompromised;
                break;
            case SoftwareEffect.Type.unlock:
                button.interactable = targetIsLocked && nearestCompromised;
                break;
            case SoftwareEffect.Type.compromise:
                button.interactable = !targetIsLocked && nearestCompromised;
                break;
        }
    }
    public void OnClick() {
        cyberdeckController.SoftwareButtonCallback(this);
    }
    public void OnPointerEnter(PointerEventData eventData) {
        cyberdeckController.SoftwareButtonMouseover(this);
    }
    public void OnPointerExit(PointerEventData eventData) {
        cyberdeckController.SoftwareButtonMouseExit(this);
    }

    // TODO: this logic will belong somewhere else- player state
    public NetworkAction GetNetworkAction(CyberNode node, CyberGraph graph) {
        float lifetime = effect.type switch {
            SoftwareEffect.Type.compromise => 10f,
            SoftwareEffect.Type.download => 10f,
            SoftwareEffect.Type.scan => 3f,
            // SoftwareEffect.Type.unlock => 5f,
            SoftwareEffect.Type.unlock => 6f,
            _ => 1f
        };

        NetworkAction networkAction = new NetworkAction() {
            title = $"uploading {effect.name}...",
            effect = effect,
            lifetime = lifetime,
            toNode = node,
            timerRate = 1f,
            path = new List<CyberNode>(),
            payData = node.payData
        };
        if (effect.type == SoftwareEffect.Type.scan) {
            networkAction.path = graph.GetPathToNearestCompromised(node);
            if (node.isManualHackerTarget) { //|| networkAction.path[networkAction.path.Count - 1].isManualHackerTarget
                networkAction.fromPlayerNode = true;
            }

        } else if (effect.type == SoftwareEffect.Type.download) {
            networkAction.title = $"downloading {node.payData.filename}...";
            networkAction.path = graph.GetPathToNearestDownloadPoint(node);
            if (node.isManualHackerTarget) {
                networkAction.fromPlayerNode = true;
            }
        } else if (node.isManualHackerTarget) {
            networkAction.fromPlayerNode = true;
        } else {
            networkAction.path.Add(node);
            networkAction.path.Add(graph.GetNearestCompromisedNode(node));
        }
        // if (networkAction.path.Count > 1 && networkAction.path[networkAction.path.Count - 1].isManualHackerTarget) {
        //     networkAction.fromPlayerNode = true;
        // }

        return networkAction;
    }


}

