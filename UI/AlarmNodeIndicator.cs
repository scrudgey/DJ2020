using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AlarmNodeIndicator : NodeIndicator<AlarmNode, AlarmGraph> {

    public GameObject timerObject;
    public RectTransform timerRect;
    public RectTransform bkgRect;

    protected override void SetGraphicalState(AlarmNode node) {
        if (node.alarmTriggered) {
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
