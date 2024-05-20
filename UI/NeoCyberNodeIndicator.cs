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
    public GameObject lockWidget2;
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
    [Header("hack stuff")]
    public GameObject hackButton;
    public GameObject hackOriginIndicator;
    public GameObject stopHackButton;
    void Start() {
        SetHackOrigin(false);
        hackButton.SetActive(false);
    }
    void SetEffect(SoftwareEffect.Type type) {
        scanEffectImage.enabled = false;
        downloadEffectImage.enabled = false;
        unlockEffectImage.enabled = false;
        crackEffectImage.enabled = false;
        switch (type) {
            default:
            case SoftwareEffect.Type.none:
                break;
            case SoftwareEffect.Type.scanAll:
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
        CyberNodeStatus nodeStatus = node.getStatus();

        iconImage.sprite = icons.CyberNodeSprite(node);
        if (node.type == CyberNodeType.datanode && node.dataStolen) {
            iconImage.enabled = false;
        }

        Color nodeColor = nodeStatus switch {
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
        // lockWidget2.SetActive(node.lockLevel > 0);
        lockWidget2.SetActive(false);

        // if (overlayHandler.selectedCyberNodeIndicator != null &&
        //     nodeStatus == CyberNodeStatus.compromised &&
        //     overlayHandler.selectedHackOrigin != this &&
        //     overlayHandler.selectedCyberNodeIndicator != this) {
        //     List<CyberNode> path = graph.GetPath(node, overlayHandler.selectedCyberNodeIndicator.node);
        //     if (path.Count <= 3) {
        //         hackButton.SetActive(true);
        //     } else hackButton.SetActive(false);
        // } else {
        //     hackButton.SetActive(false);
        // }

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
            SetEffect(networkAction.softwareTemplate.principalType);
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
        if (!node.notClickable) {
            if (GameManager.I.activeOverlayType == OverlayType.limitedCyber) {
                GameManager.I.SetOverlay(OverlayType.cyber);
            }
            base.OnPointerClick(pointerEventData);
        }
    }

    public override void OnPointerEnter(PointerEventData eventData) {
        if (!node.notClickable) {
            base.OnPointerEnter(eventData);
            if (GameManager.I.activeOverlayType == OverlayType.limitedCyber) {
                GameManager.I.playerManualHacker.Connect(node);
            } else if (GameManager.I.activeOverlayType == OverlayType.cyber && GameManager.I.playerManualHacker.targetNode == null) {
                GameManager.I.playerManualHacker.Connect(node);
            }
        }
    }
    public override void OnPointerExit(PointerEventData eventData) {
        if (!node.notClickable) {
            base.OnPointerExit(eventData);
            if (GameManager.I.activeOverlayType == OverlayType.limitedCyber) {
                GameManager.I.playerManualHacker.Disconnect();
            } else if (GameManager.I.activeOverlayType == OverlayType.cyber && GameManager.I.uiController.overlayHandler.selectedNode == null) {
                GameManager.I.playerManualHacker.Disconnect();
            }
        }
    }

    void ShowProgressBar(bool visible) {
        progressBarObject.SetActive(visible);
    }
    void SetProgress(float progress) {
        float width = progressBarParent.rect.width * progress;
        progressBarRect.sizeDelta = new Vector2(width, 50f);
    }
    public void SetHackOrigin(bool value) {
        hackOriginIndicator.SetActive(value);
        // hackButton.SetActive(!value);
        stopHackButton.SetActive(value);
    }
    // public void HackButtonCallback() {
    //     overlayHandler.HackOriginSelectCallback(this);
    // }
    // public void StophackButtonCallback() {
    //     overlayHandler.HackOriginSelectCallback(null);
    // }
}
