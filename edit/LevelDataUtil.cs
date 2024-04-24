using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDataUtil : MonoBehaviour {
    public LevelTemplate levelData;
    public SceneData sceneData;
    public Transform floorWidget;
    public Camera mapCamera;
    public RenderTexture mapTexture;
    public int floorNumber;
    public float gridSpacing = 0.1f;
    public BoxCollider boundingBox;
    public string findCullingIdn;
    public CullingGrid selectedGrid;
    void Awake() {
        Destroy(gameObject);
    }
    void OnDrawGizmosSelected() {
        Gizmos.color = new Color(1, 1, 0, 0.75F);
        if (selectedGrid != null && selectedGrid.points != null) {
            foreach (CullingGridPoint[] row in selectedGrid.points) {
                foreach (CullingGridPoint point in row) {
                    Gizmos.DrawSphere(point.position, 0.05f);
                }
            }
        }
    }
}
