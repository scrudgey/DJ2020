using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;


namespace Items {
    public class ItemInstance {
        [JsonConverter(typeof(ScriptableObjectJsonConverter<ItemTemplate>))]
        public ItemTemplate template;
        public ItemInstance(ItemTemplate template) {
            this.template = template;
        }
        public virtual ItemUseResult Use(ItemHandler handler, PlayerInput input) => ItemUseResult.Empty();
        public virtual bool EnablesManualHack() => false;
        public virtual bool EnablesBurglary() => false;
        static ItemInstance FactoryLoad(string baseName) {
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
                    _ => new ItemInstance(baseItem)
                },
                _ => new ItemInstance(baseItem)
            };
        }

        public static ItemInstance LoadItem(string itemName) {
            if (itemName == "") return null;
            ItemInstance newItem = ItemInstance.FactoryLoad(itemName);
            if (newItem != null) {
                return newItem;
            } else {
                Debug.LogError($"unable to load plan item {itemName}");
                return null;
            }
        }
    }
}


