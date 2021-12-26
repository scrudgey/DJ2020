using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerFlicker : MonoBehaviour {
    public PoweredDevice power;
    private float timer;
    void Update() {
        timer += Time.deltaTime;
        if (timer > 1f) {
            timer = 0f;
            power.power = !power.power;
        }
    }
}
