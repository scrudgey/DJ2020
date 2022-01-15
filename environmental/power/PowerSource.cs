using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSource : PoweredComponent {
    // bool sourceEnabled = true;
    public void DisableSource() {
        GameManager.I.SetPowerSourceState(idn, false);
    }
    public void EnableSource() {
        GameManager.I.SetPowerSourceState(idn, true);
    }

    void OnDisable() {
        DisableSource();
    }
    void OnEnable() {
        EnableSource();
    }
    void OnDestroy() {
        DisableSource();
    }
}
