using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Items {
    public class CyberDeck : ItemInstance {
        public CyberDeck(ItemTemplate baseData) : base(baseData) { }
        public override bool EnablesManualHack() => true;
    }
}

