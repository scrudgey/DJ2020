using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using Nimrod;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class NeoDialogueMenu : MonoBehaviour, PerkIdConstants {
    public enum DialogueResult { success, fail, stun }
    public NeoDialogueController neoDialogueController;
    public NeoDialogueBullshitMeter bullshitMeter;
    public RectTransform bullshitRect;
    public Transform cardContainer;
    public RectTransform cardContainerRect;
    public GameObject cardPrefab;
    public Action<DialogueResult> concludeCallback;

    public Action continueButtonAction;
    public NeoDialogueCardController specialCard;
    public NeoDialogueCardController escapeCard;
    public TextMeshProUGUI doubterText;
    public TextMeshProUGUI personnelDataCount;
    public Color red;
    [Header("buttons")]
    public GameObject continueButton;
    public GameObject endButton;
    public GameObject stallButton;
    public GameObject discardButtonObject;
    public GameObject easeButtonObject;
    public Button discardButton;
    public GameObject cancelDiscardButton;
    Grammar grammar;
    Stack<SuspicionRecord> unresolvedSuspicionRecords;

    SuspicionRecord failedSuspicionRecord;
    DialogueInput input;
    DialogueResult dialogueResult;
    SuspicionRecord currentChallenge;

    public void Initialize(DialogueInput input, Action<DialogueResult> onConcludeCallback) {
        this.input = input;
        this.concludeCallback = onConcludeCallback;
        grammar = new Grammar();
        grammar.Load("suspicion");

        stallButton.SetActive(false);
        easeButtonObject.SetActive(false);

        unresolvedSuspicionRecords = new Stack<SuspicionRecord>(input.suspicionRecords.Values);
        continueButton.SetActive(false);
        endButton.gameObject.SetActive(false);
        neoDialogueController.Initialize(input);
        bullshitMeter.Initialize(input);
        bullshitRect.anchoredPosition = new Vector2(-1425.18f, -224f);

        SetPersonnelDataCount();

        InitializeCards();
        cardContainerRect.anchoredPosition = new Vector2(cardContainerRect.anchoredPosition.x, -850f);
        SuspicionRecord identityChallenge = SuspicionRecord.identitySuspicion(input);


        IEnumerator easeInBullshit = Toolbox.Ease(null, 0.5f, -1425.18f, -885.66f, PennerDoubleAnimation.SineEaseOut, (amount) => {
            bullshitRect.anchoredPosition = new Vector2(amount, -224f);
        }, unscaledTime: true);

        // set current bullshit- controlled by level delta
        IEnumerator bullshitter = bullshitMeter.SetTargetBullshit(GameManager.I.gameData.levelState.delta.bullshitLevel, PulseDoubterColor);

        // set bullshit threshold 
        Dictionary<string, int> effects = getThresholdStatusEffects(input);
        IEnumerator threshold = bullshitMeter.SetBullshitThreshold(effects);

        IEnumerator challenge = StartNextChallenge(manualSuspicionRecord: identityChallenge);

        StartCoroutine(Toolbox.ChainCoroutines(
            new WaitForSecondsRealtime(0.5f),
            easeInBullshit,
            threshold,
            bullshitter,
            challenge
            ));
    }
    void SetPersonnelDataCount() {
        personnelDataCount.text = GameManager.I.gameData.levelState.delta.levelAcquiredPaydata.Where(data => data.type == PayData.DataType.personnel).Count().ToString();
    }
    void InitializeCards() {
        foreach (Transform child in cardContainer) {
            Destroy(child.gameObject);
        }
        foreach (DialogueCard card in GameManager.I.gameData.levelState.delta.dialogueCards) {
            InstantiateCard(card);
        }
        DrawUpToHand();
    }
    NeoDialogueCardController InstantiateCard(DialogueCard card) {
        GameObject cardObj = Instantiate(cardPrefab);
        cardObj.transform.SetParent(cardContainer, false);
        if (cardContainer.childCount > 1) {
            cardObj.transform.SetSiblingIndex(cardContainer.childCount - 2);
        }
        NeoDialogueCardController cardController = cardObj.GetComponent<NeoDialogueCardController>();
        cardController.Initialize(this, input, card, CardClick);
        return cardController;
    }
    NeoDialogueCardController InstantiateBlankCard() {
        GameObject cardObj = Instantiate(cardPrefab);
        cardObj.transform.SetParent(cardContainer, false);
        NeoDialogueCardController cardController = cardObj.GetComponent<NeoDialogueCardController>();
        return cardController;
    }
    void Conclude() {
        concludeCallback.Invoke(dialogueResult);
        GameManager.I.CloseMenu();
    }
    IEnumerator StartNextChallenge(SuspicionRecord manualSuspicionRecord = null) {
        yield return null;
        currentChallenge = manualSuspicionRecord == null ? unresolvedSuspicionRecords.Pop() : manualSuspicionRecord;
        stallButton.SetActive(false);
        easeButtonObject.SetActive(false);

        // rectify item cards
        RectifySpecialCards(currentChallenge);

        IEnumerator blitter = neoDialogueController.SetLeftDialogueText(currentChallenge.dialogue.challenge, "");
        IEnumerator activateCards = ActivateCards(true);

        yield return Toolbox.ChainCoroutines(blitter, activateCards);
    }

    void DrawUpToHand() {
        int numberDialogueCards = GameManager.I.gameData.playerState.PerkIsActivated(PerkIdConstants.PERKID_SPEECH_3CARD) ? 3 : 2;
        int numberCurrentCards = 0;
        foreach (NeoDialogueCardController cardController in cardContainer.GetComponentsInChildren<NeoDialogueCardController>()) {
            if (!cardController.noStatusEffects) numberCurrentCards++;
        }
        while (numberCurrentCards < numberDialogueCards) {
            numberCurrentCards++;
            StartCoroutine(DrawNewCard());
        }
    }

    void RectifySpecialCards(SuspicionRecord challenge) {
        if (specialCard != null) {
            Destroy(specialCard.gameObject);
        }
        if (escapeCard != null) {
            Destroy(escapeCard.gameObject);
        }
        if (challenge.allowIDCardResponse) {
            NeoDialogueCardController cardController = InstantiateBlankCard();
            cardController.InitializeIDCard(this, IDCardClick);
            specialCard = cardController;
        }

        if (challenge.allowDataResponse) {
            List<PayData> personnelData = GameManager.I.gameData.levelState.delta.levelAcquiredPaydata.Where(data => data.type == PayData.DataType.personnel).ToList();
            if (personnelData.Count > 0) {
                NeoDialogueCardController cardController = InstantiateBlankCard();
                cardController.InitializeDataCard(this, DataCardClick);
                specialCard = cardController;
            }
        }
        escapeCard = InstantiateBlankCard();
        escapeCard.InitializeEscapeCard(this, EscapeCardClick);
    }

    IEnumerator StartConclusion() {
        if (GameManager.I.gameData.levelState.delta.bullshitLevel > bullshitMeter.bullshitThreshold) {
            dialogueResult = DialogueResult.fail;
        } else {
            dialogueResult = DialogueResult.success;
        }
        stallButton.SetActive(false);
        easeButtonObject.SetActive(false);


        continueButtonAction = SayFinalRemark;
        IEnumerator blitter = neoDialogueController.SetLeftDialogueText($"Hmm...", "");
        IEnumerator ender = Toolbox.CoroutineFunc(() => { continueButton.gameObject.SetActive(true); });
        yield return Toolbox.ChainCoroutines(blitter, ender);
    }

    IEnumerator ActivateCards(bool value) {
        float start;
        float end;
        if (value) {
            start = -850f;
            end = -250f;
        } else {
            start = -250f;
            end = -850f;
            // cardContainer.gameObject.SetActive(false);
        }
        foreach (Button button in cardContainer.GetComponentsInChildren<Button>()) {
            button.interactable = value;
        }
        IEnumerator mover = Toolbox.Ease(null, 0.5f, start, end, PennerDoubleAnimation.Linear, (amount) => {
            cardContainerRect.anchoredPosition = new Vector2(cardContainerRect.anchoredPosition.x, amount);
        }, unscaledTime: true);
        IEnumerator showStallButton = Toolbox.CoroutineFunc(() => {
            if (value) {
                if (GameManager.I.gameData.levelState.delta.stallsAvailable > 0)
                    stallButton.SetActive(GameManager.I.gameData.playerState.PerkIsActivated(PerkIdConstants.PERKID_SPEECH_STALL));
                // easeButtonObject.SetActive(true);
                MaybeEnableEaseButton();
                DrawUpToHand();
            }
        });
        return Toolbox.ChainCoroutines(mover, showStallButton);
    }

    public void CardClick(NeoDialogueCardController cardController) {
        DialogueTactic response = currentChallenge.getResponse(cardController.cardData.type);
        GameManager.I.gameData.levelState.delta.dialogueCards.Remove(cardController.cardData);
        GameManager.I.gameData.levelState.delta.bullshitLevel += cardController.cardData.derivedValue(input);
        stallButton.SetActive(false);
        easeButtonObject.SetActive(false);

        if (GameManager.I.gameData.levelState.delta.lastTactics.Count > 0 && GameManager.I.gameData.levelState.delta.lastTactics.Peek() != cardController.cardData.type) {
            GameManager.I.gameData.levelState.delta.lastTactics = new Stack<DialogueTacticType>();
        }
        GameManager.I.gameData.levelState.delta.lastTactics.Push(cardController.cardData.type);

        IEnumerator cardPlay = cardController.PlayCard();

        IEnumerator blitter = neoDialogueController.SetRightDialogueText($"<color=#2ed573>[{response.tacticType.ToString().ToUpper()}]</color> {response.getContent()}");

        IEnumerator removeCard = cardController.RemoveCard();

        IEnumerator deactivatecards = ActivateCards(false);

        IEnumerator continuer = Toolbox.CoroutineFunc(() => { continueButton.SetActive(true); });
        IEnumerator antiContinuer = Toolbox.CoroutineFunc(() => { continueButton.SetActive(false); });

        IEnumerator bullshitter = bullshitMeter.SetTargetBullshit(GameManager.I.gameData.levelState.delta.bullshitLevel, PulseDoubterColor);

        IEnumerator responseBlit;
        if (UnityEngine.Random.Range(0f, 1f) < 0.5f) {
            responseBlit = neoDialogueController.SetLeftDialogueText(response.getSuccessResponse(), "");
        } else {
            responseBlit = neoDialogueController.SetLeftDialogueText(response.getFailResponse(), "");
        }
        IEnumerator nextBit;
        // next challenge
        if (unresolvedSuspicionRecords.Count > 0) {
            nextBit = StartNextChallenge();
        } else {
            nextBit = StartConclusion();
        }

        IEnumerator recalculateCards = Toolbox.CoroutineFunc(() => {
            foreach (NeoDialogueCardController card in cardContainer.GetComponentsInChildren<NeoDialogueCardController>()) {
                card.InitializeStatusContainer(input);
            }
        });

        StartCoroutine(Toolbox.ChainCoroutines(
            cardPlay,
            removeCard,
            deactivatecards,
            recalculateCards,
            blitter,
            bullshitter,
            continuer
        ));

        continueButtonAction = () => {
            StartCoroutine(Toolbox.ChainCoroutines(
                antiContinuer,
                responseBlit,
                new WaitForSecondsRealtime(1f),
                nextBit
            ));
        };

    }
    SuspicionRecord GetFailSuspicion() {
        string resultString = GameManager.I.gameData.levelState.delta.lastTactics.First() switch {
            DialogueTacticType.bluff => "bluff called",
            DialogueTacticType.challenge => "tried to intimidate a guard",
            DialogueTacticType.deny => "obvious denial of facts",
            // DialogueTacticType.escape => "[ESCAPE]",
            DialogueTacticType.item => "used counterfeit credentials",
            DialogueTacticType.lie => "caught in a lie",
            DialogueTacticType.redirect => "shadiness",
            _ => "general awkwardness"
        };
        return new SuspicionRecord {
            content = resultString,
            suspiciousness = Suspiciousness.aggressive,
            lifetime = 120f,
            maxLifetime = 120f
        };
    }

    void MaybeEnableEaseButton() {
        if (GameManager.I.gameData.levelState.delta.bullshitLevel > 0)
            easeButtonObject.SetActive(GameManager.I.gameData.levelState.delta.easesAvailable > 0);
    }

    public void EscapeCardClick(NeoDialogueCardController cardController) {
        dialogueResult = DialogueResult.stun;
        stallButton.SetActive(false);
        easeButtonObject.SetActive(false);

        IEnumerator cardPlay = cardController.PlayCard();

        IEnumerator blitter = neoDialogueController.SetRightDialogueText("<color=#ff4757>[ESCAPE]</color> Excuse me, I think I left my identification in my car.");

        IEnumerator deactivatecards = ActivateCards(false);

        IEnumerator removeCard = cardController.RemoveCard();

        IEnumerator continuer = Toolbox.CoroutineFunc(() => {
            SuspicionRecord record = SuspicionRecord.fledSuspicion();
            GameManager.I.AddSuspicionRecord(record);
            endButton.SetActive(true);
        });

        StartCoroutine(Toolbox.ChainCoroutines(
            cardPlay,
            removeCard,
            deactivatecards,
            blitter,
            continuer
        ));
    }
    public void IDCardClick(NeoDialogueCardController cardController) {
        GameManager.I.gameData.levelState.delta.lastTactics = new Stack<DialogueTacticType>();
        GameManager.I.gameData.levelState.delta.lastTactics.Push(DialogueTacticType.item);
        stallButton.SetActive(false);
        easeButtonObject.SetActive(false);


        IEnumerator cardPlay = cardController.PlayCard();

        IEnumerator blitter = neoDialogueController.SetRightDialogueText($"<color=#2ed573>[ITEM]</color> Sure, check my ID card.");

        IEnumerator removeCard = cardController.RemoveCard();

        IEnumerator deactivatecards = ActivateCards(false);

        IEnumerator continuer = Toolbox.CoroutineFunc(() => { continueButton.SetActive(true); });
        IEnumerator antiContinuer = Toolbox.CoroutineFunc(() => { continueButton.SetActive(false); });

        IEnumerator responseBlit = neoDialogueController.SetLeftDialogueText("I see.", "");

        IEnumerator nextBit;
        // next challenge
        if (unresolvedSuspicionRecords.Count > 0) {
            nextBit = StartNextChallenge();
        } else {
            nextBit = StartConclusion();
        }

        IEnumerator recalculateCards = Toolbox.CoroutineFunc(() => {
            foreach (NeoDialogueCardController card in cardContainer.GetComponentsInChildren<NeoDialogueCardController>()) {
                card.InitializeStatusContainer(input);
            }
        });

        StartCoroutine(Toolbox.ChainCoroutines(
            cardPlay,
            removeCard,
            deactivatecards,
            recalculateCards,
            blitter,
            continuer
        ));

        continueButtonAction = () => {
            StartCoroutine(Toolbox.ChainCoroutines(
                antiContinuer,
                responseBlit,
                new WaitForSecondsRealtime(1f),
                nextBit
            ));
        };
    }

    public void DataCardClick(NeoDialogueCardController cardController) {
        GameManager.I.gameData.levelState.delta.lastTactics = new Stack<DialogueTacticType>();
        GameManager.I.gameData.levelState.delta.lastTactics.Push(DialogueTacticType.item);
        stallButton.SetActive(false);
        easeButtonObject.SetActive(false);

        List<PayData> personnelData = GameManager.I.gameData.levelState.delta.levelAcquiredPaydata.Where(data => data.type == PayData.DataType.personnel).ToList();
        if (personnelData.Count > 0) {
            GameManager.I.gameData.levelState.delta.levelAcquiredPaydata.Remove(personnelData[0]);
        }
        SetPersonnelDataCount();

        DialogueTactic response = currentChallenge.getResponse(DialogueTacticType.item);

        IEnumerator cardPlay = cardController.PlayCard();

        IEnumerator blitter = neoDialogueController.SetRightDialogueText($"<color=#2ed573>[DATA]</color> {response.getContent()}");

        IEnumerator removeCard = cardController.RemoveCard();

        IEnumerator deactivatecards = ActivateCards(false);

        IEnumerator continuer = Toolbox.CoroutineFunc(() => { continueButton.SetActive(true); });
        IEnumerator antiContinuer = Toolbox.CoroutineFunc(() => { continueButton.SetActive(false); });

        IEnumerator responseBlit = neoDialogueController.SetLeftDialogueText(response.getSuccessResponse(), "");

        IEnumerator nextBit;
        // next challenge
        if (unresolvedSuspicionRecords.Count > 0) {
            nextBit = StartNextChallenge();
        } else {
            nextBit = StartConclusion();
        }

        IEnumerator recalculateCards = Toolbox.CoroutineFunc(() => {
            foreach (NeoDialogueCardController card in cardContainer.GetComponentsInChildren<NeoDialogueCardController>()) {
                card.InitializeStatusContainer(input);
            }
        });

        StartCoroutine(Toolbox.ChainCoroutines(
            cardPlay,
            removeCard,
            deactivatecards,
            recalculateCards,
            blitter,
            continuer
        ));

        continueButtonAction = () => {
            StartCoroutine(Toolbox.ChainCoroutines(
                antiContinuer,
                responseBlit,
                new WaitForSecondsRealtime(1f),
                nextBit
            ));
        };
    }


    IEnumerator DrawNewCard() {
        yield return null;
        DialogueCard cardData = GameManager.I.gameData.playerState.NewDialogueCard();
        GameManager.I.gameData.levelState.delta.dialogueCards.Add(cardData);
        NeoDialogueCardController newCard = InstantiateCard(cardData);
        foreach (Button button in newCard.GetComponentsInChildren<Button>()) {
            button.interactable = false;
        }
        yield return newCard.DrawCard();
    }
    public void ContinueButtonCallback() {
        continueButtonAction?.Invoke();
    }
    public void SayFinalRemark() {
        string content = "content";
        if (dialogueResult == DialogueResult.success) {
            content = "Let me know if you see anything suspicious.";
        } else if (dialogueResult == DialogueResult.fail) {
            GameManager.I.AddSuspicionRecord(GetFailSuspicion());
            content = "You're coming with me, creep!";
        }
        IEnumerator anticontinuer = Toolbox.CoroutineFunc(() => { continueButton.SetActive(false); });
        IEnumerator concludingRemark = neoDialogueController.SetLeftDialogueText(content, "");
        IEnumerator continuer = Toolbox.CoroutineFunc(() => { endButton.SetActive(true); });
        StartCoroutine(Toolbox.ChainCoroutines(anticontinuer, concludingRemark, continuer));
    }

    public void EndButtonCallback() {
        Conclude();
    }

    public IEnumerator PulseDoubterColor() {
        doubterText.enabled = true;
        float timer = 0f;
        Color color = red;
        int pulses = 0;
        while (pulses < 3) {
            timer += Time.unscaledDeltaTime;
            float factor = (float)PennerDoubleAnimation.CircEaseIn(timer, 1f, -1f, 1f);
            doubterText.color = new Color(red.r, red.g, red.b, factor);
            if (timer > 1f) {
                pulses += 1;
                timer -= 1f;
            }
            yield return null;
        }
        doubterText.color = red;
        doubterText.enabled = false;
    }


    public Dictionary<string, int> getThresholdStatusEffects(DialogueInput input) {
        Dictionary<string, int> effects = new Dictionary<string, int>();

        int trustworthiness = input.playerState.GetPerkLevel(PerkIdConstants.PERKID_SPEECH_TRUST);
        if (trustworthiness > 0) {
            effects.Add("trustworthy", +5 * trustworthiness);
        }

        if (input.alarmActive) {
            effects.Add("alarm is active", -20);
        }
        switch (input.npcCharacter.alertness) {
            case Alertness.normal:
                effects.Add("normal posture", 0);
                break;
            case Alertness.alert:
                effects.Add("on alert", -10);
                break;
            case Alertness.distracted:
                effects.Add("distracted", 10);
                break;
        }
        switch (input.levelState.template.sensitivityLevel) {
            case SensitivityLevel.publicProperty:
                effects.Add("on public property", +10);
                break;
            case SensitivityLevel.semiprivateProperty:
            case SensitivityLevel.privateProperty:
                effects.Add("on private property", 0);
                break;
            case SensitivityLevel.restrictedProperty:
                effects.Add("in restricted area", -20);
                break;
        }

        return effects;
    }

    public void StallCallback() {
        StartCoroutine(neoDialogueController.SetRightDialogueText("Um..."));
        stallButton.SetActive(false);
        easeButtonObject.SetActive(false);

        discardButtonObject.SetActive(true);
        cancelDiscardButton.SetActive(true);
        discardButton.interactable = false;
        foreach (NeoDialogueCardController cardController in cardContainer.GetComponentsInChildren<NeoDialogueCardController>()) {
            cardController.SetMode(NeoDialogueCardController.Mode.discard);
        }
    }
    public void DiscardCallback() {
        GameManager.I.gameData.levelState.delta.stallsAvailable--;
        StartCoroutine(neoDialogueController.SetRightDialogueText("<color=#2ed573>[STALL]</color> Hold on, my cell phone is ringing."));
        int numberDiscarded = 0;
        foreach (NeoDialogueCardController cardController in cardContainer.GetComponentsInChildren<NeoDialogueCardController>()) {
            // do discard
            if (cardController.markedForDiscard) {
                numberDiscarded++;
                Destroy(cardController.gameObject);
                GameManager.I.gameData.levelState.delta.dialogueCards.Remove(cardController.cardData);
            }
            cardController.SetMode(NeoDialogueCardController.Mode.normal);
        }
        // draw new
        for (int i = 0; i < numberDiscarded; i++) {
            StartCoroutine(DrawNewCard());
        }
        stallButton.SetActive(false);
        easeButtonObject.SetActive(false);

        discardButtonObject.SetActive(false);
        cancelDiscardButton.SetActive(false);
    }
    public void CancelDiscardCallback() {
        StartCoroutine(neoDialogueController.SetRightDialogueText("Never mind."));
        stallButton.SetActive(true);
        // easeButtonObject.SetActive(true);
        MaybeEnableEaseButton();

        discardButtonObject.SetActive(false);
        cancelDiscardButton.SetActive(false);
        foreach (NeoDialogueCardController cardController in cardContainer.GetComponentsInChildren<NeoDialogueCardController>()) {
            cardController.SetMode(NeoDialogueCardController.Mode.normal);
        }
    }

    public void CardModeChanged() {
        int numberSelected = 0;
        foreach (NeoDialogueCardController cardController in cardContainer.GetComponentsInChildren<NeoDialogueCardController>()) {
            if (cardController.markedForDiscard) numberSelected++;
        }
        discardButton.interactable = numberSelected > 0;
    }

    public void EaseButtonCallback() {
        GameManager.I.gameData.levelState.delta.easesAvailable--;
        GameManager.I.gameData.levelState.delta.bullshitLevel -= (int)Toolbox.RandomGaussian(20, 30);
        easeButtonObject.SetActive(false);

        string content = grammar.Parse("{ease}");

        IEnumerator blitter = neoDialogueController.SetRightDialogueText($"<color=#2ed573>[EASE]</color> {content}");

        IEnumerator bullshitter = bullshitMeter.SetTargetBullshit(GameManager.I.gameData.levelState.delta.bullshitLevel, PulseDoubterColor);

        StartCoroutine(Toolbox.ChainCoroutines(
            blitter,
            bullshitter
        ));
    }
}

