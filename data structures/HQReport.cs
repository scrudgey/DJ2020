using UnityEngine;
public class HQReport {
    public enum AlarmChange { noChange, raiseAlarm, cancelAlarm }
    public GameObject reporter;
    public AlarmChange desiredAlarmState;
    public Vector3 locationOfLastDisturbance;
    public float timer;
    public float timeOfLastContact;
    public float lifetime;
    public string speechText;
    public SuspicionRecord suspicionRecord;
}