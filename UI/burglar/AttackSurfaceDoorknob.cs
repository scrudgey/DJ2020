using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSurfaceDoorknob : AttackSurfaceElement {
    public Door door;
    public AudioSource audioSource;
    public AudioClip[] pickSounds;
    public AudioClip[] manipulateSounds;
    public override void HandleAttack(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleAttack(activeTool, data);
        if (activeTool == BurglarToolType.lockpick) {
            Toolbox.RandomizeOneShot(audioSource, pickSounds);
            door.Unlock();
        } else if (activeTool == BurglarToolType.probe) {
            Toolbox.RandomizeOneShot(audioSource, manipulateSounds);
        } else if (activeTool == BurglarToolType.none) {
            door.ActivateDoorknob(data.burglar.transform);
        }
    }
}
