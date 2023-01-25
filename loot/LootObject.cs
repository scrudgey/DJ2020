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
        bool waveArm = transform.position.y - interactor.transform.position.y > -0.25f;
        bool crouchDown = !waveArm;
        // Debug.Log(transform.position.y - interactor.transform.position.y);
        return ItemUseResult.Empty() with {
            crouchDown = crouchDown,
            waveArm = waveArm
        };
    }
    public override string ResponseString() {
        return $"picked up {data.lootName}";
    }
}
