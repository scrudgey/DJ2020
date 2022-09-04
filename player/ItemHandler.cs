using System;
using System.Collections;
using System.Collections.Generic;
using Items;
using UnityEngine;
// TODO: enable buffs on/off

public class ItemHandler : MonoBehaviour, IBindable<ItemHandler>, ISaveable, IInputReceiver {
    public Action<ItemHandler> OnValueChanged { get; set; }

    public List<BaseItem> items = new List<BaseItem>();
    public int index;
    public BaseItem activeItem;
    public AudioSource audioSource;
    public readonly float SUSPICION_TIMEOUT = 1.5f;
    void Awake() {
        audioSource = Toolbox.SetUpAudioSource(gameObject);
    }
    void Start() {
        OnItemEnter(activeItem);
    }

    public void SetSuspicion(Suspiciousness target, float timeout) {
        SuspicionRecord record = new SuspicionRecord {
            content = "a suspicious item was used",
            suspiciousness = target,
            lifetime = timeout,
            maxLifetime = timeout
        };
        GameManager.I.AddSuspicionRecord(record);
    }
    public void SetInputs(PlayerInput input) {
        if (input.incrementItem != 0) {
            index += input.incrementItem;
            if (index < 0) {
                index = items.Count - 1;
            } else if (index >= items.Count) {
                index = 0;
            }
            SwitchToItem(items[index]);
        }
        if (input.useItem) {
            UseItem();
        }
    }
    void SwitchToItem(BaseItem item) {
        OnItemExit(this.activeItem);
        this.activeItem = item;
        OnItemEnter(this.activeItem);
        OnValueChanged?.Invoke(this);
    }
    public void LoadState(PlayerData data) {
        items = new List<BaseItem>();
        foreach (string itemName in data.items) {
            BaseItem newItem = ItemInstance.NewInstance(itemName);
            if (newItem != null) {
                items.Add(newItem);
            } else {
                Debug.LogError($"unable to load saved item {itemName}");
            }
        }
        if (items.Count > 0) {
            SwitchToItem(items[0]);    // TODO: save active item
        }
    }

    void UseItem() {
        if (activeItem == null)
            return;
        activeItem.Use(this);
    }

    void OnItemEnter(BaseItem item) {
        switch (item) {
            case CyberDeck:
                GameManager.I.SetOverlay(OverlayType.cyber);
                break;
            case IRGoggles goggles:
                GameManager.I.gameData.playerData.cyberEyesThermalBuff = true;
                GameManager.OnEyeVisibilityChange?.Invoke(GameManager.I.gameData.playerData);
                Toolbox.RandomizeOneShot(audioSource, goggles.goggleData.wearSounds);
                break;
            default:
                break;
        }
    }
    void OnItemExit(BaseItem item) {
        switch (item) {
            case CyberDeck:
                GameManager.I.SetOverlay(OverlayType.none);
                break;
            case IRGoggles:
                GameManager.I.gameData.playerData.cyberEyesThermalBuff = false;
                GameManager.OnEyeVisibilityChange?.Invoke(GameManager.I.gameData.playerData);
                break;
            default:
                break;
        }
    }

}
