using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereRobotAnimator : DirectionalBillboard, IBinder<SphereRobotController> {
    public SphereRobotController target { get; set; }
    public float floatMeanHeight = 1f;
    public float floatFrequency = 1f;
    public float floatAmplitude = 0.05f;
    public float floatStep = 0.01f;
    public Octet<Sprite> sphereSprites;
    private float timer;
    void Start() {
        target = GetComponentInParent<SphereRobotController>();
        // Debug.Log(target);
        ((IBinder<SphereRobotController>)this).Bind(target.gameObject);
    }
    public override void Update() {
        base.Update();
        timer += Time.deltaTime;
        float y = FloatAmount(timer);
        transform.localPosition = new Vector3(0f, y, 0f);
    }

    float FloatAmount(float time) {
        float amount = floatMeanHeight + floatAmplitude * Mathf.Sin(time * floatFrequency);
        float discreteAmount = floatStep * (int)(amount / floatStep);
        return discreteAmount;
    }

    public void HandleValueChanged(SphereRobotController controller) {
        direction = controller.direction;
        // Debug.Log(direction);
    }
}
