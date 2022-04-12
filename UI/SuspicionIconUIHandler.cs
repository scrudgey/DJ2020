using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SuspicionIconUIHandler : MonoBehaviour {
    public SuspicionUIHandler parent;
    public Sprite normalAppearance;
    public Sprite warnAppearance;
    public Sprite alarmAppearance;
    public Image image;
    public void Bind(SuspicionUIHandler parent) {
        this.parent = parent;
    }
    public void UpdateImage(Suspiciousness suspiciousness) {
        switch (suspiciousness) {
            case Suspiciousness.normal:
                image.sprite = normalAppearance;
                image.color = parent.normalColor;
                break;
            case Suspiciousness.suspicious:
                image.sprite = warnAppearance;
                image.color = parent.warnColor;
                break;
            case Suspiciousness.aggressive:
                image.sprite = alarmAppearance;
                image.color = parent.alertColor;
                break;
        }
    }
}
