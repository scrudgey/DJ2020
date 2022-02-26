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
    void Awake() {
        audioSource = Toolbox.SetUpAudioSource(gameObject);
    }
    void Start() {
        OnItemEnter(activeItem);
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
    public void ProcessInput(PlayerCharacterInput input) {
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
            case CyberDeck cyberDeck:
                GameManager.I.SetOverlay(OverlayType.cyber);
                break;
            default:
                break;
        }
    }
    void OnItemExit(BaseItem item) {
        switch (item) {
            case CyberDeck cyberDeck:
                GameManager.I.SetOverlay(OverlayType.none);
                break;
            default:
                break;
        }
    }
}
