using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SuspicionAppeaeranceUIHandler : MonoBehaviour {
    public Sprite disguiseAppearance;
    public Sprite normalAppearance;
    public Sprite gunAppearance;
    public Suspiciousness appearance;
    public Image image;

    public void HandleValueChange(SuspicionData data, SuspicionUIHandler parent) {
        appearance = data.appearanceSuspicion;
        switch (appearance) {
            case Suspiciousness.normal:
                image.sprite = normalAppearance;
                image.color = parent.normalColor;
                break;
            case Suspiciousness.suspicious:
                image.sprite = gunAppearance;
                image.color = parent.warnColor;
                break;
            case Suspiciousness.aggressive:
                image.sprite = gunAppearance;
                image.color = parent.alertColor;
                break;
        }
    }
}
