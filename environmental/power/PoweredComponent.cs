using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredComponent : MonoBehaviour {
    // public PowerNode node;
    public string idn;
    public bool power;
    public PoweredComponent[] edges;
    // public PoweredComponentNode node;
    // public string idn;

    // void OnEnable() {
    //     // power.PowerOn += OnPowerOn;
    //     // power.PowerOff += OnPowerOff;
    // }
    // void OnDisable() {
    //     // power.PowerOn -= OnPowerOn;
    //     // power.PowerOff -= OnPowerOff;
    // }

    virtual protected void OnPowerOn() { }
    virtual protected void OnPowerOff() { }
}
