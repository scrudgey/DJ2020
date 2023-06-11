using System;
using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CyberdeckCanvasController : MonoBehaviour {
    public TextMeshProUGUI mainText;
    [Header("bodies")]
    public GameObject bodyText;
    public GameObject bodyDetect;
    public GameObject bodyMenu;
    public GameObject bodyProgress;
    [Header("progress")]
    public RectTransform progressBarParent;
    public RectTransform progressBar;
    public TextMeshProUGUI progressText1;
    public TextMeshProUGUI progressText2;
    [Header("software")]
    public GameObject virusButton;
    public GameObject downloadButton;
    CyberDataStore attachedDataStore;
    CyberComponent targetComponent;
    // CyberDataStore attachedDataStore;
    HackData currentHackData;
    void Start() {
        bodyText.SetActive(true);
        bodyDetect.SetActive(false);
        bodyMenu.SetActive(false);
        bodyProgress.SetActive(false);
        StartCoroutine(AnimateMainText());
        progressText2.enabled = false;
        progressText1.enabled = true;
    }

    void Update() {
        if (currentHackData != null) {
            float totalWidth = progressBarParent.rect.width;
            float progress = currentHackData.timer / currentHackData.lifetime;
            progress = (float)PennerDoubleAnimation.QuintEaseInOut(progress, 0f, 1f, 1f);
            float width = progress * totalWidth;
            progressBar.sizeDelta = new Vector2(width, 1f);
            int progressInt = (int)(progress * 100);
            progressText2.text = $"{progressInt} %";
            if (progress >= 1) {
                ShowAttackComplete();
            }
        } else {
            progressBar.sizeDelta = new Vector2(0f, 1f);
        }
    }

    void ShowAttackComplete() {
        if (attachedDataStore != null) {
            ShowDownloadComplete();
        } else {
            ShowUploadComplete();
        }
        currentHackData = null;
        attachedDataStore = null;
        targetComponent = null;
    }

    void ShowDownloadComplete() {
        progressBarParent.gameObject.SetActive(false);
        progressText1.enabled = true;
        progressText2.enabled = true;
        progressText1.text = "Downloaded file:";
        if (attachedDataStore != null)
            progressText2.text = attachedDataStore.payDatas[0].filename;
    }
    void ShowUploadComplete() {
        progressBarParent.gameObject.SetActive(false);
        progressText1.enabled = true;
        progressText2.enabled = true;
        progressText1.text = "Payload complete";
        progressText2.text = "virus.exe";
    }

    public void HandleConnection(BurglarAttackResult result) {
        if (result != null) {
            attachedDataStore = result.attachedDataStore;
            targetComponent = result.attachedCyberComponent;
        }
        if (attachedDataStore != null || targetComponent != null) {
            StartCoroutine(BlinkDetect((bool value) => bodyDetect.SetActive(value), ShowBodyMenu));
        } else {
            ShowBodyText();
        }
    }

    void ShowBodyText() {
        bodyText.SetActive(true);
        bodyDetect.SetActive(false);
        bodyMenu.SetActive(false);
        bodyProgress.SetActive(false);
    }

    void ShowDetectText() {
        bodyText.SetActive(false);
        bodyDetect.SetActive(true);
        bodyMenu.SetActive(false);
        bodyProgress.SetActive(false);
    }

    void ShowBodyMenu() {
        bodyText.SetActive(false);
        bodyDetect.SetActive(false);
        bodyMenu.SetActive(true);
        bodyProgress.SetActive(false);
        ShowSoftwareButtons();
    }

    void ShowSoftwareButtons() {
        if (attachedDataStore != null) {
            virusButton.SetActive(false);
            downloadButton.SetActive(true);
        } else if (targetComponent != null) {
            virusButton.SetActive(true);
            downloadButton.SetActive(false);
        }
    }

    void ShowDownloadProgress() {
        bodyText.SetActive(false);
        bodyDetect.SetActive(false);
        bodyMenu.SetActive(false);
        bodyProgress.SetActive(true);
    }

    IEnumerator BlinkDetect(Action<bool> handleValue, Action callback) {
        ShowDetectText();

        float timer = 0f;
        float duration = 0.5f;
        float blinkTimer = 0f;
        float blinkInterval = 0.03f;
        float hangtime = 1f;
        bool value = true;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            blinkTimer += Time.unscaledDeltaTime;
            if (blinkTimer > blinkInterval) {
                blinkTimer -= blinkInterval;
                // bodyDetect.SetActive(!bodyDetect.activeInHierarchy);
                value = !value;
                handleValue(value);
            }
            yield return null;
        }
        bodyDetect.SetActive(true);
        timer = 0f;
        while (timer < hangtime) {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        callback?.Invoke();
    }

    IEnumerator AnimateMainText() {
        float timer = 0f;
        float duration = 0.5f;
        int index = 0;
        while (true) {
            timer += Time.unscaledDeltaTime;
            if (timer > duration) {
                timer -= duration;
                index += 1;
                if (index > 3) index = 0;
                mainText.text = index switch {
                    0 => "Scanning",
                    1 => "Scanning.",
                    2 => "Scanning..",
                    3 => "Scanning...",
                    _ => "Scanning"
                };
            }
            yield return null;
        }
    }


    public void DataThiefButtonCallback() {
        SetUpProgressBar("Downloading...", attachedDataStore.cyberComponent.GetNode());
    }

    public void CameraHackButtonCallback() {
        SetUpProgressBar("Uploading virus...", targetComponent.GetNode());
    }

    void SetUpProgressBar(string progressTitle, CyberNode targetNode) {
        HackInput hackInput = new HackInput() {
            targetNode = targetNode,
            type = HackType.manual
        };

        currentHackData = HackController.I.HandleHackInput(hackInput);

        progressBarParent.gameObject.SetActive(true);
        progressText1.enabled = true;
        progressText1.text = progressTitle;
        // progressText2.enabled = false;
        progressText2.enabled = true;
        progressText2.text = "0%";
        ShowDownloadProgress();
    }
}
