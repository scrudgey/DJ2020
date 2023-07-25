using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
namespace Items {

    public class IRGoggles : ItemInstance {
        [JsonConverter(typeof(ScriptableObjectJsonConverter<IRGoggleData>))]
        public IRGoggleData goggleData;
        public IRGoggles(IRGoggleData baseItem) : base(baseItem) {
            this.goggleData = baseItem;
        }
        public override ItemUseResult Use(ItemHandler handler, PlayerInput input) {
            return base.Use(handler, input);
        }
    }
}

