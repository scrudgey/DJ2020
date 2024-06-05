using System.Collections;
using System.Collections.Generic;
using Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutCyberdeckSlot : MonoBehaviour {
    public MissionPlanCyberController controller;
    public SoftwareButton softwareButton;
    public SoftwareTemplate template;
    public Image highlight;
    public bool isIntrinsicSoftware;
    Coroutine blinkHighlightRoutine;
    public void Initialize(MissionPlanCyberController controller) {
        this.controller = controller;
        highlight.enabled = false;
    }
    public void SetItem(SoftwareTemplate template) {
        this.template = template;
        if (template == null) {
            Clear();
        } else {
            softwareButton.gameObject.SetActive(true);
            softwareButton.Initialize(template);
        }
    }
    public void Clear() {
        template = null;
        softwareButton.gameObject.SetActive(false);
    }
    public void OnClick() {
        controller.SoftwareSlotClicked(this);
    }
    public void OnClearClick() {
        Clear();
        controller.SoftwareClearClicked(this);
    }

    public void ShowHighlight(bool value) {
        if (blinkHighlightRoutine != null) {
            StopCoroutine(blinkHighlightRoutine);
        }
        if (value) {
            blinkHighlightRoutine = StartCoroutine(doBlink());
        } else {
            highlight.enabled = false;
        }
    }
    IEnumerator doBlink() {
        float timer = 0f;
        float interval = 0.03f;
        while (true) {
            timer += Time.unscaledDeltaTime;
            if (timer > interval) {
                timer -= interval;
                highlight.enabled = !highlight.enabled;
            }
            yield return null;
        }
    }
}
