using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : Interactive {
    public DoorLock.LockType type;
    public int keyId;
    public AudioClip[] pickupSounds;
    public override ItemUseResult DoAction(Interactor interactor) {
        Destroy(gameObject);
        // switch (type) {
        //     case DoorLock.LockType.physical:
        //         GameManager.I.AddPhysicalKey(keyId);
        //         break;
        //     case DoorLock.LockType.keycard:
        //         GameManager.I.AddKeyCard(keyId);
        //         break;
        // }
        GameManager.I.AddKey(keyId, type);
        interactor.RemoveInteractive(this);
        Toolbox.AudioSpeaker(transform.position, pickupSounds);
        return ItemUseResult.Empty() with { crouchDown = true };
    }
    public override string ResponseString() {
        return $"picked up key {keyId}";
    }
}
