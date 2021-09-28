using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Items {
    public abstract class ItemInstance {
        abstract public ItemData data {
            get;
        }

        public static BaseItem NewInstance(string baseName) {
            ItemData baseItem = ItemData.LoadItem(baseName);
            switch (baseItem.type) {
                case ItemType.c4:
                    C4Data item = (C4Data)baseItem;
                    return new C4(item);
                case ItemType.deck:
                    return new CyberDeck(baseItem);
                default:
                    return new BaseItem(baseItem);
            }
        }

        public virtual void Use(ItemHandler handler) { }
    }

    public class BaseItem {
        protected ItemData baseData;
        public BaseItem(ItemData baseData) {
            this.baseData = baseData;
        }
        public virtual ItemData data {
            get { return baseData; }
        }
        public virtual void Use(ItemHandler handler) { }
    }
}


