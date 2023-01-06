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

    void OnDestroy() {
        // check if we invalidate an objective
        List<string> myFileNames = payDatas.Select(data => data.filename).ToList();
        if (GameManager.I == null || GameManager.I.gameData.levelState == null || GameManager.I.gameData.levelState.template == null) return;
        foreach (Objective objective in GameManager.I.gameData.levelState.template.objectives) {
            if (objective is ObjectiveData) {
                ObjectiveData objectiveData = (ObjectiveData)objective;
                foreach (string targetData in objectiveData.targetFileNames) {
                    if (myFileNames.Contains(targetData)) {
                        GameManager.I.FailObjective(objective);
                    }
                }
            }
        }
    }
}
