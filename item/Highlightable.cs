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
        StartCoroutine(WaitToInitialize());
    }
    IEnumerator WaitToInitialize() {
        WaitForEndOfFrame waiter = new WaitForEndOfFrame();
        while (GameManager.I.isLoadingLevel) {
            yield return waiter;
        }
        this.outline = Toolbox.GetOrCreateComponent<Outline>(gameObject, inChildren: true);
        this.outline.color = 1;
        DisableOutline();
    }
    public void EnableOutline() {
        if (outline != null)
            outline.enabled = true;
    }
    public void DisableOutline() {
        if (outline != null)
            outline.enabled = false;
    }
}