using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// WTH is up with this name? this is the only object named like this. call it a controller or handler please.
public class DialogueTextPackage : MonoBehaviour {
    public RectTransform container;
    public TextMeshProUGUI text;
    public GameObject leftPadding;
    public GameObject rightPadding;
    public void Initialize(string content, bool left) {
        text.text = content;
        leftPadding.SetActive(!left);
        rightPadding.SetActive(left);
    }
}
