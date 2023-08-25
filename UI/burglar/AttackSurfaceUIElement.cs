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
    public RectTransform containerRectTransform;
    public BurgleTargetData data;
    public Image[] buttonImages;
    public UIProgressPip[] progressPips;

    public void Initialize(BurglarCanvasController controller, AttackSurfaceElement element) {
        this.controller = controller;
        this.element = element;
        HideProgress();
        SetPosition();
    }

    // TO CHANGE
    // public void ClickCallback() {
    //     controller.ClickCallback(element);
    // }
    // public void ClickDownCallback() {
    //     controller.ClickDownCallback(element);
    // }
    // public void MouseOverCallback() {
    //     controller.MouseOverUIElementCallback(element);
    // }
    // public void MouseExitCallback() {
    //     controller.MouseExitUIElementCallback(element);
    // }

    public void ShowProgress() {
        progressBarObject.SetActive(true);
    }
    public void HideProgress() {
        progressBarObject.SetActive(false);
        for (int i = 0; i < progressPips.Length; i++) {
            // progressPips[i].gameObject.SetActive(false);
            // progressPips[i].SetProgress(false, false);
            progressPips[i].Hide();
        }
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
        if (element == null || gameObject == null) return;
        if (element.complete) {
            BlinkCompletePips();
            element.complete = false;
            return;
        }
        SetPosition();
        if (element.progressPercent > 0 || element.progressStageIndex > 0 || element.engaged) {
            SetPipVisibility(element.progressStages);
            SetPipProgress(element.progressPercent, element.progressStages, element.progressStageIndex);
            // ShowProgress();
            // SetProgress(element.progressPercent, element.progressStages, element.progressStageIndex);
        } else {
            HideProgress();
        }
    }

    void SetPosition() {
        if (element == null) return;
        Rect bounds = Toolbox.GetTotalRenderBoundingBox(element.transform, data.target.attackCam, adjustYScale: false);

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;

        Vector3 center = Toolbox.GetBoundsCenter(element.transform);
        Vector3 position = data.target.attackCam.WorldToViewportPoint(center);
        position.x *= containerRectTransform.rect.width;
        position.y *= containerRectTransform.rect.height;

        // containerRectTransform.
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(bounds.width, bounds.height);

    }

    void SetPipVisibility(int number) {
        for (int i = 0; i < progressPips.Length; i++) {
            progressPips[i].gameObject.SetActive(i < number);
            // progressPips[i].SetProgress(false, false);
        }
    }

    void SetPipProgress(float percent, int progressStages, int stageIndex) {
        // Debug.Log($"{percent} {progressStages} {stageIndex}");
        for (int i = 0; i < stageIndex; i++) {
            progressPips[i].SetProgress(false, true);
        }
        if (percent > 0) {
            progressPips[stageIndex].SetProgress(true, false);
        } else {
            progressPips[stageIndex].SetProgress(false, false);
        }

        for (int i = stageIndex + 1; i < progressPips.Length; i++) {
            progressPips[i].SetProgress(false, false);
        }
    }

    public void HandleElementDestroyed() {
        if (gameObject != null)
            Destroy(gameObject);
    }

    public void BlinkCompletePips() {
        foreach (UIProgressPip pip in progressPips) {
            pip.BlinkComplete();
        }
    }
}
