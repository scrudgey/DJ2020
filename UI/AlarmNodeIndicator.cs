using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AlarmNodeIndicator : NodeIndicator<AlarmNode, AlarmGraph> {
    public Color deadColor = Color.red;
    public GameObject timerObject;
    public RectTransform timerRect;
    public RectTransform bkgRect;

    protected override void SetGraphicalState(AlarmNode node) {
        if (!node.enabled) {
            image.color = deadColor;
        } else if (node.alarmTriggered) {
            image.color = enabledColor;
            timerRect.sizeDelta = new Vector2(node.countdownTimer / 30f * bkgRect.rect.width, 1f);
        } else {
            image.color = disabledColor;
        }
        if (node.countdownTimer > 0f) {
            timerObject.SetActive(true);
        } else {
            timerObject.SetActive(false);
        }
    }
}
