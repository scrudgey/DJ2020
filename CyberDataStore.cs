using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CyberDataStore : MonoBehaviour {
    public AudioSource audioSource;

    public AudioClip openSound;
    public CyberComponent cyberComponent;
    public ParticleSystem particles;

    public List<PayData> payDatas;

    public void Start() {
        audioSource = Toolbox.SetUpAudioSource(gameObject);
        cyberComponent.OnStateChange += HandleCyberStateChange;
    }
    public void HandleCyberStateChange(CyberComponent component) {
        if (component.compromised) {
            Open();
        }
    }
    public void Open() {
        Toolbox.RandomizeOneShot(audioSource, openSound);
        GameManager.I.SetCyberNodeState(cyberComponent, false);
        foreach (PayData payData in payDatas) {
            Debug.Log($"stealing paydata: {payData.filename}");
        }
        GameManager.I.AddPayDatas(payDatas);
    }
    public void PlayParticles() {
        particles.Play();
    }
}
