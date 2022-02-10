using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CyberComponent : GraphNodeComponent<CyberComponent> {
    private bool _compromised;
    public bool compromised {
        get { return _compromised; }
        set {
            _compromised = value;
            OnStateChange?.Invoke(this);
        }
    }
}
