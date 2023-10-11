using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NeoAfterActionReport : MonoBehaviour {
    GameData gameData;
    enum State { missionName, reward }
    State state = State.missionName;
    public Image fade;
    public RectTransform reportMask;
    public RectTransform reportRect;
    public GameObject continueButton;
    public GameObject perkMenuButton;
    public Button perkMenuButtonComponent;

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
    int counterBalance;
    public void Initialize(GameData data) {
        this.gameData = data;
        fade.color = new Color(0, 0, 0, 1);
        targetHeight = 0f;
        continueButton.SetActive(false);
        perkMenuButton.SetActive(false);

        mutableBalance = data.playerState.credits - data.levelState.template.creditReward;
        counterBalance = data.levelState.template.creditReward;

        StartCoroutine(
                ShowTitleAndObjectives(data)
            );
    }
    void Update() {
        if (adjustMaskToRect != null) {
            float preferredHeight = LayoutUtility.GetPreferredHeight(adjustMaskToRect);
            targetHeight = Mathf.Lerp(targetHeight, preferredHeight, 1f);
            reportMask.sizeDelta = new Vector2(adjustMaskToRect.rect.width, targetHeight);
        }
        if (perkMenuButton.activeInHierarchy) {
            skillPointCounterText.text = $"{GameManager.I.gameData.playerState.skillpoints}";
            // perkMenuButtonComponent.interactable = GameManager.I.gameData.playerState.skillpoints > 0;
        }
    }
    public void ContinueButtonCallback() {
        switch (state) {
            case State.missionName:
                state = State.reward;
                continueButton.SetActive(false);
                StartCoroutine(ShowReward(gameData));
                break;
            case State.reward:
                continueButton.SetActive(false);
                StartCoroutine(FadeOut());
                break;
        }
    }
    public void PerkButtonCallback() {
        GameManager.I.ShowPerkMenu();
    }
    IEnumerator FadeIn() {
        yield return Toolbox.Ease(null, 2f, 1f, 0f, PennerDoubleAnimation.Linear, (amount) => fade.color = new Color(0, 0, 0, amount), unscaledTime: true);
    }
    IEnumerator FadeOut() {
        yield return Toolbox.Ease(null, 2f, 0f, 1f, PennerDoubleAnimation.Linear, (amount) => fade.color = new Color(0, 0, 0, amount), unscaledTime: true);
        GameManager.I.StartNewDay();
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

        List<Objective> requiredObjectives = data.levelState.template.objectives;
        List<Objective> optionalObjectives = data.levelState.template.bonusObjectives;

        objectivesContainer.gameObject.SetActive(true);
        foreach (Objective objective in requiredObjectives) {
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
            foreach (Objective objective in optionalObjectives) {
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
        bonusAmountText.text = "0";
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

    AfterActionReportObjectiveHandler CreateObjectiveHandler(Transform container, Objective objective, GameData data) {
        GameObject obj = GameObject.Instantiate(objectivePrefab);
        obj.transform.SetParent(container, false);
        AfterActionReportObjectiveHandler handler = obj.GetComponent<AfterActionReportObjectiveHandler>();
        ObjectiveStatus status = data.levelState.delta.objectivesState[objective];
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