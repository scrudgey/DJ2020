using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class SuspicionStatusRecordIndicatorHandler : MonoBehaviour {
    public SuspicionRecord suspicionRecord;
    public TextMeshProUGUI dotText;
    public TextMeshProUGUI contentText;
    public RectTransform rectTransform;
    public RectTransform lifetimeFillBarTransform;
    public Color green;
    public Color yellow;
    public Color red;
    float timer;
    bool easeIn;
    float easeInDuration = 0.1f;
    public void Configure(SuspicionRecord record, bool newRecord = false) {
        this.suspicionRecord = record;
        dotText.color = record.suspiciousness switch {
            Suspiciousness.normal => green,
            Suspiciousness.suspicious => yellow,
            Suspiciousness.aggressive => red,
            _ => green
        };
        contentText.text = record.content;
        if (newRecord) StartEaseIn();
    }
    void StartEaseIn() {
        timer = 0f;
        easeIn = true;
        lifetimeFillBarTransform.sizeDelta = new Vector2(0f, 1f);
    }
    void Update() {
        if (easeIn) {
            if (timer < easeInDuration) {
                float scaleFactor = (float)PennerDoubleAnimation.Linear(timer, 0f, 1f, easeInDuration);
                Vector3 scale = new Vector3(1f, scaleFactor, 1f);
                rectTransform.localScale = scale;
                timer += Time.unscaledDeltaTime;
            } else {
                easeIn = false;
                rectTransform.localScale = Vector3.one;
            }
        }
        if (suspicionRecord.IsTimed()) {
            float totalWidth = rectTransform.rect.width;
            float fillWidth = (suspicionRecord.lifetime / suspicionRecord.maxLifetime) * totalWidth;
            lifetimeFillBarTransform.sizeDelta = new Vector2(fillWidth, 1f);
        } else {
            lifetimeFillBarTransform.sizeDelta = new Vector2(0f, 0f);
        }
    }
}
