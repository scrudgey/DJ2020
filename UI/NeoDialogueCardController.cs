using System;
using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class NeoDialogueCardController : MonoBehaviour {
    public enum Mode { normal, discard }
    public Mode mode;
    public Button button;
    public Image discardMark;
    public bool markedForDiscard;
    public RectTransform cardRect;
    public RectTransform myRect;
    public TextMeshProUGUI title;
    public TextMeshProUGUI count;
    [Header("icon")]
    public GameObject topSpacer;
    public GameObject iconHolder;
    public Image icon;
    public Sprite dataIcon;
    public Sprite idIcon;
    public TextMeshProUGUI description;
    public GameObject statusRecordPrefab;
    public Transform statusContainer;
    float timer = 0;
    NeoDialogueMenu controller;
    public DialogueCard cardData;
    // bool isEscape;
    // bool isId;
    // bool isDataPlay;
    bool noStatusEffects;

    Action<NeoDialogueCardController> callback;

    public static readonly float BASE_WIDTH = 270f;

    public void Initialize(NeoDialogueMenu controller, DialogueInput input, DialogueCard cardData, Action<NeoDialogueCardController> callback) {
        this.controller = controller;
        this.callback = callback;
        this.cardData = cardData;

        myRect.sizeDelta = new Vector2(BASE_WIDTH, 750f);
        int derivedValue = cardData.derivedValue(input);
        count.text = $"-{derivedValue.ToString()} bullshit";

        title.text = $"<color=#ffa502>{cardData.type.ToString()}</color>";

        iconHolder.SetActive(false);
        topSpacer.SetActive(false);
        icon.enabled = false;
        count.enabled = true;
        description.text = $"Increase bullshit meter by {derivedValue} points";

        InitializeStatusContainer(input);
    }
    public void InitializeEscapeCard(NeoDialogueMenu controller, Action<NeoDialogueCardController> callback) {
        // this.controller = controller;
        this.callback = callback;
        noStatusEffects = true;

        // isEscape = true;
        title.text = "<color=#ff4757>Escape</color>";
        count.text = "";

        myRect.sizeDelta = new Vector2(BASE_WIDTH, 750f);
        iconHolder.SetActive(false);
        // topSpacer.SetActive(true);
        topSpacer.SetActive(false);

        icon.enabled = false;
        count.enabled = false;
        description.text = $"Escape the conversation. Opponent is momentarily stunned.";

        foreach (Transform child in statusContainer) {
            Destroy(child.gameObject);
        }
    }
    public void InitializeIDCard(NeoDialogueMenu controller, Action<NeoDialogueCardController> callback) {
        // this.controller = controller;
        this.callback = callback;
        noStatusEffects = true;

        // isId = true;
        title.text = "<color=#2ed573>ID Card</color>";
        count.text = $"";

        myRect.sizeDelta = new Vector2(BASE_WIDTH, 750f);
        iconHolder.SetActive(true);
        topSpacer.SetActive(false);
        icon.enabled = true;
        icon.sprite = idIcon;
        count.enabled = false;
        description.text = $"Use your ID card. Bypass this challenge.";

        foreach (Transform child in statusContainer) {
            Destroy(child.gameObject);
        }
    }
    public void InitializeDataCard(NeoDialogueMenu controller, Action<NeoDialogueCardController> callback) {
        // this.controller = controller;
        this.callback = callback;
        noStatusEffects = true;

        // isDataPlay = true;
        title.text = "<color=#2ed573>Data</color>";
        count.text = "-1 personnel data";

        myRect.sizeDelta = new Vector2(BASE_WIDTH, 750f);
        iconHolder.SetActive(true);
        topSpacer.SetActive(false);
        icon.enabled = true;
        icon.sprite = dataIcon;
        count.enabled = true;
        description.text = $"Use knowledge gained from stolen data to bypass this challenge. Consumes 1 personnel data.";

        foreach (Transform child in statusContainer) {
            Destroy(child.gameObject);
        }
    }

    public void InitializeStatusContainer(DialogueInput input) {
        if (noStatusEffects) return;
        foreach (Transform child in statusContainer) {
            Destroy(child.gameObject);
        }
        int derivedValue = cardData.derivedValue(input);
        count.text = $"<color=#ff4757>+{derivedValue.ToString()} bullshit</color>";
        CreateStatusElement($"base value", cardData.baseValue, plain: true);
        foreach (KeyValuePair<string, int> kvp in cardData.getStatusEffects(input)) {
            CreateStatusElement(kvp.Key, kvp.Value);
        }
    }
    void CreateStatusElement(string content, int alarmCount, bool plain = false) {
        GameObject statusObj = GameObject.Instantiate(statusRecordPrefab);
        statusObj.transform.SetParent(statusContainer, false);
        DialogueStatusEntry status = statusObj.GetComponent<DialogueStatusEntry>();
        status.InitializeNumeric(alarmCount, content, positiveIsBad: true, plain: plain);
    }

    public void OnMouseOver() {

    }
    void Update() {
        if (mode == Mode.discard && !noStatusEffects) {
            timer += Time.unscaledDeltaTime;
            cardRect.rotation = Quaternion.Euler(0f, 0f, 2f * Mathf.Sin(timer * 25f));
        }
    }
    public void ClickCallback() {
        if (mode == Mode.normal) {
            callback?.Invoke(this);
        } else {
            if (noStatusEffects) return;
            // mark for discard
            markedForDiscard = !markedForDiscard;
            discardMark.enabled = markedForDiscard;
            controller.CardModeChanged();
        }
    }
    public void SetMode(Mode mode) {
        this.mode = mode;
        cardRect.rotation = Quaternion.identity;
        timer = 0f;
        if (mode == Mode.discard) {
            markedForDiscard = false;
            if (noStatusEffects)
                button.interactable = false;
        } else {
            button.interactable = true;
            discardMark.enabled = false;
        }
    }

    public IEnumerator PlayCard() {
        IEnumerator mover = Toolbox.Ease(null, 0.4f, 0f, 500f, PennerDoubleAnimation.BackEaseOut, (amount) => {
            Vector2 newPos = new Vector2(cardRect.anchoredPosition.x, amount);
            cardRect.anchoredPosition = newPos;
        }, unscaledTime: true);
        IEnumerator deleteCard = Toolbox.CoroutineFunc(() => { cardRect.gameObject.SetActive(false); });
        return Toolbox.ChainCoroutines(mover, deleteCard);
    }

    public IEnumerator RemoveCard() {
        IEnumerator shrinker = Toolbox.Ease(null, 0.4f, BASE_WIDTH, 0f, PennerDoubleAnimation.ExpoEaseOut, (amount) => {
            myRect.sizeDelta = new Vector2(amount, 450f);
        }, unscaledTime: true);

        IEnumerator deleter = Toolbox.CoroutineFunc(() => { Destroy(gameObject); });
        return Toolbox.ChainCoroutines(shrinker, deleter);
    }

    public IEnumerator DrawCard() {
        cardRect.anchoredPosition = new Vector2(cardRect.anchoredPosition.x, -600f);
        IEnumerator grower = Toolbox.Ease(null, 0.4f, 0f, BASE_WIDTH, PennerDoubleAnimation.ExpoEaseOut, (amount) => {
            myRect.sizeDelta = new Vector2(amount, 450f);
        }, unscaledTime: true);
        IEnumerator mover = Toolbox.Ease(null, 0.4f, -600f, 0f, PennerDoubleAnimation.BackEaseOut, (amount) => {
            Vector2 newPos = new Vector2(cardRect.anchoredPosition.x, amount);
            cardRect.anchoredPosition = newPos;
        }, unscaledTime: true);
        IEnumerator activator = Toolbox.CoroutineFunc(() => button.interactable = true);
        return Toolbox.ChainCoroutines(grower, mover, activator);
    }
}
