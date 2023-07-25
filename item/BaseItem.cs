using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;


namespace Items {
    public class BaseItem {
        [JsonConverter(typeof(ScriptableObjectJsonConverter<ItemTemplate>))]
        public ItemTemplate data;
        public BaseItem(ItemTemplate baseData) {
            this.data = baseData;
        }
        public virtual ItemUseResult Use(ItemHandler handler, PlayerInput input) => ItemUseResult.Empty();
        public virtual bool EnablesManualHack() => false;
        public virtual bool EnablesBurglary() => false;
        static BaseItem FactoryLoad(string baseName) {
            if (baseName == "") return null;
            ItemTemplate baseItem = ItemTemplate.LoadItem(baseName);
            return baseItem switch {
                GrenadeData grenadeData => new GrenadeItem(grenadeData),
                RocketLauncherData rocketData => new RocketLauncherItem(rocketData),
                C4Data c4Data => new C4(c4Data),
                IRGoggleData goggles => new IRGoggles(goggles),
                ItemTemplate itemData => itemData.shortName switch {
                    "deck" => new CyberDeck(itemData),
                    "tools" => new BurglarTools(itemData),
                    _ => new BaseItem(baseItem)
                },
                _ => new BaseItem(baseItem)
            };
        }

        public static BaseItem LoadItem(string itemName) {
            if (itemName == "") return null;
            BaseItem newItem = BaseItem.FactoryLoad(itemName);
            if (newItem != null) {
                return newItem;
            } else {
                Debug.LogError($"unable to load plan item {itemName}");
                return null;
            }
        }
    }
}


