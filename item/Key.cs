using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : Interactive {
    public int keyId;
    public AudioClip[] pickupSounds;
    public override void DoAction(Interactor interactor) {
        Destroy(gameObject);
        GameManager.I.gameData.playerState.physicalKeys.Add(keyId);
        interactor.RemoveInteractive(this);
        Toolbox.AudioSpeaker(transform.position, pickupSounds);
    }
    public override string ResponseString() {
        return $"picked up key {keyId}";
    }
}
