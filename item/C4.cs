using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Items {
    public class C4 : BaseItem {
        public C4Data c4Data;
        public override ItemData data {
            get {
                return c4Data;
            }
        }
        public C4(C4Data baseItem) : base(baseItem) {
            this.c4Data = baseItem;
        }
        public override void Use(ItemHandler handler) {
            base.Use(handler);
            Toolbox.RandomizeOneShot(handler.audioSource, c4Data.deploySound);
            GameObject c4 = GameObject.Instantiate(c4Data.prefab, handler.transform.position, Quaternion.identity);
            Explosive explosive = c4.GetComponentInChildren<Explosive>();
            explosive.data = c4Data;
        }
    }

}