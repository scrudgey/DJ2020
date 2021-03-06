using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Items {
    public class CyberDeck : BaseItem {
        public CyberDeck(ItemData baseData) : base(baseData) { }
        public override void Use(ItemHandler handler) {
            base.Use(handler);
            handler.SetSuspicion(Suspiciousness.suspicious, 1f);
        }
        public override bool EnablesManualHack() => true;
    }
}

