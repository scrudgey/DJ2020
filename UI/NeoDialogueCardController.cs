using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NeoDialogueCardController : MonoBehaviour {
    public RectTransform cardRect;
    public RectTransform myRect;
    public TextMeshProUGUI title;
    public TextMeshProUGUI count;
    public Image icon;
    public TextMeshProUGUI description;
    public GameObject statusRecordPrefab;
    public Transform statusContainer;

    NeoDialogueMenu controller;
    public DialogueCard cardData;

    public void Initialize(NeoDialogueMenu controller, DialogueInput input, DialogueCard cardData) {
        this.controller = controller;
        this.cardData = cardData;

        myRect.sizeDelta = new Vector2(300f, 750f);
        int derivedValue = cardData.derivedValue(input);

        title.text = cardData.type.ToString();
        count.text = derivedValue.ToString();

        icon.enabled = cardData.type == DialogueTacticType.item;
        count.enabled = cardData.type != DialogueTacticType.item;
        description.text = $"Decrease bullshit meter by {derivedValue} points";

        InitializeStatusContainer(input);
    }

    public void InitializeStatusContainer(DialogueInput input) {
        foreach (Transform child in statusContainer) {
            Destroy(child.gameObject);
        }
        CreateStatusElement($"base value", cardData.baseValue, plain: true);
        foreach (KeyValuePair<string, int> kvp in cardData.getStatusEffects(input)) {
            CreateStatusElement(kvp.Key, kvp.Value);
        }
    }
    void CreateStatusElement(string content, int alarmCount, bool plain = false) {
        GameObject statusObj = GameObject.Instantiate(statusRecordPrefab);
        statusObj.transform.SetParent(statusContainer, false);
        DialogueStatusEntry status = statusObj.GetComponent<DialogueStatusEntry>();
        status.InitializeNumeric(alarmCount, content, plain: plain);
    }

    public void OnMouseOver() {

    }
    public void ClickCallback() {
        controller.CardClick(this);
    }

    public IEnumerator PlayCard() {
        IEnumerator mover = Toolbox.Ease(null, 0.4f, 0f, 600f, PennerDoubleAnimation.BackEaseOut, (amount) => {
            Vector2 newPos = new Vector2(cardRect.anchoredPosition.x, amount);
            cardRect.anchoredPosition = newPos;
        }, unscaledTime: true);
        IEnumerator deleteCard = Toolbox.CoroutineFunc(() => { cardRect.gameObject.SetActive(false); });
        return Toolbox.ChainCoroutines(mover, deleteCard);
    }

    public IEnumerator RemoveCard() {
        IEnumerator shrinker = Toolbox.Ease(null, 0.4f, 300f, 0f, PennerDoubleAnimation.ExpoEaseOut, (amount) => {
            myRect.sizeDelta = new Vector2(amount, 450f);
        }, unscaledTime: true);

        IEnumerator deleter = Toolbox.CoroutineFunc(() => { Destroy(gameObject); });
        return Toolbox.ChainCoroutines(shrinker, deleter);
    }

    public IEnumerator DrawCard() {
        cardRect.anchoredPosition = new Vector2(cardRect.anchoredPosition.x, -600f);
        IEnumerator grower = Toolbox.Ease(null, 0.4f, 0f, 300f, PennerDoubleAnimation.ExpoEaseOut, (amount) => {
            myRect.sizeDelta = new Vector2(amount, 450f);
        }, unscaledTime: true);
        IEnumerator mover = Toolbox.Ease(null, 0.4f, -600f, 0f, PennerDoubleAnimation.BackEaseOut, (amount) => {
            Vector2 newPos = new Vector2(cardRect.anchoredPosition.x, amount);
            cardRect.anchoredPosition = newPos;
        }, unscaledTime: true);
        return Toolbox.ChainCoroutines(grower, mover);
    }
}
