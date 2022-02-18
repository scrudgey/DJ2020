using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Items {
    public abstract class ItemInstance {
        public static BaseItem NewInstance(string baseName) {
            ItemData baseItem = ItemData.LoadItem(baseName);
            return baseItem switch {
                C4Data c4Data => new C4(c4Data),
                ItemData itemData => itemData.shortName switch {
                    "deck" => new CyberDeck(itemData),
                    _ => new BaseItem(baseItem)
                },
                _ => new BaseItem(baseItem)
            };
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
    }
}


