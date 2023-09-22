using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class CyberDataStore : MonoBehaviour {
    public AudioSource audioSource;

    public AudioClip openSound;
    public CyberComponent cyberComponent;
    public ParticleSystem particles;

    public PayData payData;
    public DataFileIndicator dataFileIndicator;
    bool opened;

    public void Start() {
        audioSource = Toolbox.SetUpAudioSource(gameObject);
        cyberComponent.OnStateChange += HandleCyberStateChange;
        RefreshState();
    }
    public void RefreshState() {
        if (dataFileIndicator != null) {
            dataFileIndicator.Refresh(payData);
        }
    }
    public void HandleCyberStateChange(CyberComponent component) {
        // Debug.Log($"datastore state changed: {component} {component.compromised} {component.idn}");
        if (component.compromised) {
            Open();
            // datafileIndicator.SetActive(false);
            dataFileIndicator.SetIconVisibility(false);
        }
    }
    public void Open() {
        if (opened) return;
        PlayParticles();
        Toolbox.RandomizeOneShot(audioSource, openSound);
        Debug.Log($"stealing paydata: {payData.filename}");
        GameManager.I.AddPayDatas(payData);
        opened = true;
    }
    public void PlayParticles() {
        particles.Play();
    }

    void OnDestroy() {
        cyberComponent.OnStateChange -= HandleCyberStateChange;
        // check if we invalidate an objective
        if (GameManager.I == null || GameManager.I.gameData.levelState == null || GameManager.I.gameData.levelState.template == null) return;
        foreach (Objective objective in GameManager.I.gameData.levelState.template.objectives) {
            if (objective is ObjectiveData) {
                ObjectiveData objectiveData = (ObjectiveData)objective;
                if (payData == objectiveData.targetPaydata) {
                    GameManager.I.FailObjective(objective);
                }
            }
        }
    }
}
