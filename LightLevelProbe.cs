using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightLevelProbe : MonoBehaviour, IBindable<LightLevelProbe> {
    private float lightLevel;
    public Action<LightLevelProbe> OnValueChanged { get; set; }
    public CharacterController controller;
    public RenderTexture[] lightTextures;
    void Update() {
        lightLevel = 0;
        foreach (RenderTexture texture in lightTextures) {
            float faceLevel = TextureToLightLevel(texture);
            lightLevel = Math.Max(faceLevel, lightLevel);
        }
        // lightLevel = Toolbox.DiscreteLightLevel(level, controller.isCrouching, controller.isMoving());
        if (OnValueChanged != null) OnValueChanged(this);
    }

    public int GetDiscreteLightLevel() {
        return Toolbox.DiscreteLightLevel(lightLevel, controller.isCrouching, controller.isMoving());
    }

    float TextureToLightLevel(RenderTexture texture) {
        RenderTexture tempTexture = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        Graphics.Blit(texture, tempTexture);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tempTexture;

        Texture2D temp2DTexture = new Texture2D(texture.width, texture.height);
        temp2DTexture.ReadPixels(new Rect(0, 0, tempTexture.width, tempTexture.height), 0, 0);
        temp2DTexture.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tempTexture);

        Color32[] colors = temp2DTexture.GetPixels32();
        Destroy(temp2DTexture);

        float level = 0;

        for (int i = 0; i < colors.Length; i++) {
            float pixelBrightness = (0.2126f * colors[i].r) + (0.7152f * colors[i].g) + (0.0722f * colors[i].b);
            level = Math.Max(level, pixelBrightness);
        }
        level = level / (2.55f);
        return level;
    }
    // void Start() {
    //     t = Terrain.activeTerrain;
    // }
    // public void GetTerrainTexture() {
    //     Vector2 position = ConvertPosition(transform.position);
    //     float[] textureValues = CheckTexture(position);
    // }
    // Vector2 ConvertPosition(Vector3 playerPosition) {
    //     Vector3 terrainPosition = playerPosition - t.transform.position;
    //     Vector3 mapPosition = new Vector3
    //     (terrainPosition.x / t.terrainData.size.x, 0,
    //     terrainPosition.z / t.terrainData.size.z);
    //     float xCoord = mapPosition.x * t.terrainData.alphamapWidth;
    //     float zCoord = mapPosition.z * t.terrainData.alphamapHeight;
    //     float posX = xCoord;
    //     float posZ = zCoord;
    //     return new Vector2(posX, posZ);
    // }
    // float[] CheckTexture(Vector2 position) {
    //     float[] textureValues = new float[4];

    //     float[,,] aMap = t.terrainData.GetAlphamaps((int)position.x, (int)position.y, 1, 1);
    //     // TODO: flexible number of textures
    //     textureValues[0] = aMap[0, 0, 0];
    //     textureValues[1] = aMap[0, 0, 1];
    //     // textureValues[2] = aMap[0, 0, 2];
    //     // textureValues[3] = aMap[0, 0, 3];

    //     return textureValues;
    // }
}
