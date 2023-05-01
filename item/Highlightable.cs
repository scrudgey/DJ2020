using System;
using System.Collections;
using System.Collections.Generic;
using cakeslice;
using UnityEngine;
public class Highlightable : MonoBehaviour {
    public string calloutText;
    public int priority;
    protected Outline outline;
    public virtual void Start() {
        StartCoroutine(Toolbox.WaitForSceneLoadingToFinish(Initialize));
    }
    void Initialize() {
        try {

            this.outline = Toolbox.GetOrCreateComponent<Outline>(gameObject, inChildren: true);
            this.outline.color = 1;
            DisableOutline();
        }
        catch (Exception e) {
            Debug.Log($"[highlightable] ******* trying to initialize {gameObject}: {e}");
        }
    }
    public void EnableOutline() {
        if (outline != null) {
            outline.enabled = true;
        }
    }
    public void DisableOutline() {
        if (outline != null)
            outline.enabled = false;
    }
}