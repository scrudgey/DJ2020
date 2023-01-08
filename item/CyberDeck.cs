using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Items {
    public class CyberDeck : BaseItem {
        public CyberDeck(ItemData baseData) : base(baseData) { }
        public override ItemUseResult Use(ItemHandler handler) {
            return base.Use(handler);
        }
        public override bool EnablesManualHack() => true;
    }
}

