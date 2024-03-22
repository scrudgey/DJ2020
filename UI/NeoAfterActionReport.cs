using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class NeoAfterActionReport : MonoBehaviour {
    GameData gameData;
    enum State { title, missionName, reward, fadeout }
    State state = State.missionName;
    public Image fade;
    public RectTransform reportMask;
    public RectTransform reportRect;
    public GameObject continueButton;
    public GameObject perkMenuButton;
    public Button perkMenuButtonComponent;
    public MontageViewController montageViewController;
    public Animator jackAnimation;
    public SubwayAnimator subwayAnimator;
    public SimpleAnimate[] holdAnimators;

    [Header("mission name")]
    public ContentSizeFitter missionContentFitter;
    public Image missionPanelImage;
    public GameObject missionPanel;
    public TextMeshProUGUI missionNameText;
    public TextMeshProUGUI bylineText;
    public TextMeshProUGUI missionStatusText;
    public TextMeshProUGUI optionalObjectivesTitle;
    public Image objectivesUnderline;
    public Image optionalObjectivesUnderline;
    public GameObject spacer1;
    public GameObject spacer2;
    public Transform objectivesContainer;
    public Transform optionalObjectivesContainer;
    public GameObject objectivePrefab;
    public Color baseColor;
    public float targetHeight;
    [Header("reward")]
    public RectTransform rewardRect;
    public GameObject rewardPanel;
    public GameObject rewardObject;
    public GameObject bonusObject;
    public GameObject balanceObject;
    public GameObject counterObject;
    public GameObject skillPointsObject;
    public GameObject counterCreditSymbolObject;
    public Image rewardLine;
    public TextMeshProUGUI rewardCaptionText;
    public TextMeshProUGUI bonyusCaptionText;
    public TextMeshProUGUI balanceCaptionText;
    public TextMeshProUGUI rewardAmountText;
    public TextMeshProUGUI bonusAmountText;
    public TextMeshProUGUI counterAmountText;
    public TextMeshProUGUI balanceAmountText;
    public ParticleSystem creditParticles;
    public TextMeshProUGUI skillPointsText;
    public TextMeshProUGUI skillPointCounterText;
    public RectTransform skillPointCounterRect;
    RectTransform adjustMaskToRect;
    int mutableBalance;
    int bonusAmount;
    int counterBalance;
    Coroutine currentCoroutine;
    float skipTimer = 0f;
    public void Initialize(GameData data) {
        this.gameData = data;
        skipTimer = 2f;
        fade.color = new Color(0, 0, 0, 1);
        targetHeight = 0f;
        continueButton.SetActive(false);
        perkMenuButton.SetActive(false);
        montageViewController.Initialize();
        subwayAnimator.Play();
        bonusAmount = gameData.levelState.delta.optionalObjectiveDeltas
            .Where(delta => delta.status == ObjectiveStatus.complete)
            .Select(delta => delta.template.bonusRewardCredits)
            .Sum();
        counterBalance = data.levelState.template.creditReward + bonusAmount;

        mutableBalance = data.playerState.credits - data.levelState.template.creditReward;

        state = State.title;

        currentCoroutine = StartCoroutine(ShowTitleAndObjectives(data));
    }
    void Update() {
        if (adjustMaskToRect != null) {
            float preferredHeight = LayoutUtility.GetPreferredHeight(adjustMaskToRect);
            targetHeight = Mathf.Lerp(targetHeight, preferredHeight, 1f);
            reportMask.sizeDelta = new Vector2(adjustMaskToRect.rect.width, targetHeight);
        }
        if (perkMenuButton.activeInHierarchy) {
            skillPointCounterText.text = $"{GameManager.I.gameData.playerState.skillpoints}";
        }

        if (skipTimer > 0) {
            skipTimer -= Time.unscaledDeltaTime;
        }

        if (skipTimer <= 0 && state != State.fadeout && !continueButton.activeInHierarchy && (Keyboard.current.anyKey.isPressed || Mouse.current.leftButton.isPressed || Mouse.current.rightButton.isPressed)) {
            AdvanceToNextCondition();
            skipTimer = 1f;
        }
    }
    void AdvanceToNextCondition() {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        switch (state) {
            case State.title:
                // Debug.Log("skip title ");
                SkipTitleAndObjectives(gameData);
                break;
            case State.missionName:
                // Debug.Log("skip missionName");
                SkipReward(gameData);
                break;
            case State.reward:
                // Debug.Log("skip reward");
                SkipReward(gameData);
                break;
        }
    }
    public void ContinueButtonCallback() {
        switch (state) {
            case State.missionName:
                // Debug.Log("continue from missionName -> reward");
                state = State.reward;
                continueButton.SetActive(false);
                currentCoroutine = StartCoroutine(ShowReward(gameData));
                break;
            case State.reward:
                // Debug.Log("continue from reward -> fadeout");
                state = State.fadeout;
                continueButton.SetActive(false);
                perkMenuButton.SetActive(false);
                currentCoroutine = StartCoroutine(FadeToMontage());
                break;
        }
    }
    public void PerkButtonCallback() {
        GameManager.I.ShowPerkMenu();
    }
    IEnumerator FadeIn() {
        yield return Toolbox.Ease(null, 2f, 1f, 0f, PennerDoubleAnimation.Linear, (amount) => fade.color = new Color(0, 0, 0, amount), unscaledTime: true);
    }
    IEnumerator FadeToMontage() {
        yield return Toolbox.ChainCoroutines(
            Toolbox.Ease(null, 1f, 0f, 0.5f, PennerDoubleAnimation.Linear, (amount) => fade.color = new Color(0, 0, 0, amount), unscaledTime: true),
            Toolbox.CoroutineFunc(() => {
                jackAnimation.enabled = false;
                foreach (SimpleAnimate animate in holdAnimators) {
                    animate.Stop();
                }
                subwayAnimator.Stop();
                montageViewController.StartSequence();
            })
        );
    }
    IEnumerator FadeToNewDay() {
        jackAnimation.enabled = true;
        foreach (SimpleAnimate animate in holdAnimators) {
            animate.Play();
        }
        subwayAnimator.Play();
        yield return Toolbox.ChainCoroutines(
            Toolbox.Ease(null, 1f, 0.5f, 1f, PennerDoubleAnimation.Linear, (amount) => fade.color = new Color(0, 0, 0, amount), unscaledTime: true),
            Toolbox.CoroutineFunc(() => GameManager.I.StartNewDay())
        );
    }
    public void MontageFinishedCallback() {
        StartCoroutine(FadeToNewDay());
    }

    IEnumerator ShowTitleAndObjectives(GameData data) {
        missionPanel.SetActive(true);
        rewardPanel.SetActive(false);
        reportMask.sizeDelta = new Vector2(reportRect.rect.width, 0);
        missionNameText.text = data.levelState.template.readableMissionName;
        bylineText.text = data.levelState.template.tagline;
        missionStatusText.text = "mission complete";
        ClearMissionInfo();

        yield return FadeIn();
        float preferredHeight = LayoutUtility.GetPreferredHeight(reportRect);
        yield return Toolbox.Ease(null, 1f, 0f, preferredHeight, PennerDoubleAnimation.Linear, (amount) => {
            reportMask.sizeDelta = new Vector2(reportRect.rect.width, amount);
        }, unscaledTime: true);
        adjustMaskToRect = reportRect;
        yield return new WaitForSeconds(0.5f);

        missionNameText.enabled = true;
        yield return EaseInTextColor(missionNameText, duration: 2f);
        bylineText.enabled = true;
        yield return new WaitForSeconds(1f);
        yield return EaseInTextColor(bylineText);
        objectivesUnderline.enabled = true;
        yield return EaseInImageColor(objectivesUnderline, baseColor);
        yield return new WaitForSeconds(1f);

        List<ObjectiveDelta> requiredObjectives = data.levelState.delta.objectiveDeltas;
        List<ObjectiveDelta> optionalObjectives = data.levelState.delta.optionalObjectiveDeltas;

        objectivesContainer.gameObject.SetActive(true);
        foreach (ObjectiveDelta objective in requiredObjectives) {
            AfterActionReportObjectiveHandler handler = CreateObjectiveHandler(objectivesContainer, objective, data);
            StartCoroutine(TypeText(handler.objectiveText));
            StartCoroutine(EaseInTextColor(handler.objectiveText));
            yield return new WaitForSeconds(0.5f);
        }
        if (optionalObjectives.Count > 0) {
            yield return new WaitForSeconds(1f);
            spacer1.SetActive(true);
            optionalObjectivesContainer.gameObject.SetActive(true);
            optionalObjectivesTitle.enabled = true;
            optionalObjectivesUnderline.enabled = true;
            StartCoroutine(EaseInTextColor(optionalObjectivesTitle));
            StartCoroutine(EaseInImageColor(optionalObjectivesUnderline, baseColor));
            yield return new WaitForSeconds(1f);
            foreach (ObjectiveDelta objective in optionalObjectives) {
                AfterActionReportObjectiveHandler handler = CreateObjectiveHandler(optionalObjectivesContainer, objective, data);
                StartCoroutine(TypeText(handler.objectiveText));
                StartCoroutine(EaseInTextColor(handler.objectiveText));
                yield return new WaitForSeconds(0.5f);
            }
        }
        yield return new WaitForSeconds(1f);
        spacer2.SetActive(true);
        yield return new WaitForSeconds(1f);
        missionStatusText.enabled = true;
        yield return EaseInTextColor(missionStatusText);
        state = State.missionName;
        continueButton.SetActive(true);
    }
    void SkipTitleAndObjectives(GameData data) {
        missionPanel.SetActive(true);
        rewardPanel.SetActive(false);
        missionNameText.text = data.levelState.template.readableMissionName;
        bylineText.text = data.levelState.template.tagline;
        missionStatusText.text = "mission complete";
        ClearMissionInfo();

        float preferredHeight = LayoutUtility.GetPreferredHeight(reportRect);
        reportMask.sizeDelta = new Vector2(reportRect.rect.width, preferredHeight);
        adjustMaskToRect = reportRect;
        missionNameText.enabled = true;
        missionNameText.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
        bylineText.enabled = true;
        bylineText.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
        objectivesUnderline.enabled = true;
        objectivesUnderline.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
        List<ObjectiveDelta> requiredObjectives = data.levelState.delta.objectiveDeltas;
        List<ObjectiveDelta> optionalObjectives = data.levelState.delta.optionalObjectiveDeltas;
        objectivesContainer.gameObject.SetActive(true);
        foreach (ObjectiveDelta objective in requiredObjectives) {
            AfterActionReportObjectiveHandler handler = CreateObjectiveHandler(objectivesContainer, objective, data);
            handler.objectiveText.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
        }
        if (optionalObjectives.Count > 0) {
            spacer1.SetActive(true);
            optionalObjectivesContainer.gameObject.SetActive(true);
            optionalObjectivesTitle.enabled = true;
            optionalObjectivesUnderline.enabled = true;
            optionalObjectivesTitle.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
            optionalObjectivesUnderline.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
            foreach (ObjectiveDelta objective in optionalObjectives) {
                AfterActionReportObjectiveHandler handler = CreateObjectiveHandler(optionalObjectivesContainer, objective, data);
                handler.objectiveText.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
            }
        }
        spacer2.SetActive(true);
        missionStatusText.enabled = true;
        missionStatusText.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
        state = State.missionName;
        continueButton.SetActive(true);
    }

    IEnumerator ShowReward(GameData data) {
        rewardObject.SetActive(false);
        bonusObject.SetActive(false);
        counterObject.SetActive(false);
        skillPointsObject.SetActive(false);
        balanceObject.SetActive(false);

        rewardLine.color = Color.clear;

        missionContentFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        ClearMissionInfo();

        yield return EaseInImageColor(missionPanelImage, Color.black, start: 0.67f, end: 0f);

        missionPanel.SetActive(false);
        rewardPanel.SetActive(true);
        float preferredHeight = LayoutUtility.GetPreferredHeight(rewardRect);
        yield return Toolbox.Ease(null, 1f, 0f, preferredHeight, PennerDoubleAnimation.Linear, (amount) => {
            reportMask.sizeDelta = new Vector2(rewardRect.rect.width, amount);
        }, unscaledTime: true);
        adjustMaskToRect = rewardRect;
        yield return new WaitForSeconds(1.5f);

        // missionNameText.enabled = true;
        rewardObject.SetActive(true);
        rewardAmountText.text = data.levelState.template.creditReward.ToString("#,#");
        StartCoroutine(TypeText(rewardCaptionText));
        StartCoroutine(TypeText(rewardAmountText));
        yield return new WaitForSeconds(1.5f);

        bonusObject.SetActive(true);
        bonusAmountText.text = bonusAmount.ToString("#,#");
        StartCoroutine(TypeText(bonyusCaptionText));

        StartCoroutine(TypeText(bonusAmountText));
        StartCoroutine(EaseInImageColor(rewardLine, baseColor));
        yield return new WaitForSeconds(1.5f);

        balanceObject.SetActive(true);
        counterObject.SetActive(true);
        counterAmountText.text = "+" + counterBalance.ToString("#,#");
        balanceAmountText.text = mutableBalance.ToString("#,#");
        StartCoroutine(TypeText(balanceCaptionText));
        StartCoroutine(TypeText(balanceAmountText));
        StartCoroutine(TypeText(counterAmountText));
        yield return new WaitForSeconds(1f);
        if (creditParticles != null) {
            creditParticles.Play();
        }
        while (counterBalance > 0) {
            int decrementAmount = (int)(0.01f * counterBalance);
            decrementAmount = Mathf.Max(100, decrementAmount);
            decrementAmount = Mathf.Min(decrementAmount, counterBalance);
            counterBalance -= decrementAmount;
            mutableBalance += decrementAmount;
            counterAmountText.text = "+" + counterBalance.ToString("#,#");
            balanceAmountText.text = mutableBalance.ToString("#,#");
            yield return new WaitForSeconds(0.01f);
        }
        counterAmountText.gameObject.SetActive(false);
        counterCreditSymbolObject.SetActive(false);
        if (creditParticles != null) {
            creditParticles.Stop();
        }
        yield return new WaitForSeconds(1f);
        skillPointsObject.SetActive(true);
        skillPointCounterText.text = $"{data.playerState.skillpoints - 1}";
        StartCoroutine(TypeText(skillPointsText));
        StartCoroutine(TypeText(skillPointCounterText));
        yield return new WaitForSeconds(1f);
        skillPointCounterText.text = $"{data.playerState.skillpoints}";
        StartCoroutine(Toolbox.Ease(null, 1f, 2f, 1f, PennerDoubleAnimation.BounceEaseOut, (float amount) => {
            skillPointCounterRect.localScale = amount * Vector3.one;
        }, unscaledTime: true));
        yield return new WaitForSeconds(0.5f);
        continueButton.SetActive(true);
        perkMenuButton.SetActive(true);
        // emailText.text = data.levelState.template.successEmail.text;
        // rewardAmountText.text = data.levelState.template.creditReward.ToString("#,#");
        // bonusAmountText.text = "0";
        // balanceAmountText.text = data.playerState.credits.ToString("#,#");
    }

    //sd asdada
    void SkipReward(GameData data) {
        bonusObject.SetActive(false);
        counterObject.SetActive(false);
        skillPointsObject.SetActive(false);
        balanceObject.SetActive(false);
        rewardLine.color = Color.clear;
        missionContentFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        ClearMissionInfo();
        missionPanelImage.color = Color.black;
        missionPanel.SetActive(false);
        rewardPanel.SetActive(true);
        float preferredHeight = LayoutUtility.GetPreferredHeight(rewardRect);
        reportMask.sizeDelta = new Vector2(rewardRect.rect.width, preferredHeight);
        adjustMaskToRect = rewardRect;

        rewardObject.SetActive(true);
        rewardAmountText.text = data.levelState.template.creditReward.ToString("#,#");

        bonusObject.SetActive(true);
        bonusAmountText.text = "0";
        rewardLine.color = baseColor;

        balanceObject.SetActive(true);
        counterObject.SetActive(true);
        balanceAmountText.text = mutableBalance.ToString("#,#");
        if (creditParticles != null) {
            creditParticles.Play();
        }
        mutableBalance = data.levelState.template.creditReward;
        balanceAmountText.text = data.playerState.credits.ToString("#,#");
        counterAmountText.gameObject.SetActive(false);
        counterCreditSymbolObject.SetActive(false);
        skillPointsObject.SetActive(true);
        skillPointCounterText.text = $"{data.playerState.skillpoints}";
        continueButton.SetActive(true);
        perkMenuButton.SetActive(true);
    }
    void ClearMissionInfo() {
        foreach (Transform child in objectivesContainer) {
            Destroy(child.gameObject);
        }
        foreach (Transform child in optionalObjectivesContainer) {
            Destroy(child.gameObject);
        }

        adjustMaskToRect = null;
        // reportMask.sizeDelta = new Vector2(reportRect.rect.width, 0);

        missionNameText.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0);
        bylineText.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0);
        missionStatusText.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0);
        optionalObjectivesTitle.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0);
        objectivesUnderline.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0);
        optionalObjectivesUnderline.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0);

        spacer1.SetActive(false);
        spacer2.SetActive(false);
        missionNameText.enabled = false;
        missionStatusText.enabled = false;
        bylineText.enabled = false;
        optionalObjectivesTitle.enabled = false;
        objectivesContainer.gameObject.SetActive(false);
        optionalObjectivesContainer.gameObject.SetActive(false);
        objectivesUnderline.enabled = false;
        optionalObjectivesUnderline.enabled = false;
    }

    AfterActionReportObjectiveHandler CreateObjectiveHandler(Transform container, ObjectiveDelta objective, GameData data) {
        GameObject obj = GameObject.Instantiate(objectivePrefab);
        obj.transform.SetParent(container, false);
        AfterActionReportObjectiveHandler handler = obj.GetComponent<AfterActionReportObjectiveHandler>();
        ObjectiveStatus status = objective.status;
        handler.Initialize(objective, status, data);
        return handler;
    }

    IEnumerator EaseInTextColor(TextMeshProUGUI text, float duration = 1) {
        yield return Toolbox.Ease(null, duration, 0f, 1f, PennerDoubleAnimation.Linear, (amount) =>
                    text.color = new Color(baseColor.r, baseColor.g, baseColor.b, amount),
                    unscaledTime: true
                );
    }
    IEnumerator EaseInImageColor(Image image, Color color, float start = 0f, float end = 1f) {
        yield return Toolbox.Ease(null, 1f, start, end, PennerDoubleAnimation.Linear, (amount) =>
                    image.color = new Color(color.r, color.g, color.b, amount),
                    unscaledTime: true
                );
    }
    IEnumerator TypeText(TextMeshProUGUI text) {
        float characterDuration = 0.04f;
        float cursorDuration = 0.2f;
        string totalText = text.text;
        float duration = characterDuration * totalText.Length;
        float cursortimer = 0;

        float timer = 0f;
        bool cursorvisible = true;
        text.text = "";
        while (timer < duration) {
            timer += Time.deltaTime;
            cursortimer += Time.deltaTime;
            if (cursortimer > cursorDuration) {
                cursorvisible = !cursorvisible;
                cursortimer -= cursorDuration;
            }
            int numberCharacters = (int)(totalText.Length * (timer / duration));
            string currentText = totalText.Substring(0, numberCharacters);

            if (cursorvisible) {
                currentText += "<sprite=1>";
            }

            text.text = currentText;
            yield return null;
        }
        text.text = totalText;
    }

}



// push a button to fast forward coroutines and show continue button