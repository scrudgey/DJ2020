using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorCallButton : Interactive {
    public int floorNumber;
    public AudioSource audioSource;
    public AudioClip[] buttonSound;
    public ElevatorController elevatorController;
    public SpriteRenderer lightSprite;
    public override void Start() {
        base.Start();
        lightSprite.enabled = false;
    }
    public override ItemUseResult DoAction(Interactor interactor) {
        Toolbox.RandomizeOneShot(audioSource, buttonSound);
        elevatorController.CallElevator(this);
        lightSprite.enabled = true;
        return ItemUseResult.Empty() with {
            waveArm = true
        };
    }

    public void AnswerCallButton() {
        lightSprite.enabled = false;
    }


}
