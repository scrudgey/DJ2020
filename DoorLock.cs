using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[System.Serializable]
public class DoorLock : MonoBehaviour {
    public AudioSource audioSource;
    public KeyType lockType;
    public KeyClass keyClass;
    public bool locked;
    [HideInInspector]
    public int lockId;
    public AudioClip[] unlockSounds;
    public AudioClip[] failedUnlockSounds;
    public Transform[] rotationElements;

    public List<KeyData> attemptedKeys = new List<KeyData>();
    public bool isDecoded;

    public bool TryKeyUnlock(KeyData keyData) {
        bool keyTypeMatches = keyData.type == lockType;

        if (keyTypeMatches && keyData.idn == lockId) {
            this.locked = false;
            Toolbox.RandomizeOneShot(audioSource, unlockSounds);
            return true;
        } else {
            Toolbox.RandomizeOneShot(audioSource, failedUnlockSounds);
        }
        return false;
    }
    public void ForceUnlock() {
        this.locked = false;
        Toolbox.RandomizeOneShot(audioSource, unlockSounds);
    }
    public void Lock() {
        this.locked = true;
        Toolbox.RandomizeOneShot(audioSource, unlockSounds);
    }
    public void PickLock() {
        if (lockType == KeyType.physical) {
            this.locked = false;
        }
    }


    // #if UNITY_EDITOR
    //     void OnDrawGizmos() {
    //         Handles.Label(transform.position, $"Locked: {locked}\nKeyId: {lockId}");
    //     }
    // #endif
}