using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDataUtil : MonoBehaviour {
    public LevelTemplate levelData;
    public Transform floorWidget;
    public Camera mapCamera;
    public RenderTexture mapTexture;
    public int floorNumber;
    public float gridSpacing = 0.1f;
    public BoxCollider boundingBox;
    public CullingGridPoint[][] selectedFloorPoints;
    void Awake() {
        Destroy(gameObject);
    }
    // void OnDrawGizmosSelected() {
    //     Gizmos.color = new Color(1, 1, 0, 0.75F);
    //     if (selectedFloorPoints != null) {
    //         foreach (CullingGridPoint point in selectedFloorPoints) {
    //             Gizmos.DrawSphere(point.position, 0.05f);
    //         }
    //     }
    // }
}
