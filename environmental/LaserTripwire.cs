using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class LaserTripwire : AlarmComponent {
    [System.Serializable]
    public class LaserData {
        public LaserBeam laser;
        public bool enabled;
    }
    public LaserData[] laserData;
    public AudioClip[] spottedSound;
    float cooldown;
    AudioSource audioSource;

    public void Start() {
        audioSource = Toolbox.SetUpAudioSource(gameObject);
        foreach (LaserData data in laserData) {
            data.laser.tripWire = this;
            data.laser.gameObject.SetActive(data.enabled);
        }
    }

    public void LaserTripCallback() {
        if (cooldown > 0)
            return;
        // GameManager.I.ActivateAlarm();
        AlarmNode node = GameManager.I.GetAlarmNode(idn);
        GameManager.I.SetAlarmNodeState(node, true);
        cooldown = 5f;
        Toolbox.RandomizeOneShot(audioSource, spottedSound);
    }

    void Update() {
        if (cooldown > 0f) {
            cooldown -= Time.deltaTime;
        }
    }
}
