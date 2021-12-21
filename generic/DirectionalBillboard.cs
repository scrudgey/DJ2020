using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalBillboard : MonoBehaviour {
    public Vector3 direction;
    public SpriteRenderer spriteRenderer;
    public Octet<Sprite[]> skin;
    private CharacterCamera _cam;
    public CharacterCamera cam {
        get {
            if (_cam == null) {
                _cam = Camera.main.GetComponent<CharacterCamera>();
            }
            return _cam;
        }
    }
    public void Update() {
        Vector2 camDir = new Vector2(cam.PlanarDirection.x, cam.PlanarDirection.z);
        Vector2 playerDir = new Vector2(direction.x, direction.z);
        float angle = Vector2.SignedAngle(camDir, playerDir);
        SetFrame(0, Toolbox.DirectionFromAngle(angle));
    }
    public void SetFrame(int frame, Direction direction) {
        spriteRenderer.sprite = skin[direction][frame];
    }
}
