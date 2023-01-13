using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : Interactive {
    public int keyId;
    public AudioClip[] pickupSounds;
    public override ItemUseResult DoAction(Interactor interactor) {
        Destroy(gameObject);
        GameManager.I.AddPhysicalKey(keyId);
        interactor.RemoveInteractive(this);
        Toolbox.AudioSpeaker(transform.position, pickupSounds);
        return ItemUseResult.Empty() with { transitionToUseItem = true };
    }
    public override string ResponseString() {
        return $"picked up key {keyId}";
    }
}
