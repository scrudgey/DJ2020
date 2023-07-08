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
    public override ItemUseResult DoAction(Interactor interactor) {
        Open();
        SuspicionRecord record = SuspicionRecord.robRegisterSuspicion();
        GameManager.I.AddSuspicionRecord(record);
        return ItemUseResult.Empty() with { waveArm = true };
    }
    public void Open() {
        if (opened)
            return;
        opened = true;
        Toolbox.RandomizeOneShot(audioSource, openSound);
        GameObject credstickObject = GameObject.Instantiate(credstickPrefab, transform.position + new Vector3(0f, 1f, 0f), Quaternion.identity);
        Rigidbody rigidbody = credstickObject.GetComponent<Rigidbody>();

        Vector3 velocity = Random.Range(1f, 5f) * Random.insideUnitSphere;
        velocity.y = Mathf.Abs(velocity.y);

        rigidbody.velocity = velocity;
    }

    public void HandleCyberStateChange(CyberComponent component) {
        if (component.compromised) {
            Open();
        }
    }
}
