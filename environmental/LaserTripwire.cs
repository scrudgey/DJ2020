using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class LaserTripwire : MonoBehaviour, INodeBinder<AlarmNode>, INodeBinder<CyberNode> {
    AlarmNode INodeBinder<AlarmNode>.node { get; set; }
    CyberNode INodeBinder<CyberNode>.node { get; set; }

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

    bool alarmNodeEnabled;
    bool cyberNodeEnabled;
    bool overallEnabled;

    public void Start() {
        alarmNodeEnabled = true;
        cyberNodeEnabled = true;
        UpdateEnabled();
        audioSource = Toolbox.SetUpAudioSource(gameObject);
        foreach (LaserData data in laserData) {
            data.laser.tripWire = this;
            data.laser.gameObject.SetActive(data.enabled);
        }
    }
    public void LaserTripCallback() {
        if (cooldown > 0 || !overallEnabled)
            return;
        GameManager.I.SetAlarmNodeTriggered(((INodeBinder<AlarmNode>)this).node, true);
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

    public void DisableSource() {
        foreach (LaserData data in laserData) {
            data.laser.gameObject.SetActive(false);
            data.emissionSprite.enabled = false;
        }
        if (audioSource != null)
            audioSource.Stop();
        buzzSoundSource.Stop();
    }
    public void EnableSource() {
        foreach (LaserData data in laserData) {
            data.laser.gameObject.SetActive(data.enabled);
        }
        if (audioSource != null)
            audioSource.Play();
    }


    void INodeBinder<AlarmNode>.HandleNodeChange() {
        alarmNodeEnabled = ((INodeBinder<AlarmNode>)this).node.getEnabled();
        UpdateEnabled();
    }
    void INodeBinder<CyberNode>.HandleNodeChange() {
        cyberNodeEnabled = ((INodeBinder<CyberNode>)this).node.utilityActive;
        Debug.Log(cyberNodeEnabled);
        UpdateEnabled();
    }

    void UpdateEnabled() {
        overallEnabled = alarmNodeEnabled && cyberNodeEnabled;
        if (!overallEnabled) {
            audioSource.Stop();
        }
    }
}
