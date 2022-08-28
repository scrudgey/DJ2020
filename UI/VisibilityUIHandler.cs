using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class VisibilityUIHandler : IBinder<LightLevelProbe> {
    public Color green;
    public Color yellow;
    public Color red;
    public Sprite emptyBox;
    public TextMeshProUGUI dotText;
    public Image[] images;
    float targetLightLevel;
    override public void HandleValueChanged(LightLevelProbe t) {
        int key = t.GetDiscreteLightLevel();
        targetLightLevel = Mathf.Lerp(targetLightLevel, (float)key, 0.1f);
        int targetActiveBlocks = (int)Math.Round(targetLightLevel, 0);
        for (int i = 0; i < images.Length; i++) {
            if (i < targetActiveBlocks) {
                images[i].sprite = null;
            } else {
                images[i].sprite = emptyBox;
            }
        }
        if (targetActiveBlocks < 2) {
            dotText.color = green;
        } else if (targetActiveBlocks < 4) {
            dotText.color = yellow;
        } else {
            dotText.color = red;
        }
    }
}
