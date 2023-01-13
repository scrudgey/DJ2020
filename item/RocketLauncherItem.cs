using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Items {
    public class RocketLauncherItem : BaseItem {
        public RocketLauncherData rocketData;
        public RocketLauncherItem(RocketLauncherData rocketData) : base(rocketData) {
            this.rocketData = rocketData;
        }
        public override ItemUseResult Use(ItemHandler handler, PlayerInput input) {
            handler.rocketLauncher.ShootRocket(this, input, handler);
            Toolbox.RandomizeOneShot(handler.audioSource, rocketData.shootSound);
            return base.Use(handler, input);
        }
    }
}

