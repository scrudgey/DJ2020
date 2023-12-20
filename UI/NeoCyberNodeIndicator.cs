using System;
using System.Collections;
using System.Collections.Generic;
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
    protected override void SetGraphicalState(CyberNode node) {
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

        Color nodeColor = node.status switch {
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
    }
}
