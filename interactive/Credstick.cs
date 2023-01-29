using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Credstick : Interactive {
    public int amount;
    public AudioClip[] pickupSounds;
    GameObject creditIndicator;
    override public void Start() {
        base.Start();
        creditIndicator = Resources.Load("prefabs/creditIndicator") as GameObject;
        PoolManager.I.RegisterPool(creditIndicator, poolSize: 20);
    }
    public override ItemUseResult DoAction(Interactor interactor) {
        Destroy(gameObject);
        GameManager.I.AddCredits(amount);
        interactor.RemoveInteractive(this);
        Toolbox.AudioSpeaker(transform.position, pickupSounds);
        PoolManager.I.GetPool(creditIndicator).GetObject(transform.position);
        return ItemUseResult.Empty() with { crouchDown = true };
    }
    public override string ResponseString() {
        return $"picked up {amount} credits";
    }
}
