using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CyberNodeIndicatorDataWidget : MonoBehaviour {
    public Image icon;
    public void SetColor(Color color) {
        icon.color = color;
    }
}
