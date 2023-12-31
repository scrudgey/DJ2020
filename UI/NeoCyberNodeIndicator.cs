using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class NeoCyberNodeIndicator : NodeIndicator<CyberNode, CyberGraph> {
    public Image outlineImage;
    [Header("colors")]
    public Color invulnerableColor;
    public Color vulnerableColor;
    public Color compromisedColor;
    public Color mysteryColor;
    [Header("decor")]
    public CyberNodeIndicatorLockWidget lockWidget;
    public CyberNodeIndicatorDataWidget dataWidget;
    [Header("sprites")]
    public Sprite normalIcon;
    public Sprite wanIcon;
    public Sprite datastoreIcon;
    public Sprite utilityIcon;
    public Sprite mysteryIcon;
    [Header("progress bar")]
    public GameObject progressBarObject;
    public RectTransform progressBarRect;
    public RectTransform progressBarParent;
    public override void SetGraphicalState(CyberNode node) {
        if (node.visibility == NodeVisibility.mystery) {
            SetMysteryState(node);
            return;
        }
        switch (node.type) {
            case CyberNodeType.normal:
                iconImage.sprite = normalIcon;
                break;
            case CyberNodeType.datanode:
                iconImage.sprite = datastoreIcon;
                break;
            case CyberNodeType.utility:
                iconImage.sprite = utilityIcon;
                break;
            case CyberNodeType.WAN:
                iconImage.sprite = wanIcon;
                break;
        }

        Color nodeColor = node.getStatus() switch {
            CyberNodeStatus.invulnerable => invulnerableColor,
            CyberNodeStatus.vulnerable => vulnerableColor,
            CyberNodeStatus.compromised => compromisedColor,
            _ => invulnerableColor
        };
        if (!node.getEnabled()) nodeColor = disabledColor;

        iconImage.color = nodeColor;
        outlineImage.color = nodeColor;
        lockWidget.SetColor(nodeColor);
        dataWidget.SetColor(nodeColor);

        lockWidget.gameObject.SetActive(node.lockLevel > 0);
        if (node.lockLevel > 0) {
            lockWidget.SetLockLevel(node.lockLevel);
        }

        if (node.type == CyberNodeType.datanode) {
            dataWidget.gameObject.SetActive(!node.dataStolen);
        } else {
            dataWidget.gameObject.SetActive(false);
        }

        SetNetworkActionProgress();
    }
    void SetNetworkActionProgress() {
        if (graph.networkActions != null && graph.networkActions.ContainsKey(node) && graph.networkActions[node].Count > 0) {
            ShowProgressBar(true);
            NetworkAction networkAction = graph.networkActions[node][0];
            float progess = networkAction.timer / networkAction.lifetime;
            SetProgress(progess);
        } else {
            ShowProgressBar(false);
        }
    }
    protected void SetMysteryState(CyberNode node) {
        iconImage.sprite = mysteryIcon;

        Color nodeColor = Color.gray;

        iconImage.color = nodeColor;
        outlineImage.color = nodeColor;
        lockWidget.SetColor(nodeColor);
        dataWidget.SetColor(nodeColor);

        lockWidget.gameObject.SetActive(false);
        dataWidget.gameObject.SetActive(false);

        SetNetworkActionProgress();
    }

    public override void OnPointerClick(PointerEventData pointerEventData) {
        if (GameManager.I.activeOverlayType == OverlayType.limitedCyber) {
            GameManager.I.SetOverlay(OverlayType.cyber);
            // handle node connections
        }
        base.OnPointerClick(pointerEventData);
    }

    public override void OnPointerEnter(PointerEventData eventData) {
        base.OnPointerEnter(eventData);
        if (GameManager.I.activeOverlayType == OverlayType.limitedCyber) {
            GameManager.I.playerManualHacker.Connect(node);
        } else if (GameManager.I.activeOverlayType == OverlayType.cyber && GameManager.I.playerManualHacker.targetNode == null) {
            GameManager.I.playerManualHacker.Connect(node);
        }
    }
    public override void OnPointerExit(PointerEventData eventData) {
        base.OnPointerExit(eventData);
        if (GameManager.I.activeOverlayType == OverlayType.limitedCyber) {
            GameManager.I.playerManualHacker.Disconnect();
        } else if (GameManager.I.activeOverlayType == OverlayType.cyber && GameManager.I.uiController.overlayHandler.selectedNode == null) {
            GameManager.I.playerManualHacker.Disconnect();
        }
    }

    void ShowProgressBar(bool visible) {
        progressBarObject.SetActive(visible);
    }
    void SetProgress(float progress) {
        float width = progressBarParent.rect.width * progress;
        progressBarRect.sizeDelta = new Vector2(width, 50f);
    }
}
