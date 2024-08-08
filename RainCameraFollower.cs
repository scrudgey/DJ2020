using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainCameraFollower : IBinder<CharacterCamera> {
    public CharacterCamera characterCamera;
    public void Start() {
        Bind(characterCamera.gameObject);
    }
    public override void HandleValueChanged(CharacterCamera camera) {
        transform.position = camera.lastTargetPosition;
    }
}
