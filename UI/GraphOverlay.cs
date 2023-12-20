using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
public abstract class GraphOverlay<T, U, V> : MonoBehaviour where T : Graph<U, T> where U : Node<U> where V : NodeIndicator<U, T> {
    public UIColorSet colorSet;
    public Camera cam;
    public GameObject nodeIndicatorPrefab;
    protected Graph<U, T> graph;
    public Material lineRendererMaterial;
    public Material marchingAntsMaterial;
    HashSet<string> neighborEdge;
    Dictionary<HashSet<string>, LineRenderer> lineRenderers = new Dictionary<HashSet<string>, LineRenderer>(HashSet<string>.CreateSetComparer());
    Dictionary<string[], LineRenderer> lineRendererArrays = new Dictionary<string[], LineRenderer>();
    protected Dictionary<U, V> indicators = new Dictionary<U, V>();
    protected V mousedOverIndicator;
    public OverlayHandler overlayHandler;
    void Awake() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
    }
    public void DisableOverlay() {
        indicators = new Dictionary<U, V>();
    }

    void FixedUpdate() {
        if (indicators.Count() == 0) return;
        foreach (KeyValuePair<U, V> kvp in indicators) {
            Vector3 screenPoint = cam.WorldToScreenPoint(kvp.Key.position);
            kvp.Value.Configure(kvp.Key, graph, overlayHandler, NodeMouseOverCallback, NodeMouseExitCallback);
            kvp.Value.SetScreenPosition(screenPoint);
        }
    }
    public void Refresh(T graph) {
        this.graph = graph;
        if (graph == null || cam == null) {
            DisableOverlay();
            return;
        }
        ConfigureNodes();
        SetEdgeGraphicState();
    }

    public virtual void ConfigureNodes() {
        string sceneName = SceneManager.GetActiveScene().name;
        foreach (U node in graph.nodes.Values) {
            if (node.sceneName != sceneName)
                continue;
            V indicator = GetIndicator(node);
            indicator.SetScreenPosition(cam.WorldToScreenPoint(node.position));
            indicator.Configure(node, graph, overlayHandler, NodeMouseOverCallback, NodeMouseExitCallback);
        }
    }
    public virtual void SetEdgeGraphicState() {
        string sceneName = SceneManager.GetActiveScene().name;
        if (graph.nodes.Count() == 0 || indicators.Count == 0) {
            return;
        }
        foreach (HashSet<string> edge in graph.edgePairs) {
            string[] nodes = edge.ToArray();
            U node1 = graph.nodes[nodes[0]];
            U node2 = graph.nodes[nodes[1]];
            if (node1.sceneName != sceneName || node2.sceneName != sceneName)
                continue;
            LineRenderer renderer = GetLineRenderer(edge);
            if (node1.visibility == NodeVisibility.mapped || node2.visibility == NodeVisibility.mapped) {
                SetLinePositions(renderer, node1, node2);
                SetEdgeState(renderer, node1, node2);
            } else {
                renderer.enabled = false;
            }
        }
    }
    public void SetEdgeState(HashSet<string> edge) {
        string[] nodes = edge.ToArray();
        U node1 = graph.nodes[nodes[0]];
        U node2 = graph.nodes[nodes[1]];
        LineRenderer renderer = GetLineRenderer(edge);
        SetEdgeState(renderer, node1, node2);
    }

    public abstract void SetEdgeState(LineRenderer renderer, U node1, U node2);

    protected void SetLinePositions(LineRenderer renderer, U node1, U node2) {
        Vector3 position1 = Toolbox.Round(node1.position, decimalPlaces: 1);
        Vector3 position2 = Toolbox.Round(node2.position, decimalPlaces: 1);

        bool yFlag = Mathf.Abs(position1.y - position2.y) > 2f;
        bool xFlag = Mathf.Abs(position1.x - position2.x) > 1f;
        bool zFlag = Mathf.Abs(position1.z - position2.z) > 1f;

        List<Vector3> points = new List<Vector3>();
        points.Add(position1);
        // if (yFlag)
        points.Add(new Vector3(position1.x, position2.y, position1.z));
        // if (xFlag)
        points.Add(new Vector3(position2.x, position2.y, position1.z));
        // if (zFlag)
        points.Add(new Vector3(position2.x, position2.y, position2.z));

        points.Add(position2);

        // TODO: snap

        renderer.positionCount = points.Count;
        renderer.SetPositions(points.ToArray());
    }

    protected V GetIndicator(U node) {
        if (indicators.ContainsKey(node)) {
            return indicators[node];
        } else {
            Vector3 screenPoint = cam.WorldToScreenPoint(node.position);
            GameObject newIndicator = GameObject.Instantiate(nodeIndicatorPrefab);
            V indicator = newIndicator.GetComponent<V>();
            indicator.SetScreenPosition(screenPoint);
            indicator.Configure(node, graph, overlayHandler, NodeMouseOverCallback, NodeMouseExitCallback);
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
            lineRendererArrays[edge.ToArray()] = renderer;
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
        renderer.numCapVertices = 5;
        renderer.numCornerVertices = 5;
        renderer.shadowBias = 0.5f;
        renderer.generateLightingData = false;
        // renderer.widthCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 1f), new Keyframe(1f, 1f) });
        // renderer.widthMultiplier = 0.2f;
        renderer.widthMultiplier = 0.05f;
        renderer.materials = new Material[1] { lineRendererMaterial };
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        // renderer.lightProbeUsage = Renderer.li;
        // renderer.reflectionProbeUsage = false;
        renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        renderer.allowOcclusionWhenDynamic = false;
        // renderer.sortingLayerName = "linerender";
        // renderer.sortingOrder = 100;

        renderer.gameObject.layer = LayerUtil.GetLayer(Layer.linerender);

        renderer.widthCurve = new AnimationCurve(new Keyframe(0, 1f), new Keyframe(1, 1f));
        return renderer;
    }


    public virtual void NodeMouseOverCallback(NodeIndicator<U, T> indicator) {
        mousedOverIndicator = (V)indicator;
        overlayHandler.NodeMouseOverCallback(indicator);
    }
    public virtual void NodeMouseExitCallback(NodeIndicator<U, T> indicator) {
        mousedOverIndicator = null;
        overlayHandler.NodeMouseExitCallback();
    }

    public void NeighborButtonClick(string idn) {
        NeighborButtonMouseExit();
        U node = graph.nodes[idn];
        V indicator = GetIndicator(node);
        overlayHandler.NodeSelectCallback(indicator);
    }
    public void NeighborButtonMouseOver(string sourceidn, string neighboridn) {
        HashSet<string> edge = new HashSet<string> { sourceidn, neighboridn };
        LineRenderer renderer = GetLineRenderer(edge);
        renderer.material.color = Color.white;
        neighborEdge = edge;
    }
    public void NeighborButtonMouseExit() {
        if (neighborEdge != null) {
            SetEdgeState(neighborEdge);
        }
    }
}
