using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public abstract class Interactive : Highlightable {
    public bool dontRequireRaycast;
    [HideInInspector]
    public string actionPrompt;
    [HideInInspector]
    public Interactor interactor;

    public Action OnUsed;
    public bool interactible = true;
    // public List<DoorLock> doorLocks;

    public virtual string ResponseString() {
        return $"did {actionPrompt}";
    }
    public ItemUseResult DoActionAndUpdateState(Interactor interactor) {
        OnUsed?.Invoke();
        CutsceneManager.I.HandleTrigger($"interact_{calloutText}");
        return DoAction(interactor);
    }
    public abstract ItemUseResult DoAction(Interactor interactor);
    public virtual bool AllowInteraction() {
        return true;
    }
    public static T TopTarget<T>(IEnumerable<T> interactives) where T : InteractorTargetData {
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

    public static T ClosestTarget<T>(HashSet<T> interactives) where T : InteractorTargetData {
        if (interactives.Count == 0 || interactives.Where(t => t.target.interactible).Count() == 0) return null;
        return interactives
            .Where(t => t.target.interactible)
            .OrderBy(t => Vector3.Distance(t.target.transform.position, GameManager.I.playerPosition))
            .First();
    }
    void OnDestroy() {
        // if (interactor != null) {
        //     interactor.RemoveInteractive(this);
        // }
    }
}
