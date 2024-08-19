using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
namespace Items {
    public class C4 : ItemInstance {
        [JsonConverter(typeof(ScriptableObjectJsonConverter<C4Data>))]
        public C4Data c4Data;
        public C4(C4Data baseItem) : base(baseItem) {
            this.c4Data = baseItem;
            maxCount = GameManager.I.gameData.playerState.PerkNumberOfExplosives();
            count = maxCount;
        }
        protected override ItemUseResult DoUse(ItemHandler handler, PlayerInput input) {
            base.DoUse(handler, input);
            Toolbox.RandomizeOneShot(handler.audioSource, c4Data.deploySound);
            GameObject c4 = GameObject.Instantiate(c4Data.prefab, handler.transform.position, Quaternion.identity);
            Explosive explosive = c4.GetComponentInChildren<Explosive>();
            explosive.data = c4Data.explosionData;
            SuspicionRecord record = SuspicionRecord.c4Suspicion();
            GameManager.I.AddSuspicionRecord(record);
            return ItemUseResult.Empty() with { crouchDown = true };
        }
    }

}