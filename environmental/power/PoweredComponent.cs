using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PoweredDevice))]
public abstract class PoweredComponent : MonoBehaviour {
    public PoweredDevice power;
    void Awake() {
        power = GetComponent<PoweredDevice>();
    }
    void OnEnable() {
        power.PowerOn += OnPowerOn;
        power.PowerOff += OnPowerOff;
    }
    void OnDisable() {
        power.PowerOn -= OnPowerOn;
        power.PowerOff -= OnPowerOff;
    }

    abstract protected void OnPowerOn();
    abstract protected void OnPowerOff();
}
