using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CashRegister : Interactive {
    public bool opened;
    public AudioSource audioSource;
    public AudioClip openSound;
    public CyberComponent cyberComponent;
    public GameObject credstickPrefab;
    public override void Start() {
        base.Start();
        audioSource = Toolbox.SetUpAudioSource(gameObject);
        cyberComponent.OnStateChange += HandleCyberStateChange;
    }
    void OnDestroy() {
        cyberComponent.OnStateChange -= HandleCyberStateChange;
    }
    public override void DoAction(Interactor interactor) {
        if (!opened) {
            Open();
        }
    }
    public void Open() {
        opened = true;
        Toolbox.RandomizeOneShot(audioSource, openSound);
        GameObject.Instantiate(credstickPrefab, transform.position + new Vector3(0f, 1f, 0f), Quaternion.identity);
    }

    public void HandleCyberStateChange(CyberComponent component) {
        if (component.compromised) {
            Open();
        }
    }
}
