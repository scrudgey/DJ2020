using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
[ExecuteAlways]
public class UIColorHandler : MonoBehaviour {
    public Color primaryColor;
    public Color backgroundColor;
    public float transparency;

    public Image[] primaryImages = new Image[0];
    public Image[] backgroundImages = new Image[0];
    public TextMeshProUGUI[] primaryTexts = new TextMeshProUGUI[0];
    public TextMeshProUGUI[] backgroundTexts = new TextMeshProUGUI[0];

    public UIColorHandler[] children;
    void Update() {
        Color primaryColorTransparent = new Color(primaryColor.r, primaryColor.g, primaryColor.b, transparency);
        Color backgroundColorTransparent = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, transparency);

        if (primaryImages != null)
            foreach (Image image in primaryImages) {
                if (image == null) continue;
                image.color = primaryColorTransparent;
            }
        if (backgroundImages != null)
            foreach (Image image in backgroundImages) {
                if (image == null) continue;
                image.color = backgroundColorTransparent;
            }
        if (primaryTexts != null)
            foreach (TextMeshProUGUI text in primaryTexts) {
                if (text == null) continue;
                text.color = primaryColor;
            }
        if (backgroundTexts != null)
            foreach (TextMeshProUGUI text in backgroundTexts) {
                if (text == null) continue;
                text.color = backgroundColor;
            }

        if (children != null)
            foreach (UIColorHandler child in children) {
                if (child != null) {
                    child.primaryColor = primaryColor;
                    child.backgroundColor = backgroundColor;
                    child.transparency = transparency;
                }
            }
    }
}
