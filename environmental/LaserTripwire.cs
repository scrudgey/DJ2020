using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class LaserTripwire : AlarmComponent {
    [System.Serializable]
    public class LaserData {
        public LaserBeam laser;
        public SpriteRenderer emissionSprite;
        public bool enabled;
    }
    public LaserData[] laserData;
    public AudioClip[] spottedSound;
    float cooldown;
    AudioSource audioSource;
    public AudioSource buzzSoundSource;

    public override void Start() {
        base.Start();
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
        GameManager.I.SetAlarmNodeTriggered(node, true);
        cooldown = 5f;
        Toolbox.RandomizeOneShot(audioSource, spottedSound);
        foreach (LaserData data in laserData) {
            data.laser.ShowLaserTemporarily();
        }
        GameManager.I.SetLocationOfDisturbance(transform.position);
        GameManager.I.DispatchGuard(transform.position);
        GameManager.I.AddSuspicionRecord(SuspicionRecord.trippedSensor("laser tripwire"));
    }
    void Update() {
        if (cooldown > 0f) {
            cooldown -= Time.deltaTime;
        }
    }

    override public void DisableSource() {
        base.DisableSource();
        foreach (LaserData data in laserData) {
            data.laser.gameObject.SetActive(false);
            data.emissionSprite.enabled = false;
        }
        if (audioSource != null)
            audioSource.Stop();
        buzzSoundSource.Stop();
    }
    override public void EnableSource() {
        base.EnableSource();
        foreach (LaserData data in laserData) {
            data.laser.gameObject.SetActive(data.enabled);
        }
        if (audioSource != null)
            audioSource.Play();
    }
}
