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
        // cyberComponent.OnStateChange += HandleCyberStateChange;
        DeactivateCallout();
    }
    // void OnDestroy() {
    //     cyberComponent.OnStateChange -= HandleCyberStateChange;
    // }
    public void HandleNodeChange() {
        if (node.compromised) {
            Open();
        }
    }
    // public void HandleCyberStateChange(CyberComponent component) {
    //     if (component.compromised) {
    //         Open();
    //     }
    // }
    public void Open() {
        Toolbox.RandomizeOneShot(audioSource, openSound);
        // GameManager.I.SetCyberNodeCompromised(cyberComponent, false);
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
        // throw new System.NotImplementedException();
        return ItemUseResult.Empty() with { waveArm = true };
    }
}
