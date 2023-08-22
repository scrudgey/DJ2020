using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorCallButton : Interactive {
    public int floorNumber;
    public AudioSource audioSource;
    public AudioClip[] buttonSound;
    public ElevatorController elevatorController;
    public override ItemUseResult DoAction(Interactor interactor) {
        Toolbox.RandomizeOneShot(audioSource, buttonSound);
        elevatorController.CallElevator(this);
        return ItemUseResult.Empty() with {
            waveArm = true
        };
    }


}
