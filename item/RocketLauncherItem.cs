using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
namespace Items {
    public class RocketLauncherItem : ItemInstance {
        [JsonConverter(typeof(ScriptableObjectJsonConverter<RocketLauncherData>))]
        public RocketLauncherData rocketData;
        public RocketLauncherItem(RocketLauncherData rocketData) : base(rocketData) {
            this.rocketData = rocketData;
        }
        protected override ItemUseResult DoUse(ItemHandler handler, PlayerInput input) {
            handler.rocketLauncher.ShootRocket(this, input, handler);
            Toolbox.RandomizeOneShot(handler.audioSource, rocketData.shootSound);
            return base.DoUse(handler, input);
        }
    }
}

