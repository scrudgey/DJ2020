using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSurfaceUIElement : IBinder<AttackSurfaceElement> {
    BurglarCanvasController controller;
    AttackSurfaceElement element;
    public GameObject progressBarObject;
    public RectTransform progressBarParentRect;
    public RectTransform progressRect;

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
    public void SetProgress(float percent) {
        float totalWidth = progressBarParentRect.rect.width;
        float width = percent * totalWidth;
        progressRect.sizeDelta = new Vector2(width, 1f);
    }
    public override void HandleValueChanged(AttackSurfaceElement element) {
        if (element.progressPercent > 0) {
            ShowProgress();
            SetProgress(element.progressPercent);
        } else {
            HideProgress();
        }
    }
}
