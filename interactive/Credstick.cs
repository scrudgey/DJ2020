using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Credstick : Interactive {
    public int amount;
    public AudioClip[] pickupSounds;
    public override void DoAction(Interactor interactor) {
        Destroy(gameObject);
        GameManager.I.gameData.playerState.credits += amount;
        interactor.RemoveInteractive(this);
        Toolbox.AudioSpeaker(transform.position, pickupSounds);
    }
    public override string ResponseString() {
        return $"picked up {amount} credits";
    }
}
