using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootObject : Interactive {
    public LootData data;
    public AudioClip[] pickupSounds;
    GameObject creditIndicator;
    override public void Start() {
        base.Start();
        creditIndicator = Resources.Load("prefabs/creditIndicator") as GameObject;
        PoolManager.I.RegisterPool(creditIndicator, poolSize: 20);
    }
    public override ItemUseResult DoAction(Interactor interactor) {
        Destroy(gameObject);
        GameManager.I.CollectLoot(data);
        interactor.RemoveInteractive(this);
        Toolbox.AudioSpeaker(transform.position, pickupSounds);
        bool waveArm = transform.position.y - interactor.transform.position.y > -0.25f;
        bool crouchDown = !waveArm;
        PoolManager.I.GetPool(creditIndicator).GetObject(transform.position);
        GameManager.I.AddSuspicionRecord(SuspicionRecord.lootSuspicion(data));
        return ItemUseResult.Empty() with {
            crouchDown = crouchDown,
            waveArm = waveArm
        };
    }
    public override string ResponseString() {
        return $"picked up {data.lootName}";
    }
}
