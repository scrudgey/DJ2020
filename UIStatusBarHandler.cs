using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIStatusBarHandler : MonoBehaviour {
    public GameObject effectPrefab;
    public Camera UICamera;
    public RectTransform effectContainer;
    [Header("stat numbers")]
    public TextMeshProUGUI creditAmount;
    public TextMeshProUGUI lootAmount;
    public TextMeshProUGUI dataAmount;
    public TextMeshProUGUI keysAmount;
    [Header("icons")]
    public Sprite creditSprite;
    public Sprite lootSprite;
    public Sprite dataSprite;
    public Sprite keySprite;

    int currentCredits;
    int currentLoots;
    int currentData;
    int currentKeys;
    public void Initialize() {
        GameManager.OnGameStateChange += HandleStateChange;
        SetInitialValues(GameManager.I.gameData);
    }
    void SetInitialValues(GameData data) {
        currentCredits = data.playerState.credits + data.levelState.delta.levelAcquiredCredits;
        currentLoots = data.playerState.loots.Count + data.levelState.delta.levelAcquiredLoot.Count;
        currentData = data.playerState.payDatas.Count + data.levelState.delta.levelAcquiredPaydata.Count;
        currentKeys = data.playerState.physicalKeys.Count + data.playerState.keycards.Count;
        SetTextAmounts();
    }

    void SetTextAmounts() {
        creditAmount.text = $"{currentCredits}";
        lootAmount.text = $"{currentLoots}";
        dataAmount.text = $"{currentData}";
        keysAmount.text = $"{currentKeys}";
    }

    void OnDestroy() {
        GameManager.OnGameStateChange -= HandleStateChange;
    }

    void HandleStateChange(StatusUpdateData data) {
        StartEffectCoroutine(data);
    }

    void StartEffectCoroutine(StatusUpdateData data) {
        GameObject obj = GameObject.Instantiate(effectPrefab) as GameObject;
        RectTransform rectTransform = obj.GetComponent<RectTransform>();
        rectTransform.SetParent(effectContainer);

        Image icon = obj.GetComponentInChildren<Image>();
        icon.sprite = data.type switch {
            StatusUpdateData.StatusType.credit => creditSprite,
            StatusUpdateData.StatusType.data => dataSprite,
            StatusUpdateData.StatusType.key => keySprite,
            StatusUpdateData.StatusType.loot => lootSprite
        };

        rectTransform.position = UICamera.WorldToScreenPoint(data.originLocation);

        Vector3 finalPosition = data.type switch {
            StatusUpdateData.StatusType.credit => creditAmount.transform.position,
            StatusUpdateData.StatusType.data => dataAmount.transform.position,
            StatusUpdateData.StatusType.key => keysAmount.transform.position,
            StatusUpdateData.StatusType.loot => lootAmount.transform.position
        };

        IEnumerator incremeter = data.type switch {
            StatusUpdateData.StatusType.credit => Toolbox.CoroutineFunc(() => {
                currentCredits += data.increment;
                SetTextAmounts();
            }),
            StatusUpdateData.StatusType.data => Toolbox.CoroutineFunc(() => {
                currentData += data.increment;
                SetTextAmounts();
            }),
            StatusUpdateData.StatusType.key => Toolbox.CoroutineFunc(() => {
                currentKeys += data.increment;
                SetTextAmounts();
            }),
            StatusUpdateData.StatusType.loot => Toolbox.CoroutineFunc(() => {
                currentLoots += data.increment;
                SetTextAmounts();
            }),
        };

        IEnumerator destroyer = Toolbox.CoroutineFunc(() => Destroy(obj));

        IEnumerator routine = Toolbox.ChainCoroutines(toCoroutine(rectTransform, finalPosition), destroyer, incremeter, PulseStatusAmount(data.type));
        StartCoroutine(routine);
    }

    IEnumerator PulseStatusAmount(StatusUpdateData.StatusType type) {
        Transform target = type switch {
            StatusUpdateData.StatusType.credit => creditAmount.transform,
            StatusUpdateData.StatusType.data => dataAmount.transform,
            StatusUpdateData.StatusType.key => keysAmount.transform,
            StatusUpdateData.StatusType.loot => lootAmount.transform
        };

        yield return Toolbox.Ease(null, 0.5f, 2f, 1f, PennerDoubleAnimation.BounceEaseOut, (amount) => target.localScale = amount * Vector3.one, unscaledTime: true);
    }

    IEnumerator toCoroutine(RectTransform rectTransform, Vector3 finalPosition) {
        Vector3 initialPosition = rectTransform.position;
        Vector3 displacement = finalPosition - rectTransform.position;
        yield return Toolbox.Ease(null, 0.5f, 0f, 1f, PennerDoubleAnimation.ExpoEaseIn, (amount) =>
            rectTransform.position = initialPosition + (amount * displacement), unscaledTime: true
        );
    }

    public void OnClick(string type) {
        StatusUpdateData.StatusType statusType = type switch {
            "credits" => StatusUpdateData.StatusType.credit,
            "loot" => StatusUpdateData.StatusType.loot,
            "data" => StatusUpdateData.StatusType.data,
            "keys" => StatusUpdateData.StatusType.key
        };
        GameManager.I.ShowMenu(MenuType.escapeMenu, () => {
            Debug.Log(statusType);
            // TODO: configure pause menu
        });
    }

}

public class StatusUpdateData {
    public enum StatusType { credit, loot, data, key }
    public StatusType type;
    public Vector3 originLocation;
    public int increment;

}