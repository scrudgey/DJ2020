using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CashRegister : Interactive, INodeBinder<CyberNode> {
    public CyberNode node { get; set; }
    public bool opened;
    public AudioSource audioSource;
    public AudioClip openSound;
    public GameObject credstickPrefab;
    public Action<List<Interactive>> OnRegisterOpened;
    public override void Start() {
        base.Start();
        audioSource = Toolbox.SetUpAudioSource(gameObject);
    }
    public void HandleNodeChange() {
        if (!node.utilityActive) Open();
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
        List<Interactive> credsticks = new List<Interactive>();

        for (int i = 0; i < 3; i++) {
            GameObject credstickObject = GameObject.Instantiate(credstickPrefab, transform.position + new Vector3(0f, 1f, 0f), Quaternion.identity);
            Rigidbody rigidbody = credstickObject.GetComponent<Rigidbody>();

            Vector3 velocity = UnityEngine.Random.Range(1f, 5f) * UnityEngine.Random.insideUnitSphere;
            velocity.y = Mathf.Abs(velocity.y);

            rigidbody.velocity = velocity;

            Interactive credstick = credstickObject.GetComponentInChildren<Interactive>();
            credsticks.Add(credstick);
        }
        OnRegisterOpened?.Invoke(credsticks);
        enabled = false;
        interactible = false;
    }
}
