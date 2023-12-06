using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmButton : Interactive {
    public AlarmComponent alarmComponent;
    public AudioSource audioSource;
    public AudioClip[] useButtonSounds;

    bool alarmState;
    public override void Start() {
        base.Start();
        audioSource = Toolbox.SetUpAudioSource(gameObject);
        alarmComponent.OnStateChange += OnAlarmChange;
    }
    public override ItemUseResult DoAction(Interactor interactor) {
        PressButton();
        return ItemUseResult.Empty() with {
            waveArm = true,
        };
    }
    public void PressButton() {
        GameManager.I.SetAlarmNodeTriggered(alarmComponent, true);
        Toolbox.RandomizeOneShot(audioSource, useButtonSounds);
    }
    public void OnAlarmChange(AlarmComponent node) {
        // Debug.Log($"alarm component {this} on power change powered: {node.power}");
        // powerPowered = node.power;
        // ApplyCyberPowerState();
        alarmState = node.alarmTriggered;
    }

    public bool IsAlarmActive() {
        return alarmState;
    }
}
