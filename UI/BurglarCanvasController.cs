using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BurglarCanvasController : MonoBehaviour {
    BurgleTargetData data;
    public RawImage rawImage;
    public void Initialize(BurgleTargetData data) {
        this.data = data;
        data.target.EnableAttackSurface();
        rawImage.texture = data.target.renderTexture;
        Debug.Log($"Initialize with {data}");
    }
    public void TearDown() {
        if (data != null)
            data.target.DisableAttackSurface();
    }

    public void DoneButtonCallback() {
        GameManager.I.CloseBurglar();
    }
}
