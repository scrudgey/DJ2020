using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Credstick : Interactive {
    public int amount;
    public AudioClip[] pickupSounds;
    public override ItemUseResult DoAction(Interactor interactor) {
        Destroy(gameObject);
        GameManager.I.AddCredits(amount);
        interactor.RemoveInteractive(this);
        Toolbox.AudioSpeaker(transform.position, pickupSounds);
        return ItemUseResult.Empty() with { transitionToUseItem = true };
    }
    public override string ResponseString() {
        return $"picked up {amount} credits";
    }
}
