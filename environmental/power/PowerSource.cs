using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSource : PoweredComponent {
    // bool sourceEnabled = true;
    public void DisableSource() {
        GameManager.I.SetNodeEnabled(idn, false);
    }
    public void EnableSource() {
        GameManager.I.SetNodeEnabled(idn, true);
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
