using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CyberComponent : GraphNodeComponent<CyberComponent, CyberNode> {
    protected bool _compromised;
    public virtual bool compromised {
        get { return _compromised; }
        set {
            _compromised = value;
            OnStateChange?.Invoke(this);
        }
    }
}
