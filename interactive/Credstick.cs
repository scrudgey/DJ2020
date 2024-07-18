using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Credstick : Interactive {
    public int amount;
    public AudioClip[] pickupSounds;
    GameObject creditIndicator;
    override public void Start() {
        base.Start();
    }
    public override ItemUseResult DoAction(Interactor interactor) {
        Destroy(gameObject);
        GameManager.I.AddCredits(amount, transform.position);
        Toolbox.AudioSpeaker(transform.position, pickupSounds);
        return ItemUseResult.Empty() with { crouchDown = true };
    }
    public override string ResponseString() {
        return $"picked up {amount} credits";
    }
}
