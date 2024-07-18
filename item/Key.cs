using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : Interactive {
    public KeyType type;
    public int keyId;
    public AudioClip[] pickupSounds;

    public bool suspicious;
    public override ItemUseResult DoAction(Interactor interactor) {
        Destroy(gameObject);
        GameManager.I.AddKey(keyId, type, transform.position);
        // interactor.RemoveInteractive(this);
        Toolbox.AudioSpeaker(transform.position, pickupSounds);
        if (suspicious) {
            string lootName = type switch {
                KeyType.keycard => "keycard",
                KeyType.physical => "key",
                _ => "key"
            };
            GameManager.I.AddSuspicionRecord(SuspicionRecord.lootSuspicion(lootName));

        }
        return ItemUseResult.Empty() with { crouchDown = true };
    }
    public override string ResponseString() {
        return $"picked up key {keyId}";
    }
}
