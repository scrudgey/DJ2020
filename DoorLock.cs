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
    public Transform[] rotationElements;
    public bool isDecoded;

    public bool TryKeyToggle(LockType keyType, int keyId) {
        if (keyType == lockType && keyId == lockId) {
            this.locked = !this.locked;
            Toolbox.RandomizeOneShot(audioSource, unlockSounds);
            return true;
        }
        return false;
    }
    public bool TryKeyUnlock(LockType keyType, int keyId) {
        if (keyType == lockType && keyId == lockId) {
            this.locked = false;
            Toolbox.RandomizeOneShot(audioSource, unlockSounds);
            return true;
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