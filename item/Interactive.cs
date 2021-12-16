using System.Collections;
using System.Collections.Generic;
using cakeslice;
using UnityEngine;
public abstract class Interactive : MonoBehaviour {
    public int priority;
    public string calloutText;
    public string actionPrompt;
    protected Outline outline;
    public virtual string ResponseString() {
        return $"did {actionPrompt}";
    }
    public abstract void DoAction(Interactor interactor);
    public virtual void Start() {
        outline = Toolbox.GetOrCreateComponent<Outline>(gameObject);
        outline.color = 1;
        DisableOutline();
    }
    public void EnableOutline() {
        outline.enabled = true;
    }
    public void DisableOutline() {
        outline.enabled = false;
    }

    public static InteractorTargetData TopTarget(IEnumerable<InteractorTargetData> interactives) {
        // TODO: why is this disabling / enabling outlines?
        InteractorTargetData topInteractive = null;
        foreach (InteractorTargetData data in interactives) {
            if (topInteractive == null) {
                topInteractive = data;
            } else if (topInteractive.target.priority < data.target.priority) {
                topInteractive = data;
            } else {
                // data.target.DisableOutline();
            }
        }
        return topInteractive;
    }
}
