using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSurfaceUIElement : MonoBehaviour {
    BurglarCanvasController controller;
    AttackSurfaceElement element;
    public void Initialize(BurglarCanvasController controller, AttackSurfaceElement element) {
        this.controller = controller;
        this.element = element;
    }
    public void ClickCallback() {
        controller.ClickCallback(element);
    }
    public void ClickDownCallback() {
        controller.ClickDownCallback(element);
    }
    public void MouseOverCallback() {
        controller.MouseOverUIElementCallback(element);
    }
    public void MouseExitCallback() {
        controller.MouseExitUIElementCallback(element);
    }
}
