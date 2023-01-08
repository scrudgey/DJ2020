using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Items;
using UnityEngine;
public record ItemUseResult {
    public bool transitionToUseItem;
    public bool waveArm;
    public static ItemUseResult Empty() => new ItemUseResult {

    };
}
public class ItemHandler : MonoBehaviour, IBindable<ItemHandler> {

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

    public ItemUseResult SetInputs(PlayerInput input) {
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
            return UseItem();
        } else return ItemUseResult.Empty();
    }
    void SwitchToItem(BaseItem item) {
        OnItemExit(this.activeItem);
        this.activeItem = item;
        OnItemEnter(this.activeItem);
        OnValueChanged?.Invoke(this);
    }
    void ClearItem() {
        SwitchToItem(null);
        index = items.IndexOf(null);
    }
    public void LoadItemState(string[] itemNames) {
        items = new List<BaseItem>();
        foreach (string itemName in itemNames) {
            BaseItem newItem = ItemInstance.LoadItem(itemName);
            items.Add(newItem);
        }
        items.Add(null);
        items = items.ToHashSet().ToList();
        ClearItem();
    }

    ItemUseResult UseItem() {
        if (activeItem == null)
            return ItemUseResult.Empty();
        return activeItem.Use(this);
    }

    void OnItemEnter(BaseItem item) {
        if (item == null)
            return;
        switch (item) {
            case CyberDeck:
                GameManager.I.SetOverlay(OverlayType.cyber);
                break;
            case IRGoggles goggles:
                GameManager.I.gameData.playerState.cyberEyesThermalBuff = true;
                GameManager.OnEyeVisibilityChange?.Invoke(GameManager.I.gameData.playerState);
                Toolbox.RandomizeOneShot(audioSource, goggles.goggleData.wearSounds);
                break;
            case BurglarTools:
                foreach (AttackSurface surface in GameObject.FindObjectsOfType<AttackSurface>()) {
                    surface.EnableOutline();
                }
                break;
            default:
                break;
        }
    }
    void OnItemExit(BaseItem item) {
        if (item == null)
            return;
        switch (item) {
            case CyberDeck:
                GameManager.I.SetOverlay(OverlayType.none);
                break;
            case IRGoggles:
                GameManager.I.gameData.playerState.cyberEyesThermalBuff = false;
                GameManager.OnEyeVisibilityChange?.Invoke(GameManager.I.gameData.playerState);
                break;
            case BurglarTools:
                foreach (AttackSurface surface in GameObject.FindObjectsOfType<AttackSurface>()) {
                    surface.DisableOutline();
                }
                break;
            default:
                break;
        }
    }

}
