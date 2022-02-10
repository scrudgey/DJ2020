using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum PowerNodeIcon { normal, power, mains }
public class PoweredComponent : GraphNodeComponent<PoweredComponent> {
    private bool _power;
    public bool power {
        get { return _power; }
        set {
            _power = value;
            OnStateChange?.Invoke(this);
        }
    }
}
