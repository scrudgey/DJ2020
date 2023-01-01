using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MapView : MonoBehaviour {
    public RenderTexture renderTexture;
    public Image mapImage;
    Texture2D texture;
    Rect rect;
    void Start() {
        texture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
        rect = Rect.MinMaxRect(0, 0, 512, 512);
        InvokeRepeating("UpdateMap", 0f, 1f);
    }
    public Sprite CaptureScreen() {
        RenderTexture currentRenderTexture = RenderTexture.active;
        RenderTexture.active = renderTexture;
        texture.ReadPixels(rect, 0, 0);
        texture.Apply();
        RenderTexture.active = currentRenderTexture;

        Sprite sprite = Sprite.Create(texture, rect, Vector2.zero);
        return sprite;
    }
    // Update is called once per frame
    void UpdateMap() {
        mapImage.sprite = CaptureScreen();
    }
}
