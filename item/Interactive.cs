using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class Interactive : Highlightable {
    public string actionPrompt;
    public Interactor interactor;
    public virtual string ResponseString() {
        return $"did {actionPrompt}";
    }
    public abstract void DoAction(Interactor interactor);
    public static T TopTarget<T>(IEnumerable<T> interactives) where T : HighlightableTargetData {
        T topInteractive = null;
        foreach (T data in interactives) {
            if (topInteractive == null) {
                topInteractive = data;
            } else if (topInteractive.target.priority < data.target.priority) {
                topInteractive = data;
            }
        }
        return topInteractive;
    }
    void OnDestroy() {
        if (interactor != null) {
            interactor.RemoveInteractive(this);
        }
    }
}
