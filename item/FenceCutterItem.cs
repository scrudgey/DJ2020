using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
namespace Items {
    public class FenceCutterItem : ItemInstance {
        [JsonConverter(typeof(ScriptableObjectJsonConverter<FenceCutterTemplate>))]
        public FenceCutterTemplate fenceCutterTemplate;
        public FenceCutterItem(FenceCutterTemplate fenceCutterTemplate) : base(fenceCutterTemplate) {
            this.fenceCutterTemplate = fenceCutterTemplate;
        }
        protected override ItemUseResult DoUse(ItemHandler handler, PlayerInput input) {
            handler.FenceCutterSnip();
            return base.DoUse(handler, input);
        }

        public void PlaySnipSound(ItemHandler handler) {
            Toolbox.RandomizeOneShot(handler.audioSource, fenceCutterTemplate.snipSound);
        }
    }
}

