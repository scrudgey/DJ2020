using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurglarSelectorButton : MonoBehaviour {
    public RectTransform myRect;
    public LoHi bounds = new LoHi(-796, -750);
    bool mouseIsOver;
    public void OnMouseOver() {
        mouseIsOver = true;
    }
    public void OnMouseExit() {
        mouseIsOver = false;
    }
    public void ResetPosition() {
        mouseIsOver = false;
        myRect.anchoredPosition = new Vector3(myRect.anchoredPosition.x, bounds.low);
    }
    void Update() {
        if (mouseIsOver && myRect.anchoredPosition.y < bounds.high) {
            OffsetYPosition(Time.unscaledDeltaTime * 200f);
        } else if (!mouseIsOver && myRect.anchoredPosition.y > bounds.low) {
            OffsetYPosition(-200f * Time.unscaledDeltaTime);
        }
    }

    void OffsetYPosition(float offset) {
        float y = myRect.anchoredPosition.y + offset;
        Vector2 newPosition = new Vector3(myRect.anchoredPosition.x, y);
        myRect.anchoredPosition = newPosition;
    }
}
