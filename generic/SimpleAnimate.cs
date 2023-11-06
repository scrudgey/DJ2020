using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SimpleAnimate : MonoBehaviour {
    public Image image;
    public Sprite[] sprites;
    public float interval;
    float timer;
    int index;
    void Update() {
        timer += Time.unscaledDeltaTime;
        if (timer > interval) {
            timer -= interval;
            index += 1;
            if (index > sprites.Length - 1) {
                index = 0;
            }
            image.sprite = sprites[index];
        }
    }
}
