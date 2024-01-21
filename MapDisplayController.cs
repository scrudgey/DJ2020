using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapInput {
    public int floorIncrement;
    public int zoomIncrement;
    public float zoomFloatIncrement;
    public MapDisplay3DGenerator.Mode modeChange;
    public float thetaDelta;
    public Vector2 translationInput;
}

public class MapDisplayController : MonoBehaviour {
    public MapDisplay3DGenerator mapDisplay3DGenerator;
    bool mouseOverMap;
    bool mapEngaged;


    int flootIncrementThisFrame;
    int zoomIncrementThisFrame;
    float zoomFloatIncrementThisFrame;
    MapDisplay3DGenerator.Mode modeChangeThisFrame;
    float thetaDeltaThisFrame;
    Vector2 translationInput;

    void Start() {
        GameManager.OnPlayerInput += UpdateWithInput;
    }
    void OnDestroy() {
        GameManager.OnPlayerInput -= UpdateWithInput;
    }
    public void OnMouseOverMap() {
        mouseOverMap = true;
    }
    public void OnMouseExitMap() {
        mouseOverMap = false;
    }


    // control
    public void UpdateWithInput(PlayerInput playerInput) {
        if (playerInput.rightMouseDown && mouseOverMap) {
            mapEngaged = true;
            thetaDeltaThisFrame = playerInput.mouseDelta.x * Time.unscaledDeltaTime;
        } else if (playerInput.mouseDown && mouseOverMap) {
            mapEngaged = true;
            translationInput = playerInput.mouseDelta * Time.unscaledDeltaTime;
        } else if (!playerInput.mouseDown && !playerInput.rightMouseDown) {
            mapEngaged = false;
        }

        zoomFloatIncrementThisFrame = playerInput.zoomInput.y * Time.unscaledDeltaTime;
    }

    public void FloorIncrementButtonCallback(int increment) {
        flootIncrementThisFrame += increment;
    }
    public void ZoomIncrementButtonCallback(int increment) {
        zoomIncrementThisFrame += increment;
    }
    public void ModeButtonCallback() {
        modeChangeThisFrame = mapDisplay3DGenerator.mode switch {
            MapDisplay3DGenerator.Mode.playerfocus => MapDisplay3DGenerator.Mode.rotate,
            MapDisplay3DGenerator.Mode.rotate => MapDisplay3DGenerator.Mode.playerfocus,
            _ => MapDisplay3DGenerator.Mode.rotate
        };
    }

    void Update() {
        MapInput input = new MapInput() {
            floorIncrement = flootIncrementThisFrame,
            zoomIncrement = zoomIncrementThisFrame,
            zoomFloatIncrement = zoomFloatIncrementThisFrame,
            modeChange = modeChangeThisFrame,
            thetaDelta = thetaDeltaThisFrame,
            translationInput = translationInput
        };
        mapDisplay3DGenerator.UpdateWithInput(input);

        flootIncrementThisFrame = 0;
        zoomIncrementThisFrame = 0;
        zoomFloatIncrementThisFrame = 0;
        modeChangeThisFrame = MapDisplay3DGenerator.Mode.none;
        thetaDeltaThisFrame = 0;
        translationInput = Vector2.zero;
    }
}
