using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurglarSelectorButton : MonoBehaviour {
    public RectTransform myRect;
    bool mouseIsOver;
    public void OnMouseOver() {
        mouseIsOver = true;
    }
    public void OnMouseExit() {
        mouseIsOver = false;
    }
    public void ResetPosition() {
        mouseIsOver = false;
        myRect.anchoredPosition = new Vector3(myRect.anchoredPosition.x, -796);
    }
    void Update() {
        if (mouseIsOver && myRect.anchoredPosition.y < -750) {
            OffsetYPosition(Time.unscaledDeltaTime * 200f);
        } else if (!mouseIsOver && myRect.anchoredPosition.y > -796) {
            OffsetYPosition(-200f * Time.unscaledDeltaTime);
        }
    }

    void OffsetYPosition(float offset) {
        float y = myRect.anchoredPosition.y + offset;
        Vector2 newPosition = new Vector3(myRect.anchoredPosition.x, y);
        myRect.anchoredPosition = newPosition;
    }
}
