using System;
using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MontageViewController : MonoBehaviour {
    enum Phase { start, restaurant, bar, club }
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
    public GameObject clubButtonsHolder;
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
    public GameObject restaurantReactionHolder;
    public Transform restaurantJackHolder;
    [Header("nightclub")]
    public Transform clubJackHolder;
    public GameObject clubImageHolder;
    public GameObject clubReactionHolder;

    public ClubMontageButton[] clubMontageButtons;
    public RectTransform laserBundle1;
    public RectTransform laserBundle2;
    public RectTransform laserBundle3;
    public Image light1;
    public Image light2;

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
        clubButtonsHolder.SetActive(false);
        restaurantReactionHolder.SetActive(false);
        clubReactionHolder.SetActive(false);
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
    public void ClubButtonCallback() {
        phase = Phase.club;
        InitializeClub();
        StartMontage();
        StartCoroutine(ClubSequence());
    }
    // public void BarButtonMouseOver() {
    //     worldmapView.HighlightPoint(1);
    // }
    // public void RestaurantButtonMouseOver() {
    //     worldmapView.HighlightPoint(2);
    // }
    // public void MapButtonMouseExit() {
    //     worldmapView.StopHighlight();
    // }
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
        chibiJack.SetParent(restaurantJackHolder.transform);
        chibiJack.SetAsLastSibling();
        // restaurantSign.text = "Sizzling Hot\nSquid on a Stick";
        restaurantSign.text = Toolbox.RandomFromList(randomRestaurantNames);
    }
    void InitializeClub() {
        barImageHolder.SetActive(false);
        clubImageHolder.SetActive(true);
        chibiJack.SetParent(clubJackHolder.transform);
        chibiJack.SetAsLastSibling();
        List<LootBuyerData> availableTactics = GameManager.I.gameData.playerState.FencesAvailableToUnlock();
        List<LootBuyerData> targetTactics = new List<LootBuyerData>();

        laserBundle1.gameObject.SetActive(true);
        laserBundle2.gameObject.SetActive(false);
        laserBundle3.gameObject.SetActive(false);


        StartCoroutine(rotateLaserBundle(laserBundle1));
        StartCoroutine(rotateLaserBundle(laserBundle2));
        StartCoroutine(blinkLasers(laserBundle1.gameObject, laserBundle2.gameObject));
        StartCoroutine(blinkLasers(laserBundle3.gameObject, null));
        StartCoroutine(PulseLight(light1, 0f));
        StartCoroutine(PulseLight(light2, 2f));

        while (targetTactics.Count < 3 && availableTactics.Count > 0) {
            LootBuyerData tactic = Toolbox.RandomFromList(availableTactics);
            targetTactics.Add(tactic);
            availableTactics.Remove(tactic);
        }
        int i = 0;
        foreach (LootBuyerData tactic in targetTactics) {
            // Debug.Log($"configuring club montage button {i} {tactic}");
            clubMontageButtons[i].Initialize(this, tactic);
            i++;
        }
        while (i < 3) {
            // Debug.Log($"disabling club montage button {i}");
            clubMontageButtons[i].gameObject.SetActive(false);
            i++;
        }
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
    public void ClubMontageButtonCallback(ClubMontageButton montageButton) {
        clubButtonsHolder.SetActive(false);
        reactionTextHolder.SetActive(true);

        reactionPortrait.sprite = montageButton.lootBuyerData.portrait;
        reactionText.text = "";
        GameManager.I.gameData.playerState.unlockedFences.Add(montageButton.lootBuyerData);
        StartCoroutine(Toolbox.ChainCoroutines(
            Toolbox.BlitText(reactionText, montageButton.lootBuyerData.introduction, interval: 0.02f),
            new WaitForSecondsRealtime(0.5f),
            Toolbox.CoroutineFunc(() => {
                continueButtonCallback = () => ContinueToReactionImage(montageButton.lootBuyerData);
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

    void ContinueToReactionImage(LootBuyerData fence) {
        continueButton.SetActive(false);
        barImageHolder.SetActive(false);
        reactionTextHolder.SetActive(false);

        clubReactionHolder.SetActive(true);
        plainTextHolder.SetActive(true);

        StartCoroutine(Toolbox.ChainCoroutines(
            Toolbox.BlitText(plainText, $"Cheers! You have unlocked fence: {fence.buyerName}!", interval: 0.02f),
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
        StartCoroutine(Toolbox.BlitText(plainText, "Visiting the bar...", interval: 0.02f));
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
    IEnumerator ClubSequence() {
        plainTextHolder.SetActive(true);
        barButtonsHolder.SetActive(false);
        chibiJack.anchoredPosition = new Vector2(-304, 160);
        plainText.text = "";
        StartCoroutine(Toolbox.BlitText(plainText, "Visiting the club...", interval: 0.02f));
        yield return Toolbox.ChainCoroutines(
            new WaitForSecondsRealtime(1f),
            Toolbox.CoroutineFunc(() => { chibiJackAnimation.enabled = true; }),
            Toolbox.Ease(null, 3f, 0, 1, PennerDoubleAnimation.Linear, (amount) => {
                float y = 160 + (-58 - 160) * amount;
                float x = -304 + (235 - -304) * amount;
                chibiJack.anchoredPosition = new Vector2(x, y);
            }, unscaledTime: true),
            Toolbox.CoroutineFunc(() => {
                chibiJackAnimation.enabled = false;
                chibiJackImage.sprite = chibiJackIdleSprite;
            }),
            new WaitForSecondsRealtime(0.5f),
            Toolbox.CoroutineFunc(() => {
                plainTextHolder.SetActive(false);
                clubButtonsHolder.SetActive(true);
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
                restaurantReactionHolder.SetActive(true);
                plainTextHolder.SetActive(true);
            }),
            Toolbox.BlitText(plainText, "Got food!\n+1 skill point"),
            Toolbox.CoroutineFunc(() => {
                // GameManager.I.gameData.playerState.bonusHealth += 10;
                // GameManager.I.gameData.playerState.health = GameManager.I.gameData.playerState.fullHealthAmount();
                GameManager.I.gameData.playerState.skillpoints += 1;
                continueButton.SetActive(true);
                continueButtonCallback = ContinueToClose;
            })
        );
    }

    IEnumerator rotateLaserBundle(RectTransform laserBundle) {
        float time = 0;
        float dutyCycle = 4f;
        float hangtime = 0f;
        Quaternion origRotation = laserBundle.rotation;
        while (true) {
            time += Time.unscaledDeltaTime;
            float angle = 50f * Mathf.Sin(time);
            laserBundle.rotation = origRotation * Quaternion.Euler(0f, 0f, angle);
            if (time > dutyCycle) {
                time = 0f;
                laserBundle.rotation = origRotation;
                while (hangtime < dutyCycle) {
                    hangtime += Time.unscaledDeltaTime;
                    yield return null;
                }
                hangtime = 0f;
            }
            yield return null;
        }
    }

    IEnumerator blinkLasers(GameObject bundle1, GameObject bundle2) {
        float time = 0f;
        float duration = 1f;
        bool cycle = true;
        bool flicker1 = true;
        bool flicker2 = true;
        while (true) {
            time += Time.unscaledDeltaTime;
            if (time > duration) {
                time -= duration;
                cycle = !cycle;
            }
            flicker1 = !flicker1;
            flicker2 = !flicker2;

            bundle1?.SetActive(cycle && flicker1);
            bundle2?.SetActive(!cycle && flicker2);
            yield return null;
        }
    }

    IEnumerator PulseLight(Image image, float phase) {
        Color originalColor = image.color;
        // return Toolbox.Ease(null, 5f, originalColor.a, 0f, PennerDoubleAnimation.CircEaseOut, (amount) => {
        //     image.color = new Color(originalColor.r, originalColor.g, originalColor.b, amount);
        // }, unscaledTime: true, looping: true);
        float timer = phase;
        while (true) {
            timer += Time.unscaledDeltaTime;
            float amount = originalColor.a * Mathf.Abs((float)Mathf.Sin(timer));
            image.color = new Color(originalColor.r, originalColor.g, originalColor.b, amount);
            yield return null;
        }
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
