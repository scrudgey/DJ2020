using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Items {
    public class GrenadeItem : BaseItem {
        public GrenadeData grenadeData;
        public GrenadeItem(GrenadeData grenadeData) : base(grenadeData) {
            this.grenadeData = grenadeData;
        }
        public override ItemUseResult Use(ItemHandler handler, PlayerInput input) {
            base.Use(handler, input);
            handler.ThrowGrenade(grenadeData, input);
            Toolbox.RandomizeOneShot(handler.audioSource, grenadeData.throwSound);
            return new ItemUseResult {
                waveArm = true
            };
        }
    }
}
