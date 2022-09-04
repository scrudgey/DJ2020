using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Items {

    public class IRGoggles : BaseItem {
        public IRGoggleData goggleData;
        public IRGoggles(IRGoggleData baseItem) : base(baseItem) {
            this.goggleData = baseItem;
        }
        public override void Use(ItemHandler handler) {
            base.Use(handler);
            // handler.SetSuspicion(Suspiciousness.suspicious, 1f);
        }
    }
}

