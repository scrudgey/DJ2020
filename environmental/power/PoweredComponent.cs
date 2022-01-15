using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredComponent : MonoBehaviour {
    public string idn;
    private bool _power;
    public bool power {
        get { return _power; }
        set {
            _power = value;
            if (_power) {
                OnPowerOn();
            } else { OnPowerOff(); }
        }
    }
    public PoweredComponent[] edges;

    virtual protected void OnPowerOn() { }
    virtual protected void OnPowerOff() { }


#if UNITY_EDITOR
    private void OnDrawGizmos() {
        // string customName = "Relic\\" + relicType.ToString() + ".png";
        // Gizmos.DrawIcon(transform.position, customName, true);
        foreach (PoweredComponent other in edges) {
            Gizmos.DrawLine(transform.position, other.transform.position);
        }
    }
#endif
}
