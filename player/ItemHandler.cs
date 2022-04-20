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
    public Suspiciousness suspiciousness = Suspiciousness.normal;
    public float suspicionTimer;
    public readonly float SUSPICION_TIMEOUT = 1.5f;
    void Awake() {
        audioSource = Toolbox.SetUpAudioSource(gameObject);
    }
    void Start() {
        OnItemEnter(activeItem);
    }
    void Update() {
        if (suspicionTimer >= 0) {
            suspicionTimer -= Time.deltaTime;
            if (suspicionTimer <= 0) {
                if (suspiciousness > Suspiciousness.normal) {
                    suspiciousness = (Suspiciousness)((int)suspiciousness - 1);
                    suspicionTimer = SUSPICION_TIMEOUT;
                }
            }
        }
    }
    public void SetSuspicion(Suspiciousness target, float timeout) {
        suspiciousness = target;
        suspicionTimer = timeout;
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

    public Suspiciousness GetSuspiciousness() {
        return suspiciousness;
    }
}
