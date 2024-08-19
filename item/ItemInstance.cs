using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;


namespace Items {
    public class ItemInstance {
        [JsonConverter(typeof(ScriptableObjectJsonConverter<ItemTemplate>))]
        public ItemTemplate template;
        public int count = 1;
        public int maxCount;
        public ItemInstance(ItemTemplate template) {
            this.template = template;
            count = template.maxCount;
            maxCount = template.maxCount;
        }
        public ItemUseResult Use(ItemHandler handler, PlayerInput input) {
            if (count > 0) {
                if (template.consumable) {
                    count -= 1;
                }
                return DoUse(handler, input);
            } else {
                return ItemUseResult.Empty() with { emptyUse = true };
            }
        }

        protected virtual ItemUseResult DoUse(ItemHandler handler, PlayerInput input) => ItemUseResult.Empty();
        public static ItemInstance FactoryLoad(ItemTemplate baseItem) {
            if (baseItem == null) return null;
            return baseItem switch {
                GrenadeData grenadeData => new GrenadeItem(grenadeData),
                RocketLauncherData rocketData => new RocketLauncherItem(rocketData),
                C4Data c4Data => new C4(c4Data),
                IRGoggleData goggles => new IRGoggles(goggles),
                FenceCutterTemplate fenceCutterTemplate => new FenceCutterItem(fenceCutterTemplate),
                ItemTemplate itemData => itemData.shortName switch {
                    "deck" => new CyberDeck(itemData),
                    "tools" => new BurglarTools(itemData),
                    _ => new ItemInstance(baseItem)
                },
                _ => new ItemInstance(baseItem)
            };
        }
    }
}


