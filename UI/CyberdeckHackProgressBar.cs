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
    [Header("sprites")]
    public Sprite normalIcon;
    public Sprite wanIcon;
    public Sprite datastoreIcon;
    public Sprite utilityIcon;
    public Sprite mysteryIcon;
    public void Initialize(NetworkAction action) {
        this.networkAction = action;
    }
    public void HandleNetworkActionChange(NetworkAction action) {
        // caption.text = action.
        this.networkAction = action;
        float progress = action.timer / action.lifetime;
        float width = progressParent.rect.width * progress;
        progressBar.sizeDelta = new Vector2(width, 20f);
        // targetIcon.sprite = action.toNode.
    }
}
