using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class CyberDataStore : MonoBehaviour, INodeBinder<CyberNode> {
    public CyberNode node { get; set; }
    public AudioSource audioSource;
    public AudioClip openSound;
    public ParticleSystem particles;
    public DataFileIndicator dataFileIndicator;
    bool opened;
    public void Start() {
        audioSource = Toolbox.SetUpAudioSource(gameObject);
        // RefreshState();
    }
    public void HandleNodeChange() {
        if (node.compromised) {
            // Open();
            dataFileIndicator.SetIconVisibility(false);
            RefreshState();
        }
    }
    public void RefreshState() {
        dataFileIndicator?.Refresh(node.payData);
    }
    // public void Open() {
    //     if (opened) return;
    //     PlayParticles();
    //     Toolbox.RandomizeOneShot(audioSource, openSound);
    //     Debug.Log($"stealing paydata: {node.payData.filename}");
    //     GameManager.I.AddPayDatas(node.payData);
    //     opened = true;
    // }
    public void PlayParticles() {
        particles.Play();
    }

}
