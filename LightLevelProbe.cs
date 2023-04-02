using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
public class LightLevelProbe : MonoBehaviour, IBindable<LightLevelProbe> {
    private float lightLevel;
    float targetLightLevel;
    public Action<LightLevelProbe> OnValueChanged { get; set; }
    public CharacterController controller;
    public RenderTexture[] lightTextures;
    public SpriteRenderer[] spriteRenderers;
    HashSet<Collider> concealment = new HashSet<Collider>();
    public Color targetSpriteColor;
    public Color currentSpriteColor;
    Coroutine coroutine;
    NativeArray<byte> textureData;
    WaitForSeconds waitForSeconds = new WaitForSeconds(0.1f);
    void Start() {
        textureData = new NativeArray<byte>(lightTextures[0].height * lightTextures[0].width * 4, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        coroutine = StartCoroutine(RunJobRepeatedly());
    }
    void OnDestroy() {
        // if (textureData.)
        try {
            textureData.Dispose();
        }
        catch (Exception e) {
            // Debug.Log($"texturedata is already disposed: {e}");
        }
        if (coroutine != null)
            StopCoroutine(coroutine);
    }

    IEnumerator RunJobRepeatedly() {
        while (true) {
            if (concealment.Count > 0) {
                targetLightLevel = 13f;
            } else {
                targetLightLevel = 0;
                yield return AsyncRead(lightTextures[0]);
                yield return AsyncRead(lightTextures[1]);
                yield return AsyncRead(lightTextures[2]);
                yield return AsyncRead(lightTextures[3]);
            }
            UpdateLightLevel();
            // yield return new WaitForEndOfFrame();
            yield return waitForSeconds;
        }
    }
    IEnumerator AsyncRead(RenderTexture renderTexture) {
        var asyncRead = AsyncGPUReadback.RequestIntoNativeArray(ref textureData, renderTexture, 0, request => {
            if (request.hasError) {
                Debug.LogError($"GPU readback error detected: {renderTexture}");
                return;
            }
        });
        yield return new WaitForEndOfFrame();
        while (!asyncRead.done) {
            yield return null;
        }
        var colorArray = new Color32[textureData.Length / 4];
        float level = 0;
        for (var i = 0; i < textureData.Length; i += 4) {
            // var color = new Color32(textureData[i + 0], textureData[i + 1], textureData[i + 2], textureData[i + 3]);
            float pixelBrightness = (0.2126f * textureData[i + 0]) + (0.7152f * textureData[i + 1]) + (0.0722f * textureData[i + 2]);
            level = Math.Max(level, pixelBrightness);
        }
        targetLightLevel = Math.Max(targetLightLevel, level / (2f));
    }
    public void UpdateLightLevel() {
        float lerpfactor = targetLightLevel > lightLevel ? 2f : 0.01f;
        lerpfactor *= 200f;
        lightLevel = Mathf.Lerp(lightLevel, targetLightLevel, lerpfactor);
        int discreteLightLevel = Toolbox.DiscreteLightLevel(lightLevel, controller.isCrouching, controller.isMoving());
        targetSpriteColor = discreteLightLevel switch {
            0 => Color.black,
            1 => new Color(0.25f, 0.25f, 0.25f, 1f),
            2 => new Color(0.85f, 0.85f, 0.85f, 1f),
            _ => Color.white
        };
        // Debug.Log($"updating with target light level: {targetLightLevel} {discreteLightLevel}");
        currentSpriteColor = Color.Lerp(currentSpriteColor, targetSpriteColor, 150f * Time.unscaledDeltaTime);
        currentSpriteColor.a = 1f;
        foreach (SpriteRenderer spriteRenderer in spriteRenderers) {
            spriteRenderer.color = currentSpriteColor;
        }
        if (OnValueChanged != null) OnValueChanged(this);
    }

    public int GetDiscreteLightLevel() => Toolbox.DiscreteLightLevel(lightLevel, controller.isCrouching, controller.isMoving());
    private void OnTriggerEnter(Collider other) {
        if (other.tag == "concealment") {
            concealment.Add(other);
        }
    }
    private void OnTriggerExit(Collider other) {
        if (other.tag == "concealment") {
            concealment.Remove(other);
        }
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
