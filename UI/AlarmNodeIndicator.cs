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
    public GraphIconReference iconReference;
    public override void SetGraphicalState(AlarmNode node) {
        // iconImage.sprite = normalIcon;
        iconImage.sprite = iconReference.AlarmNodeSprite(node);
        if (!node.getEnabled()) {
            iconImage.color = deadColor;
        } else if (node.alarmTriggered) {
            iconImage.color = enabledColor;
            timerRect.sizeDelta = new Vector2(node.countdownTimer / 30f * bkgRect.rect.width, 1f);
        } else {
            iconImage.color = disabledColor;
        }

        if (node.countdownTimer > 0f) {
            timerObject.SetActive(true);
        } else {
            timerObject.SetActive(false);
        }
    }
}
