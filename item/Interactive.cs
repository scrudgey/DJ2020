using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactive : MonoBehaviour {
    public int priority;
    public string calloutText;
    public string actionPrompt;
    public virtual string ResponseString() {
        return $"did {actionPrompt}";
    }
    public abstract void DoAction(Interactor interactor);
}
