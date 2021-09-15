using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerminateAfterTime : MonoBehaviour {
    public float lifetime;
    private float timer;
    void Update() {
        timer += Time.deltaTime;
        if (timer > lifetime) {
            Toolbox.DestroyIfExists<Rigidbody>(gameObject);
            Toolbox.DestroyIfExists<Collider>(gameObject);
            Toolbox.DestroyIfExists<AudioSource>(gameObject);
            Toolbox.DestroyIfExists<PlaySound>(gameObject);
            Toolbox.DestroyIfExists<FlipScintillator>(gameObject);
            Destroy(this);
        }
    }
}
