using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereRobotSpeaker : MonoBehaviour {
    public AudioSource audioSource;
    public AudioClip[] idleSpeakSounds;
    public AudioClip[] angrySpeakSounds;
    public SpeechTextController speechTextController;
    float idleSpeakTimer;
    void Start() {
        audioSource = Toolbox.SetUpAudioSource(gameObject);
        idleSpeakTimer = 5f;
    }
    void SetIdleSpeakTimer() {
        idleSpeakTimer = Random.Range(5f, 10f);
    }
    void DoIdleSpeak() {
        // Toolbox.RandomizeOneShot(audioSource, idleSpeakSounds);
        // speechTextController.Say("memcache-88xdd*");
    }
    void Update() {
        if (idleSpeakTimer > 0) {
            idleSpeakTimer -= Time.deltaTime;
            if (idleSpeakTimer < 0) {
                SetIdleSpeakTimer();
                DoIdleSpeak();
                idleSpeakTimer = 8f;
            }
        }
    }

    public void DoAttackSpeak() {
        Toolbox.RandomizeOneShot(audioSource, angrySpeakSounds);
        speechTextController.Say("Enemy sighted");
    }

    public void DoInvestigateSpeak() {
        Toolbox.RandomizeOneShot(audioSource, angrySpeakSounds);
        speechTextController.Say("Stop right there!");
    }
}
