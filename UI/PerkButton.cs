using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PerkButton : MonoBehaviour {
    public Image icon;
    public GameObject lockIcon;
    public TextMeshProUGUI caption;
    public Button mybutton;
    public Image[] pips;
    public Perk perk;
    public Sprite pipActive;
    public Sprite pipDisabled;
    public Image selectedIndicator;
    public Image backgroundImage;
    [Header("colors")]
    public Color activeBackgroundCOlor;
    public ColorBlock purchasableButtonColors;
    public ColorBlock nonpurchaseableButtonColors;
    public Color purchaseableColor;
    public Color notpurchaseableColor;
    [Header("easing")]
    public RectTransform buttonRect;
    public RectTransform iconRect;
    PlayerState state;
    PerkMenuController controller;

    public void Initialize(PerkMenuController controller, PlayerState state) {
        this.controller = controller;
        this.state = state;

        icon.sprite = perk.icon;
        caption.text = perk.readableName;

        SetPips();

        SetActiveStatus();
    }
    void SetPips() {
        if (perk.IsMultiStagePerk()) {
            int i = 0;
            foreach (Image pip in pips) {
                pip.gameObject.SetActive(i < perk.stages);
                i += 1;
            }
        } else {
            foreach (Image pip in pips) {
                pip.gameObject.SetActive(false);
            }
        }
    }

    public void SetActiveStatus() {
        // if (perk.IsMultiStagePerk()) {
        //     for (int i = 1; i < perk.stages + 1; i++) {
        //         if (state.PerkLevelIsActivated(perk, i)) {
        //             pips[i - 1].sprite = pipActive;
        //         } else {
        //             pips[i - 1].sprite = pipDisabled;
        //         }
        //     }
        // }
        SetPips();

        if (state.PerkIsFullyActivated(perk)) {
            backgroundImage.color = activeBackgroundCOlor;
            icon.color = Color.black;
        }

        if (perk.CanBePurchased(state)) {
            lockIcon.SetActive(false);

            icon.color = purchaseableColor;
            caption.color = purchaseableColor;
            mybutton.colors = purchasableButtonColors;
        } else {
            lockIcon.SetActive(true);

            icon.color = notpurchaseableColor;
            caption.color = notpurchaseableColor;
            mybutton.colors = nonpurchaseableButtonColors;
        }
    }

    public void ClickCallback() {
        controller.PerkButtonCallback(this);
    }

    public void SetSelected(bool selected) {
        selectedIndicator.enabled = selected;
    }
    public void EaseIn(float delay) {
        if (gameObject.activeInHierarchy)
            StartCoroutine(EaseInRoutine(delay));
    }
    IEnumerator EaseInRoutine(float delay) {
        lockIcon.SetActive(false);
        caption.enabled = false;
        buttonRect.sizeDelta = new Vector2(0, 0);
        iconRect.sizeDelta = new Vector2(0, 0);
        foreach (Image pip in pips) {
            pip.gameObject.SetActive(false);
        }

        yield return Toolbox.ChainCoroutines(
            new WaitForSecondsRealtime(delay),
            Toolbox.Ease(null, 0.25f, 0f, 1f, PennerDoubleAnimation.Linear, (amount) => {
                // Debug.Log(amount);
                buttonRect.sizeDelta = new Vector2(amount * 80f, amount * 80f);
                iconRect.sizeDelta = new Vector2(amount * 60f, amount * 60f);
            }, unscaledTime: true),
            Toolbox.CoroutineFunc(() => {
                // lockIcon.SetActive(true);
                caption.enabled = true;
                // SetPips();
                SetActiveStatus();
            })
        );
    }


#if UNITY_EDITOR
    private void OnDrawGizmos() {
        Handles.Label(transform.position, $"{perk.readableName}");

    }
#endif
}
