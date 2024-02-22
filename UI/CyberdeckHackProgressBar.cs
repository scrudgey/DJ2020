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
    public RectTransform progressBarOpposite;
    public TextMeshProUGUI caption;
    public TextMeshProUGUI targetNameCaption;
    public GraphIconReference icons;
    [Header("left")]
    public GameObject leftIconObject;
    public Image leftIcon;
    public Image leftNodeIcon;
    public TextMeshProUGUI leftNameCaption;
    public Sprite playerSprite;

    public void Initialize(NetworkAction action) {
        this.networkAction = action;
        // if (action.effect.type == SoftwareEffect.Type.download) {
        // progressBar.gameObject.SetActive(false);
        // } else {
        progressBarOpposite.gameObject.SetActive(false);
        // }

        targetIcon.sprite = icons.CyberNodeSprite(action.toNode);
        targetNameCaption.text = action.toNode.nodeTitle;

        if (action.fromPlayerNode) {
            leftIconObject.SetActive(false);
            leftIcon.enabled = true;
            leftIcon.sprite = playerSprite;
        } else {
            leftIconObject.SetActive(true);
            leftIcon.enabled = false;
            leftNodeIcon.sprite = icons.CyberNodeSprite(action.path[action.path.Count - 1]);
            leftNameCaption.text = action.path[action.path.Count - 1].nodeTitle;
        }
    }
    public void HandleNetworkActionChange(NetworkAction action) {
        this.networkAction = action;
        float progress = action.timer / action.lifetime;
        float width = progressParent.rect.width * progress;
        caption.text = action.title;

        progressBar.sizeDelta = new Vector2(width, 20f);
        progressBarOpposite.sizeDelta = new Vector2(width, 20f);

    }
}
