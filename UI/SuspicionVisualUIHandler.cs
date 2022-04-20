using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SuspicionVisualUIHandler : MonoBehaviour {
    public SuspicionUIHandler parent;
    public Sprite normalAppearance;
    public Sprite suspiciousAppearance;
    public Sprite alarmAppearance;
    public Suspiciousness appearance;
    public Image image;
    public void Bind(SuspicionUIHandler parent) {
        this.parent = parent;
    }
    public void Update() {
        appearance = Toolbox.Max<Suspiciousness>(
            GameManager.I.playerInteractor?.GetSuspiciousness() ?? Suspiciousness.normal,
            GameManager.I.playerItemHandler?.GetSuspiciousness() ?? Suspiciousness.normal);
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
        parent.OnValueChanged();
    }
}
