using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
namespace Items {
    public class GrenadeItem : ItemInstance {
        [JsonConverter(typeof(ScriptableObjectJsonConverter<GrenadeData>))]
        public GrenadeData grenadeData;
        public GrenadeItem(GrenadeData grenadeData) : base(grenadeData) {
            this.grenadeData = grenadeData;
            maxCount = GameManager.I.gameData.playerState.PerkNumberOfExplosives() + 1;
            count = maxCount;
        }
        protected override ItemUseResult DoUse(ItemHandler handler, PlayerInput input) {
            base.DoUse(handler, input);
            handler.ThrowGrenade(grenadeData, input);
            Toolbox.RandomizeOneShot(handler.audioSource, grenadeData.throwSound);
            return ItemUseResult.Empty() with {
                waveArm = true
            };
        }
    }
}

