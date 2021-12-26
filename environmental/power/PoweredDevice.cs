using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PoweredDevice : MonoBehaviour {
    public Action PowerOn;
    public Action PowerOff;
    private bool _power;
    public bool power {
        get { return _power; }
        set {
            _power = value;
            if (_power) {
                PowerOn();
            } else {
                PowerOff();
            }
        }
    }
}
