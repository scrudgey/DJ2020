using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmButton : Interactive {
    public AlarmComponent alarmComponent;
    public AudioSource audioSource;
    public AudioClip[] useButtonSounds;
    public override void Start() {
        base.Start();
        audioSource = Toolbox.SetUpAudioSource(gameObject);
    }

    public override ItemUseResult DoAction(Interactor interactor) {
        GameManager.I.SetAlarmNodeState(alarmComponent, true);
        Toolbox.RandomizeOneShot(audioSource, useButtonSounds);
        return ItemUseResult.Empty() with {
            waveArm = true,
        };
    }
}
