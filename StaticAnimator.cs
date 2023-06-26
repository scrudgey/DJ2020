using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticAnimator : MonoBehaviour {
    public RectTransform myRectTransform;
    public LoHi range;
    void Update() {
        Vector2 newPosition = new Vector2(range.GetRandomInsideBound(), range.GetRandomInsideBound());
        myRectTransform.anchoredPosition = newPosition;
        int xScale = Random.Range(0, 2) * 2 - 1;
        int yScale = Random.Range(0, 2) * 2 - 1;
        myRectTransform.localScale = new Vector3(xScale * 2, yScale, 1f);
    }
}
