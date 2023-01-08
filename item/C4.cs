using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Items {
    public class C4 : BaseItem {
        public C4Data c4Data;
        public C4(C4Data baseItem) : base(baseItem) {
            this.c4Data = baseItem;
        }
        public override ItemUseResult Use(ItemHandler handler) {
            base.Use(handler);
            Toolbox.RandomizeOneShot(handler.audioSource, c4Data.deploySound);
            GameObject c4 = GameObject.Instantiate(c4Data.prefab, handler.transform.position, Quaternion.identity);
            Explosive explosive = c4.GetComponentInChildren<Explosive>();
            explosive.data = c4Data;
            SuspicionRecord record = new SuspicionRecord {
                content = "seen placing infernal device",
                suspiciousness = Suspiciousness.aggressive,
                lifetime = 1f,
                maxLifetime = 1f
            };
            GameManager.I.AddSuspicionRecord(record);
            return ItemUseResult.Empty() with { transitionToUseItem = true };
        }
    }

}