using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SuspicionVisualUIHandler : MonoBehaviour {
    public Sprite normalAppearance;
    public Sprite suspiciousAppearance;
    public Sprite alarmAppearance;
    public Suspiciousness appearance;
    public Image image;
    public void HandleValueChange(SuspicionData data, SuspicionUIHandler parent) {
        // TODO: fix
        appearance = data.playerActivity();
        switch (appearance) {
            case Suspiciousness.normal:
                image.sprite = normalAppearance;
                image.color = parent.normalColor;
                break;
            case Suspiciousness.suspicious:
                image.sprite = suspiciousAppearance;
                image.color = parent.warnColor;
                break;
            case Suspiciousness.aggressive:
                image.sprite = alarmAppearance;
                image.color = parent.alertColor;
                break;
        }
    }
}
