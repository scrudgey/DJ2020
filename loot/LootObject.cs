using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LootObject : Interactive {
    public LootData data;
    public bool isStealing;
    public AudioClip[] pickupSounds;
    public Action onCollect;
    GameObject creditIndicator;
    override public void Start() {
        base.Start();
        creditIndicator = Resources.Load("prefabs/creditIndicator") as GameObject;
        PoolManager.I.RegisterPool(creditIndicator, poolSize: 20);
    }
    public override ItemUseResult DoAction(Interactor interactor) {
        Destroy(gameObject);
        GameManager.I.CollectLoot(data, transform.position);
        // interactor.RemoveInteractive(this);
        Toolbox.AudioSpeaker(transform.position, pickupSounds);
        bool waveArm = transform.position.y - interactor.transform.position.y > -0.25f;
        bool crouchDown = !waveArm;
        PoolManager.I.GetPool(creditIndicator).GetObject(transform.position);
        if (isStealing)
            GameManager.I.AddSuspicionRecord(SuspicionRecord.lootSuspicion(data.lootName));

        onCollect?.Invoke();
        CutsceneManager.I.HandleTrigger("got_loot");
        return ItemUseResult.Empty() with {
            crouchDown = crouchDown,
            waveArm = waveArm
        };
    }
    public override string ResponseString() {
        return $"picked up {data.lootName}";
    }
}
