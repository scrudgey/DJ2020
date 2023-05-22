using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class CyberDataStore : MonoBehaviour {
    public AudioSource audioSource;

    public AudioClip openSound;
    public CyberComponent cyberComponent;
    public ParticleSystem particles;

    public List<PayData> payDatas;

    public GameObject datafileIndicator;
    bool opened;

    public void Start() {
        audioSource = Toolbox.SetUpAudioSource(gameObject);
        cyberComponent.OnStateChange += HandleCyberStateChange;
    }
    public void HandleCyberStateChange(CyberComponent component) {
        // Debug.Log($"datastore state changed: {component} {component.compromised} {component.idn}");
        if (component.compromised) {
            Open();
            datafileIndicator.SetActive(false);
        }
    }
    public void Open() {
        if (opened) return;
        PlayParticles();
        Toolbox.RandomizeOneShot(audioSource, openSound);
        foreach (PayData payData in payDatas) {
            Debug.Log($"stealing paydata: {payData.filename}");
        }
        GameManager.I.AddPayDatas(payDatas);
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
                foreach (PayData targetData in objectiveData.targetPaydata) {
                    if (payDatas.Contains(targetData)) {
                        GameManager.I.FailObjective(objective);
                    }
                }
            }
        }
    }
}
