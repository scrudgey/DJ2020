using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public abstract class Interactive : Highlightable {
    public bool dontRequireRaycast;
    public string actionPrompt;
    [HideInInspector]
    public Interactor interactor;
    public string interactiveId;

    public Action OnUsed;

    public virtual string ResponseString() {
        return $"did {actionPrompt}";
    }
    public ItemUseResult DoActionAndUpdateState(Interactor interactor) {
        // GameManager.I.gameData.levelState.delta.levelInteractedObjects.Add(interactiveId);
        GameManager.I.CheckObjectives();
        OnUsed?.Invoke();
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
        if (interactives.Count == 0) return null;
        return interactives
            .OrderBy(t => Vector3.Distance(t.target.transform.position, GameManager.I.playerPosition))
            .First();
    }
    void OnDestroy() {
        if (interactor != null) {
            interactor.RemoveInteractive(this);
        }
    }
}
