using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MeleeWeaponUIHandler : IBinder<MeleeHandler> {
    public TextMeshProUGUI gunTitle;
    public Image gunImage;
    public RectTransform gunPanelRect;
    Coroutine openDisplayCoroutine;
    bool displayIsOpen;

    override public void HandleValueChanged(MeleeHandler meleeHandler) {
        if (target.meleeWeapon != null) {
            gunImage.enabled = true;
            gunImage.sprite = target.meleeWeapon.sprite;
            gunTitle.text = target.meleeWeapon.name;
            if (!displayIsOpen) {
                if (openDisplayCoroutine != null)
                    StopCoroutine(openDisplayCoroutine);
                openDisplayCoroutine = StartCoroutine(openDisplay());
            }
        } else {
            gunImage.enabled = false;
            gunImage.sprite = null;
            gunTitle.text = "";
            if (displayIsOpen) {
                if (openDisplayCoroutine != null)
                    StopCoroutine(openDisplayCoroutine);
                openDisplayCoroutine = StartCoroutine(closeDisplay());
            }
        }
    }
    public void Initialize() {
        // displayIsOpen = true;
    }

    IEnumerator openDisplay() {
        displayIsOpen = true;
        return Toolbox.Ease(null, 0.25f, 0f, 1f, PennerDoubleAnimation.Linear, (amount) => {
            gunPanelRect.localScale = new Vector3(1f, amount, 1f);
        });
    }
    IEnumerator closeDisplay() {
        displayIsOpen = false;
        return Toolbox.Ease(null, 0.25f, 1f, 0f, PennerDoubleAnimation.Linear, (amount) => {
            gunPanelRect.localScale = new Vector3(1f, amount, 1f);
        });
    }
}
