using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpIndicatorView : IBinder<CharacterController> {
    // public CharacterController target { get; set; }
    public LineRenderer lineRenderer;
    public SpriteRenderer indicatorSprite;
    public Transform indicator;
    public CharacterController characterController;

    void Start() {
        // TODO: fix
        // GameManager.OnFocusChanged += Bind;
        Bind(target.gameObject);
    }
    public override void HandleValueChanged(CharacterController controller) {
        AnimationInput input = controller.BuildAnimationInput();
        UpdateView(input);
    }
    public void UpdateView(AnimationInput input) {
        if (input.state == CharacterState.jumpPrep) {
            lineRenderer.enabled = true;
            indicatorSprite.enabled = true;
            SetLineArc();
        } else {
            lineRenderer.enabled = false;
            indicatorSprite.enabled = false;
        }
    }

    public void SetLineArc() {
        List<Vector3> points = new List<Vector3>();

        Vector3 initialVelocity = Toolbox.SuperJumpVelocity(indicator.localPosition, characterController.superJumpSpeed, characterController.Gravity.y);

        float flyTime = Toolbox.SuperJumpTime(indicator.localPosition, characterController.superJumpSpeed, characterController.Gravity.y);

        Vector3 pos(float t) {
            return (initialVelocity * t + (0.5f * characterController.Gravity * (Mathf.Pow(t, 2))));
        }

        float dt = 0.1f;
        points.Add(Vector3.zero);
        for (int i = 0; i < flyTime / dt + 1; i++) {
            points.Add(pos(dt * i));
        }

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }
}
