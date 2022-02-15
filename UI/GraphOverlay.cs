using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class GraphOverlay<T, U, V> : MonoBehaviour where T : Graph<U, T> where U : Node where V : NodeIndicator<U, T> {
    public UIColorSet colorSet;
    public Camera cam;
    public GameObject nodeIndicatorPrefab;
    protected Graph<U, T> graph;
    public Material lineRendererMaterial;
    Dictionary<HashSet<string>, LineRenderer> lineRenderers = new Dictionary<HashSet<string>, LineRenderer>();

    Dictionary<U, V> indicators = new Dictionary<U, V>();
    void Awake() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
    }
    public void DisableOverlay() {
        indicators = new Dictionary<U, V>();
    }

    void Update() {
        foreach (KeyValuePair<U, V> kvp in indicators) {
            Vector3 screenPoint = cam.WorldToScreenPoint(kvp.Key.position);
            kvp.Value.Configure(kvp.Key, graph);
            kvp.Value.SetScreenPosition(screenPoint);
        }
    }

    public void Refresh(T graph) {
        this.graph = graph;
        // Debug.Log($"refreshing graph {graph}");
        if (graph == null || cam == null) {
            DisableOverlay();
            return;
        }

        foreach (U node in graph.nodes.Values) {
            V indicator = GetIndicator(node);
            Vector3 screenPoint = cam.WorldToScreenPoint(node.position);
            indicator.SetScreenPosition(screenPoint);
            indicator.Configure(node, graph);
        }

        SetEdgeGraphicState();
    }
    public virtual void SetEdgeGraphicState() {
        foreach (HashSet<string> edge in graph.edgePairs) {
            LineRenderer renderer = GetLineRenderer(edge);
            string[] nodes = edge.ToArray();
            U node1 = graph.nodes[nodes[0]];
            U node2 = graph.nodes[nodes[1]];

            renderer.positionCount = 2;
            renderer.SetPositions(new Vector3[2] { node1.position, node2.position });
        }
    }

    V GetIndicator(U node) {
        if (indicators.ContainsKey(node)) {
            return indicators[node];
        } else {
            Vector3 screenPoint = cam.WorldToScreenPoint(node.position);
            GameObject newIndicator = GameObject.Instantiate(nodeIndicatorPrefab);
            V indicator = newIndicator.GetComponent<V>();
            indicator.SetScreenPosition(screenPoint);
            indicator.Configure(node, graph);
            indicator.rectTransform.SetParent(transform, false);
            indicators[node] = indicator;
            return indicator;
        }
    }



    protected LineRenderer GetLineRenderer(HashSet<string> edge) {
        if (lineRenderers.ContainsKey(edge)) {
            return lineRenderers[edge];
        } else {
            LineRenderer renderer = InitializeLineRenderer();
            lineRenderers[edge] = renderer;
            return renderer;
        }
    }

    LineRenderer InitializeLineRenderer() {
        GameObject newChild = new GameObject($"lineRenderer_{lineRenderers.Count}");
        newChild.transform.SetParent(transform, false);

        LineRenderer renderer = newChild.AddComponent<LineRenderer>();
        renderer.material = lineRendererMaterial;
        renderer.SetPositions(new Vector3[0]);
        renderer.startColor = Color.white;
        renderer.endColor = Color.white;
        renderer.alignment = LineAlignment.View;
        renderer.textureMode = LineTextureMode.Stretch;
        renderer.shadowBias = 0.5f;
        renderer.generateLightingData = false;
        renderer.materials = new Material[1] { lineRendererMaterial };
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        // renderer.lightProbeUsage = Renderer.li;
        // renderer.reflectionProbeUsage = false;
        renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        renderer.allowOcclusionWhenDynamic = false;
        renderer.sortingLayerName = "UI";
        renderer.sortingOrder = 100;

        renderer.widthCurve = new AnimationCurve(new Keyframe(0, 0.02f), new Keyframe(1, 0.02f));
        return renderer;
    }
}
