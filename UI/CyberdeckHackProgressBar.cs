using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CyberdeckHackProgressBar : MonoBehaviour {
    public NetworkAction networkAction;
    public Image targetIcon;
    public RectTransform progressParent;
    public RectTransform progressBar;
    public TextMeshProUGUI caption;
    public TextMeshProUGUI targetNameCaption;
    public GraphIconReference icons;

    public void Initialize(NetworkAction action) {
        this.networkAction = action;
    }
    public void HandleNetworkActionChange(NetworkAction action) {
        this.networkAction = action;
        float progress = action.timer / action.lifetime;
        float width = progressParent.rect.width * progress;
        caption.text = action.title;
        progressBar.sizeDelta = new Vector2(width, 20f);
        targetIcon.sprite = icons.CyberNodeSprite(action.toNode);
        targetNameCaption.text = action.toNode.nodeTitle;
    }
}
