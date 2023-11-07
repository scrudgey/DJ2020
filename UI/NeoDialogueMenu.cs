using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class NeoDialogueMenu : MonoBehaviour {
    public enum DialogueResult { success, fail, stun }
    // static public Action<DialogueResult> OnDialogueConclude;
    public NeoDialogueController neoDialogueController;
    public NeoDialogueBullshitMeter bullshitMeter;
    public RectTransform bullshitRect;
    public Transform cardContainer;
    public RectTransform cardContainerRect;
    public GameObject cardPrefab;
    public Action<DialogueResult> concludeCallback;
    public GameObject continueButton;
    public GameObject endButton;
    public Action continueButtonAction;
    // public Action en
    Stack<SuspicionRecord> unresolvedSuspicionRecords;
    DialogueInput input;
    DialogueResult dialogueResult;
    SuspicionRecord currentChallenge;

    public void Initialize(DialogueInput input, Action<DialogueResult> onConcludeCallback) {
        this.input = input;
        this.concludeCallback = onConcludeCallback;

        unresolvedSuspicionRecords = new Stack<SuspicionRecord>(input.suspicionRecords.Values);
        continueButton.SetActive(false);
        endButton.gameObject.SetActive(false);
        neoDialogueController.Initialize(input);
        bullshitMeter.Initialize(input);
        bullshitRect.anchoredPosition = new Vector2(-1425.18f, -224f);

        InitializeCards();
        cardContainerRect.anchoredPosition = new Vector2(cardContainerRect.anchoredPosition.x, -850f);
        SuspicionRecord identityChallenge = SuspicionRecord.identitySuspicion(input);


        IEnumerator easeInBullshit = Toolbox.Ease(null, 1f, -1425.18f, -885.66f, PennerDoubleAnimation.ExpoEaseOut, (amount) => {
            bullshitRect.anchoredPosition = new Vector2(amount, -224f);
        }, unscaledTime: true);
        // set current bullshit- controlled by level delta
        IEnumerator bullshitter = bullshitMeter.SetTargetBullshit(GameManager.I.gameData.levelState.delta.bullshitLevel);

        // set bullshit threshold 
        IEnumerator threshold = bullshitMeter.SetBullshitThreshold(65);

        IEnumerator challenge = StartNextChallenge(manualSuspicionRecord: identityChallenge);

        StartCoroutine(Toolbox.ChainCoroutines(
            new WaitForSecondsRealtime(0.5f),
            easeInBullshit,
            threshold,
            bullshitter,
            new WaitForSecondsRealtime(1f),
            challenge
            ));
    }
    void InitializeCards() {
        foreach (Transform child in cardContainer) {
            Destroy(child.gameObject);
        }
        foreach (DialogueCard card in GameManager.I.gameData.levelState.delta.dialogueCards) {
            InstantiateCard(card);
        }
    }
    NeoDialogueCardController InstantiateCard(DialogueCard card) {
        GameObject cardObj = Instantiate(cardPrefab);
        cardObj.transform.SetParent(cardContainer, false);
        NeoDialogueCardController cardController = cardObj.GetComponent<NeoDialogueCardController>();
        cardController.Initialize(this, input, card);
        return cardController;
    }
    void Conclude() {
        concludeCallback.Invoke(dialogueResult);
        GameManager.I.CloseMenu();
    }
    IEnumerator StartNextChallenge(SuspicionRecord manualSuspicionRecord = null) {
        currentChallenge = manualSuspicionRecord == null ? unresolvedSuspicionRecords.Pop() : manualSuspicionRecord;
        GameManager.I.gameData.levelState.delta.bullshitLevel += currentChallenge.challengeValue;

        IEnumerator blitter = neoDialogueController.SetLeftDialogueText(currentChallenge.dialogue.challenge, $"Challenge: {currentChallenge.challengeValue}");
        IEnumerator bullshitter = bullshitMeter.SetTargetBullshit(GameManager.I.gameData.levelState.delta.bullshitLevel);
        IEnumerator activateCards = ActivateCards(true);
        return Toolbox.ChainCoroutines(blitter, bullshitter, activateCards);
    }


    IEnumerator StartConclusion() {
        if (GameManager.I.gameData.levelState.delta.bullshitLevel > bullshitMeter.bullshitThreshold) {
            dialogueResult = DialogueResult.fail;
        } else {
            dialogueResult = DialogueResult.success;
        }

        // IEnumerator deactivatecards = ActivateCards(false);
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
        return Toolbox.Ease(null, 0.5f, start, end, PennerDoubleAnimation.Linear, (amount) => {
            cardContainerRect.anchoredPosition = new Vector2(cardContainerRect.anchoredPosition.x, amount);
        }, unscaledTime: true);
    }

    public void CardClick(NeoDialogueCardController cardController) {
        DialogueTactic response = currentChallenge.getResponse(cardController.cardData.type);
        GameManager.I.gameData.levelState.delta.dialogueCards.Remove(cardController.cardData);
        GameManager.I.gameData.levelState.delta.bullshitLevel -= cardController.cardData.derivedValue(input);

        if (GameManager.I.gameData.levelState.delta.lastTactics.Count > 0 && GameManager.I.gameData.levelState.delta.lastTactics.Peek() != cardController.cardData.type) {
            GameManager.I.gameData.levelState.delta.lastTactics = new Stack<DialogueTacticType>();
        }
        GameManager.I.gameData.levelState.delta.lastTactics.Push(cardController.cardData.type);
        foreach (NeoDialogueCardController card in cardContainer.GetComponentsInChildren<NeoDialogueCardController>()) {
            card.InitializeStatusContainer(input);
        }
        // Debug.Log("cardclick " + response.content);

        IEnumerator cardPlay = cardController.PlayCard();

        IEnumerator blitter = neoDialogueController.SetRightDialogueText($"<color=#2ed573>[{response.tacticType}]</color> {response.content}");

        IEnumerator removeCard = cardController.RemoveCard();

        IEnumerator newCard = DrawNewCard();

        IEnumerator deactivatecards = ActivateCards(false);

        IEnumerator continuer = Toolbox.CoroutineFunc(() => { continueButton.SetActive(true); });
        IEnumerator antiContinuer = Toolbox.CoroutineFunc(() => { continueButton.SetActive(false); });

        IEnumerator bullshitter = bullshitMeter.SetTargetBullshit(GameManager.I.gameData.levelState.delta.bullshitLevel);

        IEnumerator responseBlit;
        if (UnityEngine.Random.Range(0f, 1f) < 0.5f) {
            responseBlit = neoDialogueController.SetLeftDialogueText(response.successResponse, "");
        } else {
            responseBlit = neoDialogueController.SetLeftDialogueText(response.failResponse, "");
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
            newCard,
            new WaitForSecondsRealtime(1f),
            deactivatecards,
            recalculateCards,
            blitter,
            new WaitForSecondsRealtime(0.5f),
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


    IEnumerator DrawNewCard() {
        yield return null;
        DialogueCard cardData = GameManager.I.gameData.playerState.NewDialogueCard();
        GameManager.I.gameData.levelState.delta.dialogueCards.Add(cardData);
        NeoDialogueCardController newCard = InstantiateCard(cardData);
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
            content = "You're coming with me, creep!";
        }
        // continueButtonAction = () => { Conclude(); };
        IEnumerator anticontinuer = Toolbox.CoroutineFunc(() => { continueButton.SetActive(false); });
        IEnumerator concludingRemark = neoDialogueController.SetLeftDialogueText(content, "");
        IEnumerator continuer = Toolbox.CoroutineFunc(() => { endButton.SetActive(true); });
        StartCoroutine(Toolbox.ChainCoroutines(anticontinuer, concludingRemark, continuer));
    }

    public void EndButtonCallback() {
        Conclude();
    }
}

