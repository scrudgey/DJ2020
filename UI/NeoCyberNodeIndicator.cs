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
    public GraphIconReference icons;
    [Header("progress bar")]
    public GameObject progressBarObject;
    public RectTransform progressBarRect;
    public RectTransform progressBarParent;
    public Material lineMaterial;
    [Header("effects")]
    public Image scanEffectImage;
    public Animation scanEffectAnimator;

    public Image downloadEffectImage;

    public Image unlockEffectImage;
    public Animation unlockEffectAnimator;

    public Image crackEffectImage;

    void SetEffect(SoftwareEffect.Type type) {
        scanEffectImage.enabled = false;
        downloadEffectImage.enabled = false;
        unlockEffectImage.enabled = false;
        crackEffectImage.enabled = false;
        switch (type) {
            default:
            case SoftwareEffect.Type.none:
                break;
            case SoftwareEffect.Type.scan:
                scanEffectImage.enabled = true;
                scanEffectAnimator.Play("animateScanEffect");
                break;
            case SoftwareEffect.Type.download:
                downloadEffectImage.enabled = true;
                break;
            case SoftwareEffect.Type.unlock:
                unlockEffectImage.enabled = true;
                unlockEffectAnimator.Play("animateUnlockEffect");
                break;
            case SoftwareEffect.Type.compromise:
                crackEffectImage.enabled = true;
                break;
        }
    }
    public override void SetGraphicalState(CyberNode node) {
        if (node.visibility == NodeVisibility.mystery) {
            SetMysteryState(node);
            return;
        }
        iconImage.sprite = icons.CyberNodeSprite(node);
        if (node.type == CyberNodeType.datanode && node.dataStolen) {
            iconImage.enabled = false;
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
        lockWidget.gameObject.SetActive(false);
        dataWidget.gameObject.SetActive(false);
        // lockWidget.SetColor(nodeColor);
        // dataWidget.SetColor(nodeColor);

        // lockWidget.gameObject.SetActive(node.lockLevel > 0);
        // if (node.lockLevel > 0) {
        //     lockWidget.SetLockLevel(node.lockLevel);
        // }

        // if (node.type == CyberNodeType.datanode) {
        //     dataWidget.gameObject.SetActive(!node.dataStolen);
        // } else {
        //     dataWidget.gameObject.SetActive(false);
        // }

        SetNetworkActionProgress();
    }
    void SetNetworkActionProgress() {
        if (graph.networkActions != null && graph.networkActions.ContainsKey(node) && graph.networkActions[node].Count > 0) {
            ShowProgressBar(true);
            NetworkAction networkAction = graph.networkActions[node][0];
            float progess = networkAction.timer / networkAction.lifetime;
            SetProgress(progess);
            SetEffect(networkAction.effect.type);
        } else {
            ShowProgressBar(false);
            SetEffect(SoftwareEffect.Type.none);
        }
    }
    protected void SetMysteryState(CyberNode node) {
        iconImage.sprite = icons.CyberNodeSprite(node);

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
