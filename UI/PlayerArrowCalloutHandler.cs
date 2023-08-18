using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerArrowCalloutHandler : IBinder<CharacterController> {
    public Camera UICamera;
    public Image arrowImage;
    public bool active;
    public RectTransform cursorRect;
    override public void HandleValueChanged(CharacterController target) {
        if (target.state == CharacterState.hvac) {
            Show();
        } else {
            Hide();
        }
    }

    void Show() {
        active = true;
        arrowImage.enabled = true;
    }

    void Hide() {
        active = false;
        arrowImage.enabled = false;
    }

    void Update() {
        if (active) {
            SetPosition();
        }
    }

    public void SetPosition() {
        if (UICamera == null) return;
        Transform root = target.transform;
        Rect bounds = Toolbox.GetTotalRenderBoundingBox(root, UICamera);
        cursorRect.position = UICamera.WorldToScreenPoint(root.position) + new Vector3(0f, bounds.height / 2f, 0f);
    }
}
