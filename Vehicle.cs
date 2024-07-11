using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class Vehicle : MonoBehaviour {
    public Transform body;
    public Transform[] wheels;
    public Transform[] passengerSlots;
    public Transform[] dismountPoints;
    public Transform[] lookPoints;
    [Header("audio")]
    AudioSource audioSource;
    public AudioClip soundEngineStart;
    public AudioClip soundEngineIdle;
    public AudioClip soundEngineStop;
    public AudioClip soundDriveStart;
    public AudioClip soundDriveLoop;
    public AudioClip soundDriveStop;
    public AudioClip[] doorOpenSound;
    public AudioClip[] doorShutSound;
    enum AudioState { off, idle, drive }
    AudioState audioState;
    Coroutine audioCoroutine;
    Vector3 bodyInitialLocalPosition;
    CharacterController[] passengers;

    void Awake() {
        bodyInitialLocalPosition = body.localPosition;
        audioSource = Toolbox.SetUpAudioSource(gameObject);
        passengers = new CharacterController[passengerSlots.Length];
    }

    void Start() {
        ChangeAudioState(AudioState.idle);
    }

    void Update() {
        if (audioState != AudioState.off) {
            Vector3 vibration = 0.01f * Random.insideUnitSphere;
            vibration.x = 0;
            vibration.z = 0;
            body.localPosition = vibration + bodyInitialLocalPosition;
        }
    }

    public IEnumerator DriveToPoint(string idn) {
        Vector3 target = CutsceneManager.I.worldLocations[idn].transform.position;
        ChangeAudioState(AudioState.drive);
        float speed = 7f;
        Transform myTransform = transform;
        float wheelsize = 0.1f;
        Vector3 displacement = target - transform.position;
        while (displacement.magnitude > 0.01f) {
            float distance = speed * Time.unscaledDeltaTime;
            distance = Mathf.Min(distance, displacement.magnitude);

            myTransform.position += displacement.normalized * distance;
            if (displacement.magnitude > 1f) {
                Quaternion targetRotation = Quaternion.LookRotation(displacement, Vector3.up);
                myTransform.rotation = Quaternion.Lerp(myTransform.rotation, targetRotation, 5f * Time.unscaledDeltaTime);
            }
            displacement = target - transform.position;
            foreach (Transform wheel in wheels) {
                Vector3 euler = wheel.rotation.eulerAngles;
                euler.z -= (speed * Time.unscaledDeltaTime) / wheelsize * Mathf.Rad2Deg;
                wheel.rotation = Quaternion.Euler(euler.x, euler.y, euler.z);
            }
            yield return null;
        }
        ChangeAudioState(AudioState.idle);
    }

    /*
        *  *            
     *        *    
    *    .     *    
    *   r|\    *  
     *   |θ\  *      
        *  *
          d

        d = r θ
        d = dv/dt * dt

        θ = dv/dt * dt / r
    */

    public void LoadCharacter(CharacterController character) {
        for (int i = 0; i < passengerSlots.Length; i++) {
            if (passengers[i] == null) {
                LoadCharacterIntoSlot(i, character);
                Toolbox.AudioSpeaker(passengerSlots[i].position, doorOpenSound);
                return;
            }
        }
        int numberPassengers = passengers.Select(passenger => passenger == null ? 0 : 1).Sum();
        if (numberPassengers == 1 && audioState == AudioState.off) {
            ChangeAudioState(AudioState.idle);
        }
    }

    public CharacterController Unload() {
        CharacterController output = null;
        for (int i = 0; i < passengerSlots.Length; i++) {
            if (passengers[i] != null) {
                output = UnloadCharacterFromSlot(i);
                Toolbox.AudioSpeaker(passengerSlots[i].position, doorShutSound);
                break;
            }
        }
        int numberPassengers = passengers.Select(passenger => passenger == null ? 0 : 1).Sum();
        if (numberPassengers == 0 && audioState != AudioState.off) {
            ChangeAudioState(AudioState.off);
        }
        return output;
    }

    void LoadCharacterIntoSlot(int index, CharacterController character) {

        Transform slot = passengerSlots[index];
        passengers[index] = character;
        SphereRobotAI ai = character.GetComponent<SphereRobotAI>();
        ai.enabled = false;
        character.transform.SetParent(slot, false);
        character.transform.localPosition = Vector3.zero;
        Vector3 direction = lookPoints[index].position - slot.transform.position;
        direction.y = 0;
        character.direction = direction;
        character.lookAtDirection = direction;
        character.Motor.enabled = false;
    }

    CharacterController UnloadCharacterFromSlot(int index) {
        Transform slot = passengerSlots[index];
        Transform dismountPoint = dismountPoints[index];
        CharacterController character = passengers[index];

        passengers[index] = null;

        SphereRobotAI ai = character.GetComponent<SphereRobotAI>();
        ai.enabled = true;
        character.transform.SetParent(null, true);
        character.transform.position = dismountPoint.transform.position;

        character.Motor.enabled = true;
        character.Motor.SetPosition(dismountPoint.transform.position);
        return character;
    }


    void ChangeAudioState(AudioState newState) {
        OnStateEnter(audioState, newState);
        audioState = newState;
        if (newState == AudioState.off) {
            body.localPosition = bodyInitialLocalPosition;
        }
    }

    void OnStateEnter(AudioState oldState, AudioState newState) {
        if (audioCoroutine != null) StopCoroutine(audioCoroutine);
        switch (oldState, newState) {
            case (AudioState.off, AudioState.off):
                break;
            case (AudioState.off, AudioState.idle):
                audioCoroutine = StartCoroutine(ChainAudio((soundEngineStart, false), (soundEngineIdle, true)));
                break;
            case (AudioState.off, AudioState.drive):
                audioCoroutine = StartCoroutine(ChainAudio(
                    (soundEngineStart, false),
                    (soundDriveStart, false),
                    (soundDriveLoop, true)));
                break;

            case (AudioState.idle, AudioState.off):
                audioCoroutine = StartCoroutine(ChainAudio((soundEngineStop, false)));
                break;
            case (AudioState.idle, AudioState.idle):
                break;
            case (AudioState.idle, AudioState.drive):
                audioCoroutine = StartCoroutine(ChainAudio((soundDriveStart, false), (soundDriveLoop, true)));
                break;

            case (AudioState.drive, AudioState.off):
                audioCoroutine = StartCoroutine(ChainAudio((soundDriveStop, false)));
                break;
            case (AudioState.drive, AudioState.idle):
                audioCoroutine = StartCoroutine(ChainAudio((soundDriveStop, false), (soundEngineIdle, true)));
                break;
            case (AudioState.drive, AudioState.drive):
                break;
        }
    }

    IEnumerator ChainAudio(params (AudioClip, bool)[] audioClips) {
        IEnumerator[] plays = audioClips.Select(element =>
        Toolbox.ChainCoroutines(
            Toolbox.CoroutineFunc(() => {
                bool doLoop = element.Item2;
                AudioClip clip = element.Item1;
                if (doLoop) {
                    audioSource.clip = clip;
                    audioSource.loop = true;
                    audioSource.pitch = 1;
                    audioSource.Play();
                } else {
                    audioSource.loop = false;
                    audioSource.Stop();
                    audioSource.PlayOneShot(clip);
                }
            }),
             new WaitForSecondsRealtime(element.Item1.length)
        )).ToArray();

        return Toolbox.ChainCoroutines(plays);
    }
}
