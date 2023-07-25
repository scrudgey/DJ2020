using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Items {
    public class CyberDeck : ItemInstance {
        public CyberDeck(ItemTemplate baseData) : base(baseData) { }
        public override ItemUseResult Use(ItemHandler handler, PlayerInput input) {
            return base.Use(handler, input);
        }
        public override bool EnablesManualHack() => true;
    }
}

