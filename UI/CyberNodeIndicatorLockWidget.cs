using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CyberNodeIndicatorLockWidget : MonoBehaviour {
    public Image[] lockIcons;
    public void SetLockLevel(int lockLevel) {
        for (int i = 0; i < lockIcons.Length; i++) {
            lockIcons[i].gameObject.SetActive(i < lockLevel);
        }
    }
    public void SetColor(Color color) {
        foreach (Image image in lockIcons) {
            image.color = color;
        }
    }
}
