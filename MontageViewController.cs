using System;
using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MontageViewController : MonoBehaviour {
    enum Phase { start, restaurant, bar, hack }
    Phase phase;
    public NeoAfterActionReport afterActionReport;
    public RectTransform bodyRect;
    [Header("panels")]
    public GameObject mapPanel;
    public GameObject montagePanel;
    [Header("bar")]
    public Image barCutaway;
    public GameObject barImageHolder;
    public GameObject barTextHolder;
    public GameObject barButtonsHolder;
    public GameObject reactionImageHolder;
    public GameObject reactionTextHolder;
    public GameObject continueButton;
    public TextMeshProUGUI reactionText;
    public Image reactionPortrait;
    public RectTransform chibiJack;
    public TextMeshProUGUI barText;
    public BarMontageButton[] barButtons;
    public Tactic[] tactics; // TODO: change!
    Action continueButtonCallback;
    public void Initialize() {
        phase = Phase.start;
        gameObject.SetActive(false);
        bodyRect.sizeDelta = new Vector2(1000f, 50f);
        mapPanel.SetActive(true);
        montagePanel.SetActive(false);
        barText.text = "";
        barImageHolder.SetActive(true);
        reactionImageHolder.SetActive(false);
        reactionTextHolder.SetActive(false);
        barTextHolder.SetActive(true);
        barButtonsHolder.SetActive(false);
        continueButton.SetActive(false);
    }
    public void Finish() {
        gameObject.SetActive(false);
        afterActionReport.MontageFinishedCallback();
    }
    public void StartSequence() {
        gameObject.SetActive(true);
        StartCoroutine(Toolbox.Ease(null, 0.3f, 50f, 780f, PennerDoubleAnimation.Linear, (amount) => {
            bodyRect.sizeDelta = new Vector2(1000f, amount);
        }, unscaledTime: true));
    }
    public void RestaurantButtonCallback() {
        phase = Phase.restaurant;
        StartMontage();
    }
    public void BarButtonCallback() {
        phase = Phase.bar;
        InitializeBar();

        StartMontage();
        StartCoroutine(BarSequence());
    }
    public void HackButtonCallback() {
        phase = Phase.hack;
        StartMontage();
    }
    void StartMontage() {
        mapPanel.SetActive(false);
        montagePanel.SetActive(true);
        barText.text = "";
    }

    void InitializeBar() {
        for (int i = 0; i < 3; i++) {
            barButtons[i].Initialize(this, tactics[i]);
        }
    }
    public void BarMontageButtonCallback(BarMontageButton montageButton) {
        Debug.Log($"unlock {montageButton.tactic.name}");
        barButtonsHolder.SetActive(false);
        reactionTextHolder.SetActive(true);
        reactionPortrait.sprite = montageButton.tactic.vendorSprite;
        reactionText.text = "";
        StartCoroutine(Toolbox.ChainCoroutines(
            Toolbox.BlitText(reactionText, montageButton.tactic.vendorIntroduction, interval: 0.02f),
            new WaitForSecondsRealtime(0.5f),
            Toolbox.CoroutineFunc(() => {
                continueButtonCallback = () => ContinueToReactionImage(montageButton.tactic);
                continueButton.SetActive(true);
            })
        ));
    }

    void ContinueToReactionImage(Tactic tactic) {
        continueButton.SetActive(false);
        barImageHolder.SetActive(false);
        reactionTextHolder.SetActive(false);

        reactionImageHolder.SetActive(true);
        barTextHolder.SetActive(true);

        StartCoroutine(Toolbox.ChainCoroutines(
            Toolbox.BlitText(barText, $"You have unlocked tactic: {tactic.title}!", interval: 0.02f),
            new WaitForSecondsRealtime(0.5f),
            Toolbox.CoroutineFunc(() => {
                continueButtonCallback = ContinueToClose;
                continueButton.SetActive(true);
            })
        ));
    }

    void ContinueToClose() {
        Debug.Log("close");
        Finish();
    }

    IEnumerator BarSequence() {
        barCutaway.enabled = true;
        barTextHolder.SetActive(true);
        barButtonsHolder.SetActive(false);
        chibiJack.anchoredPosition = new Vector2(-283, -100);
        barText.text = "";
        StartCoroutine(Toolbox.BlitText(barText, "Jack visits the bar...", interval: 0.02f));
        yield return Toolbox.ChainCoroutines(
            new WaitForSecondsRealtime(0.5f),
            Toolbox.Ease(null, 2f, -283, -50, PennerDoubleAnimation.Linear, (amount) => {
                chibiJack.anchoredPosition = new Vector2(amount, -100);
            }, unscaledTime: true),
            Toolbox.CoroutineFunc(() => barCutaway.enabled = false),
            Toolbox.Ease(null, 2f, -50, 181, PennerDoubleAnimation.Linear, (amount) => {
                chibiJack.anchoredPosition = new Vector2(amount, -100);
            }, unscaledTime: true),
            new WaitForSecondsRealtime(0.5f),
            Toolbox.CoroutineFunc(() => {
                barTextHolder.SetActive(false);
                barButtonsHolder.SetActive(true);
            })
        );
    }

    public void ContinueButtonClicked() {
        continueButtonCallback?.Invoke();
    }
}
