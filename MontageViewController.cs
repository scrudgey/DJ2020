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
    public WorldmapView worldmapView;
    public Sprite chibiJackIdleSprite;

    [Header("panels")]
    public GameObject mapPanel;
    public GameObject montagePanel;
    [Header("bottom montage")]
    public GameObject plainTextHolder;
    public TextMeshProUGUI plainText;

    public GameObject continueButton;
    public GameObject barButtonsHolder;
    public GameObject reactionTextHolder;
    public TextMeshProUGUI reactionText;
    public Image reactionPortrait;

    [Header("bar")]
    public Image barCutaway;
    public GameObject barImageHolder;
    public GameObject reactionImageHolder;

    public RectTransform chibiJack;
    public SimpleAnimate chibiJackAnimation;
    public Image chibiJackImage;

    public BarMontageButton[] barButtons;
    [Header("restaurant")]
    public GameObject restaurantImageHolder;
    public TextMeshProUGUI restaurantSign;

    // public Tactic[] tactics; // TODO: change!
    Action continueButtonCallback;
    public void Initialize() {
        phase = Phase.start;
        gameObject.SetActive(false);
        bodyRect.sizeDelta = new Vector2(1000f, 50f);
        mapPanel.SetActive(true);
        montagePanel.SetActive(false);
        plainText.text = "";
        barImageHolder.SetActive(true);
        restaurantImageHolder.SetActive(false);
        reactionImageHolder.SetActive(false);
        reactionTextHolder.SetActive(false);
        plainTextHolder.SetActive(true);
        barButtonsHolder.SetActive(false);
        continueButton.SetActive(false);
        worldmapView.Initialize();

        chibiJackAnimation.enabled = false;
    }
    public void Finish() {
        gameObject.SetActive(false);
        afterActionReport.MontageFinishedCallback();
    }
    public void StartSequence() {
        gameObject.SetActive(true);
        StartCoroutine(Toolbox.ChainCoroutines(
            Toolbox.Ease(null, 0.3f, 50f, 780f, PennerDoubleAnimation.Linear, (amount) => {
                bodyRect.sizeDelta = new Vector2(1000f, amount);
            }, unscaledTime: true),
            Toolbox.CoroutineFunc(() => worldmapView.ShowText())
        ));
    }
    public void RestaurantButtonCallback() {
        phase = Phase.restaurant;
        InitializeRestaurant();
        StartMontage();
        StartCoroutine(RestaurantSequence());
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
        plainText.text = "";
    }

    void InitializeBar() {
        List<Tactic> availableTactics = GameManager.I.gameData.playerState.TacticsAvailableToUnlock();

        List<Tactic> targetTactics = new List<Tactic>();
        while (targetTactics.Count < 3 && availableTactics.Count > 0) {
            Tactic tactic = Toolbox.RandomFromList(availableTactics);
            targetTactics.Add(tactic);
            availableTactics.Remove(tactic);
        }
        int i = 0;
        foreach (Tactic tactic in targetTactics) {
            barButtons[i].Initialize(this, tactic);
            i++;
        }
        while (i < 3) {
            barButtons[i].gameObject.SetActive(false);
            i++;
        }
    }

    void InitializeRestaurant() {
        barImageHolder.SetActive(false);
        restaurantImageHolder.SetActive(true);
        chibiJack.SetParent(restaurantImageHolder.transform);
        chibiJack.SetAsLastSibling();
        // restaurantSign.text = "Sizzling Hot\nSquid on a Stick";
        restaurantSign.text = Toolbox.RandomFromList(randomRestaurantNames);
    }
    public void BarMontageButtonCallback(BarMontageButton montageButton) {
        // Debug.Log($"unlock {montageButton.tactic.name}");
        barButtonsHolder.SetActive(false);
        reactionTextHolder.SetActive(true);
        reactionPortrait.sprite = montageButton.tactic.vendorSprite;
        reactionText.text = "";
        GameManager.I.gameData.playerState.unlockedTactics.Add(montageButton.tactic);
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
        plainTextHolder.SetActive(true);

        StartCoroutine(Toolbox.ChainCoroutines(
            Toolbox.BlitText(plainText, $"Cheers! You have unlocked tactic: {tactic.title}!\nTo use it, call {tactic.vendorName} during mission planning.", interval: 0.02f),
            new WaitForSecondsRealtime(0.5f),
            Toolbox.CoroutineFunc(() => {
                continueButtonCallback = ContinueToClose;
                continueButton.SetActive(true);
            })
        ));
    }

    void ContinueToClose() {
        Finish();
    }

    IEnumerator BarSequence() {
        barCutaway.enabled = true;
        plainTextHolder.SetActive(true);
        barButtonsHolder.SetActive(false);
        chibiJack.anchoredPosition = new Vector2(-283, -100);
        plainText.text = "";
        StartCoroutine(Toolbox.BlitText(plainText, "Jack visits the bar...", interval: 0.02f));
        yield return Toolbox.ChainCoroutines(
            new WaitForSecondsRealtime(2f),
            Toolbox.CoroutineFunc(() => { chibiJackAnimation.enabled = true; }),
            Toolbox.Ease(null, 1.5f, -400, -122, PennerDoubleAnimation.Linear, (amount) => {
                chibiJack.anchoredPosition = new Vector2(amount, -100);
            }, unscaledTime: true),
            Toolbox.CoroutineFunc(() => barCutaway.enabled = false),
            Toolbox.Ease(null, 1.5f, -122, 267, PennerDoubleAnimation.Linear, (amount) => {
                chibiJack.anchoredPosition = new Vector2(amount, -100);
            }, unscaledTime: true),
            Toolbox.CoroutineFunc(() => {
                chibiJackAnimation.enabled = false;
                chibiJackImage.sprite = chibiJackIdleSprite;
            }),
            new WaitForSecondsRealtime(0.5f),
            Toolbox.CoroutineFunc(() => {
                plainTextHolder.SetActive(false);
                barButtonsHolder.SetActive(true);
            })
        );
    }

    IEnumerator RestaurantSequence() {
        plainTextHolder.SetActive(true);
        barButtonsHolder.SetActive(false);
        chibiJack.anchoredPosition = new Vector2(-429, -152f);
        plainText.text = "";
        StartCoroutine(Toolbox.BlitText(plainText, "Searching for late night food...", interval: 0.02f));
        yield return Toolbox.ChainCoroutines(
            new WaitForSecondsRealtime(2f),
            Toolbox.CoroutineFunc(() => { chibiJackAnimation.enabled = true; }),
            Toolbox.Ease(null, 2.5f, -429, 57, PennerDoubleAnimation.Linear, (amount) => {
                chibiJack.anchoredPosition = new Vector2(amount, -152f);
            }, unscaledTime: true),
            Toolbox.CoroutineFunc(() => {
                chibiJackAnimation.enabled = false;
                chibiJackImage.sprite = chibiJackIdleSprite;
            }),
            new WaitForSecondsRealtime(1.5f),
            Toolbox.CoroutineFunc(() => {
                restaurantImageHolder.SetActive(false);
                reactionImageHolder.SetActive(true);
                plainTextHolder.SetActive(true);
            }),
            Toolbox.BlitText(plainText, "Got food!\n+10 max HP"),
            Toolbox.CoroutineFunc(() => {
                // GameManager.I.gameData.playerState.fullHealthAmount
                // TODO: increase max health
                continueButton.SetActive(true);
                continueButtonCallback = ContinueToClose;
            })
        );
    }


    public void ContinueButtonClicked() {
        continueButtonCallback?.Invoke();
    }


    public static List<string> randomRestaurantNames = new List<string>{
        "Soy Beef Bucket",
        "tikka masala sushi",
        "empanada fusion",
        "samosa pizza",
        "Sizzling Lunch",
        "Dumpster soup",
        "Sizzling Hot\nSquid on a Stick",
        "3AM BEEF",
        "Angel Jesus's\nFresh Manpower Factory Outlet",
        "Baked Salad with Cheddar",
        "Laser-heated\nCurrywurst Supreme",
        "sushi burrito",
        "biryani bucket",
        "frozen hummus hut",
        "New York style sushi"
    };
}
