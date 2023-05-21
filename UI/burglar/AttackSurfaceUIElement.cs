using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AttackSurfaceUIElement : IBinder<AttackSurfaceElement> {
    BurglarCanvasController controller;
    AttackSurfaceElement element;
    public GameObject progressBarObject;
    public RectTransform rectTransform;
    public RectTransform progressBarParentRect;
    public RectTransform progressRect;
    public Image divider1;
    public Image divider2;
    public RectTransform dividerRect1;
    public RectTransform dividerRect2;

    public void Initialize(BurglarCanvasController controller, AttackSurfaceElement element) {
        this.controller = controller;
        this.element = element;
        HideProgress();
    }
    public void ClickCallback() {
        controller.ClickCallback(element);
    }
    public void ClickDownCallback() {
        controller.ClickDownCallback(element);
    }
    public void MouseOverCallback() {
        controller.MouseOverUIElementCallback(element);
    }
    public void MouseExitCallback() {
        controller.MouseExitUIElementCallback(element);
    }
    public void ShowProgress() {
        progressBarObject.SetActive(true);
    }
    public void HideProgress() {
        progressBarObject.SetActive(false);
    }
    public void SetProgress(float percent, int progressStages, int stageIndex) {
        SetProgressDimensions(progressStages, stageIndex);
        float totalWidth = progressBarParentRect.rect.width;
        float width = percent * totalWidth;
        progressRect.sizeDelta = new Vector2(width, 1f);
    }
    void SetProgressDimensions(int progressStages, int stageIndex) {
        float totalHeight = progressBarParentRect.rect.height;
        float totalWidth = progressStages switch {
            1 => 150f,
            2 => 200f,
            3 => 300f
        };
        progressBarParentRect.sizeDelta = new Vector2(totalWidth, totalHeight);
        if (progressStages == 1) {
            divider1.enabled = false;
            divider2.enabled = false;
        } else if (progressStages == 2) {
            divider1.enabled = true;
            divider2.enabled = false;
            dividerRect1.anchoredPosition = new Vector2(100f, 0f);
        } else if (progressStages == 3) {
            divider1.enabled = true;
            divider2.enabled = true;
            dividerRect1.anchoredPosition = new Vector2(100f, 0f);
            dividerRect2.anchoredPosition = new Vector2(200f, 0f);
        }
    }
    public override void HandleValueChanged(AttackSurfaceElement element) {
        if (element.progressPercent > 0) {
            ShowProgress();
            SetProgress(element.progressPercent, element.progressStages, element.progressStageIndex);
        } else {
            HideProgress();
        }
    }
}
