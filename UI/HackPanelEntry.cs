using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class HackPanelEntry : MonoBehaviour {
    public TextMeshProUGUI textMesh;
    public RectTransform progressBar;
    public RectTransform parent;
    public void Clear() {
        textMesh.text = "";
    }
    public void Configure(HackController.HackData data) {
        textMesh.text = data.node.nodeTitle;
        float width = parent.rect.width * (data.timer / data.lifetime);
        progressBar.sizeDelta = new Vector2(width, 1f);
    }
}
