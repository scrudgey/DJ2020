using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootObject : Interactive {
    public LootData data;
    public AudioClip[] pickupSounds;
    public override ItemUseResult DoAction(Interactor interactor) {
        Destroy(gameObject);
        // GameManager.I.AddCredits(amount);
        GameManager.I.CollectLoot(data);
        interactor.RemoveInteractive(this);
        Toolbox.AudioSpeaker(transform.position, pickupSounds);
        return ItemUseResult.Empty() with { crouchDown = true };
    }
    public override string ResponseString() {
        return $"picked up {data.lootName}";
    }
}
