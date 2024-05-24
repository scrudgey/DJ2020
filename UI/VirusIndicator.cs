using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VirusIndicator : MonoBehaviour {
    public VirusProgram virus;
    public RectTransform rectTransform;
    public Camera cam;
    public Image image;
    public Sprite[] sprites;
    Vector3 randomOffset;

    public void Initialize(VirusProgram virus, Camera cam) {
        this.virus = virus;
        this.cam = cam;
        randomOffset = 20f * Random.insideUnitSphere;
        StartCoroutine(flipSprites());
    }
    public void OverlayUpdate() {
        SetScreenPosition(virus.position);
    }

    public void SetScreenPosition(Vector3 worldPosition) {
        Vector3 screenPoint = cam.WorldToScreenPoint(worldPosition);

        // bool keepOnScreen = overlayHandler.selectedNode != null && graph.edges.ContainsKey(node.idn) ?
        //    graph.edges[node.idn].Contains(overlayHandler.selectedNode.GetNodeId()) : false;

        // keepOnScreen = keepOnScreen || node.alwaysOnScreen;

        // if (keepOnScreen) {
        //     position.x = Math.Max(position.x, 0);
        //     position.x = Math.Min(position.x, 1905);
        //     position.y = Math.Max(position.y, 0);
        //     position.y = Math.Min(position.y, 1065);
        // }
        rectTransform.position = screenPoint + randomOffset;
    }

    IEnumerator flipSprites() {
        float timer = 0f;
        float duration = 0.5f;
        int index = 0;
        while (true) {
            timer += Time.unscaledDeltaTime;
            if (timer > duration) {
                timer -= duration;
                index++;
                if (index >= sprites.Length) index = 0;
                image.sprite = sprites[index];
            }
            yield return null;
        }
    }
}
