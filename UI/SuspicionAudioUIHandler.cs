using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SuspicionAudioUIHandler : MonoBehaviour {
    public static void OnNoise(NoiseData noise) {
        integratedVolume += noise.volume;
    }
    public static float integratedVolume;
    public static readonly float VOLUME_DECAY_CONSTANT = 2f;
    public SuspicionUIHandler parent;
    public Sprite silentImage;
    public Sprite mediumImage;
    public Sprite loudImage;
    public Image image;
    public Suspiciousness suspiciousness;
    public void Bind(SuspicionUIHandler parent) {
        this.parent = parent;
    }
    public void Update() {
        if (integratedVolume > 0) {
            integratedVolume -= integratedVolume * Time.deltaTime * VOLUME_DECAY_CONSTANT;
        } else { integratedVolume = 0; }
        if (integratedVolume <= 1) {
            image.sprite = silentImage;
        } else if (integratedVolume <= 2.5) {
            image.sprite = mediumImage;
        } else {
            image.sprite = loudImage;
        }
        image.color = parent.normalColor;
        parent.OnValueChanged();
    }
}
