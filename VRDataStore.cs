using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class VRDataStore : MonoBehaviour {
    public AudioSource audioSource;
    public AudioClip openSound;
    public CyberComponent cyberComponent;
    public Action<VRDataStore> OnDataStoreOpened;
    public SpriteRenderer calloutSprite;
    public ParticleSystem particles;
    public void Start() {
        audioSource = Toolbox.SetUpAudioSource(gameObject);
        cyberComponent.OnStateChange += HandleCyberStateChange;
        DeactivateCallout();
    }
    public void HandleCyberStateChange(CyberComponent component) {
        if (component.compromised) {
            Open();
        }
    }
    public void Open() {
        Toolbox.RandomizeOneShot(audioSource, openSound);
        GameManager.I.SetCyberNodeState(cyberComponent, false);
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
}
