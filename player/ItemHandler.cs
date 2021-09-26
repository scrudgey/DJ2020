using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ItemHandler : MonoBehaviour, ISaveable {
    public Action<ItemHandler> OnValueChanged;
    public List<Item> items = new List<Item>();
    public int index;
    public Item activeItem;
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
    }

    void SwitchToItem(Item item) {
        this.activeItem = item;
        OnValueChanged?.Invoke(this);
    }
    public void LoadState(PlayerData data) {
        items = new List<Item>();
        foreach (string itemName in data.items) {
            // Debug.Log($"loadin {itemName}");
            Item newItem = Item.LoadItem(itemName);
            if (newItem != null) {
                items.Add(newItem);
            } else {
                Debug.LogError($"unable to load saved item {itemName}");
            }
        }
    }
}
