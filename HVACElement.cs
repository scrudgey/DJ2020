using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HVACElement : MonoBehaviour {
    public Transform crawlpoint;
    public bool dismountOnEnter;
    public Rigidbody grate;

    public AudioSource audioSource;
    public AudioClip[] impactGrateSound;
    public AudioClip[] ejectGrateSound;
    void Awake() {
        if (grate != null) {
            grate.transform.SetParent(null);
            grate.isKinematic = true;
        }
    }
    public bool DismountOnEnter() {
        return dismountOnEnter;
    }

    public void EjectGrating(CharacterController controller) {
        if (grate == null) {
            controller.TransitionToState(CharacterState.normal);
        } else {
            GameManager.I.ShowGrateKickCutscene(this, controller);
            grate = null;
        }
    }

    public void PlayImactSound() {
        Toolbox.RandomizeOneShot(audioSource, impactGrateSound);
    }
    public void PlayEjectSound() {
        Toolbox.RandomizeOneShot(audioSource, ejectGrateSound);
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(crawlpoint.position + Vector3.up / 2f, 0.1f);
    }
#endif
}
