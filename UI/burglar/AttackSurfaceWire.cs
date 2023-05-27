using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSurfaceWire : MonoBehaviour {
    public AlarmComponent[] alarmComponents;
    public void DoCut() {
        Debug.Log("attack surface wire is cut");
        foreach (AlarmComponent component in alarmComponents) {
            GameManager.I.SetNodeEnabled<AlarmComponent, AlarmNode>(component, false);
        }
    }
}
