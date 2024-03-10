using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class VRDataStore : Interactive, INodeBinder<CyberNode> {
    public CyberNode node { get; set; }
    public AudioSource audioSource;
    public AudioClip openSound;
    public Action<VRDataStore> OnDataStoreOpened;
    public SpriteRenderer calloutSprite;
    public ParticleSystem particles;
    public override void Start() {
        base.Start();
        audioSource = Toolbox.SetUpAudioSource(gameObject);
        DeactivateCallout();
    }
    public void HandleNodeChange() {
        if (node.compromised) {
            Open();
        }
    }
    public void Open() {
        Toolbox.RandomizeOneShot(audioSource, openSound);
        OnDataStoreOpened?.Invoke(this);
    }
    public void PlayParticles() {
        particles.Play();
    }
    public void ActivateCallout() {
        calloutSprite.enabled = true;
    }
    public void DeactivateCallout() {
        calloutSprite.enabled = false;
    }
    public override ItemUseResult DoAction(Interactor interactor) {
        return ItemUseResult.Empty() with { waveArm = true };
    }
}
