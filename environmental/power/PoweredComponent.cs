using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PoweredComponent : GraphNodeComponent<PoweredComponent, PowerNode> {
    private bool _power;
    public bool power {
        get { return _power; }
        set {
            _power = value;
            OnStateChange?.Invoke(this);
        }
    }
}
