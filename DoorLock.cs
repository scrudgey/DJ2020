using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[System.Serializable]
public class DoorLock : MonoBehaviour {
    public enum LockType { physical, keycard, physicalCode, keycardCode, keypadCode }
    // public Door door;
    public AudioSource audioSource;
    public LockType lockType;
    public bool locked;
    public int lockId;
    public AudioClip[] unlockSounds;
    public AudioClip[] failedUnlockSounds;
    public Transform[] rotationElements;

    public List<KeyData> attemptedKeys = new List<KeyData>();
    public bool isDecoded;

    public bool TryKeyUnlock(KeyData keyData) {
        bool keyTypeMatches = keyData.type switch {
            KeyType.keycard => lockType == LockType.keycard,
            KeyType.physical => lockType == LockType.physical,
            _ => false
        };
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
        if (lockType == LockType.physical) {
            this.locked = false;
        }
    }


    // #if UNITY_EDITOR
    //     void OnDrawGizmos() {
    //         Handles.Label(transform.position, $"Locked: {locked}\nKeyId: {lockId}");
    //     }
    // #endif
}