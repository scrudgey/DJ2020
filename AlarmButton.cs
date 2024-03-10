using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmButton : Interactive, INodeBinder<AlarmNode> {
    public AlarmNode node { get; set; }
    public AudioSource audioSource;
    public AudioClip[] useButtonSounds;
    bool alarmState;
    public override void Start() {
        base.Start();
        audioSource = Toolbox.SetUpAudioSource(gameObject);
    }
    public override ItemUseResult DoAction(Interactor interactor) {
        PressButton();
        return ItemUseResult.Empty() with {
            waveArm = true,
        };
    }
    public void PressButton() {
        GameManager.I.SetAlarmNodeTriggered(node, true);
        Toolbox.RandomizeOneShot(audioSource, useButtonSounds);
    }
    public void HandleNodeChange() {
        alarmState = node.alarmTriggered;
    }

    public bool IsAlarmActive() {
        return alarmState;
    }
}
