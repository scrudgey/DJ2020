using System;
using System.Collections;
using System.Collections.Generic;
using cakeslice;
using UnityEngine;
public class Highlightable : MonoBehaviour {
    public string calloutText;
    public int priority;
    protected Outline outline;
    public bool alwaysOutline;

    public virtual void Start() {
        StartCoroutine(Toolbox.WaitForSceneLoadingToFinish(Initialize));
    }
    void Initialize() {
        try {
            this.outline = Toolbox.GetOrCreateComponent<Outline>(gameObject, inChildren: true);
            this.outline.color = 1;
            if (alwaysOutline) {
                EnableOutline();
            } else {
                DisableOutline();
            }
        }
        catch (Exception e) {
            Debug.Log($"[highlightable] ******* trying to initialize {gameObject}: {e}");
        }
    }
    public virtual void EnableOutline() {
        if (outline != null) {
            outline.enabled = true;
        }
    }
    public virtual void DisableOutline() {
        if (alwaysOutline) return;
        if (outline != null)
            outline.enabled = false;
    }
}