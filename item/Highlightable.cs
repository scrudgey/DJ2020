using cakeslice;
using UnityEngine;

public class Highlightable : MonoBehaviour {
    public string calloutText;
    public int priority;
    protected Outline outline;
    public virtual void Start() {
        outline = Toolbox.GetOrCreateComponent<Outline>(gameObject, inChildren: true);
        outline.color = 1;
        DisableOutline();
    }
    public void EnableOutline() {
        outline.enabled = true;
    }
    public void DisableOutline() {
        outline.enabled = false;
    }
}