using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Items {
    public abstract class ItemInstance {
        static BaseItem NewInstance(string baseName) {
            if (baseName == "") return null;
            ItemData baseItem = ItemData.LoadItem(baseName);
            return baseItem switch {
                C4Data c4Data => new C4(c4Data),
                IRGoggleData goggles => new IRGoggles(goggles),
                ItemData itemData => itemData.shortName switch {
                    "deck" => new CyberDeck(itemData),
                    "tools" => new BurglarTools(itemData),
                    _ => new BaseItem(baseItem)
                },
                _ => new BaseItem(baseItem)
            };
        }

        public static BaseItem LoadItem(string itemName) {
            if (itemName == "") return null;
            BaseItem newItem = ItemInstance.NewInstance(itemName);
            if (newItem != null) {
                return newItem;
            } else {
                Debug.LogError($"unable to load plan item {itemName}");
                return null;
            }
        }

        public virtual void Use(ItemHandler handler) { }
    }

    public class BaseItem {
        public ItemData data;
        public BaseItem(ItemData baseData) {
            this.data = baseData;
        }
        public virtual void Use(ItemHandler handler) { }
        public virtual bool EnablesManualHack() => false;
        public virtual bool EnablesBurglary() => false;
    }
}


