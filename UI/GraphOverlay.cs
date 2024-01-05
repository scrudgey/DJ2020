using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
public abstract class GraphOverlay<T, U, V> : MonoBehaviour where T : Graph<U, T> where U : Node<U> where V : NodeIndicator<U, T> {
    public UIColorSet colorSet;
    public Camera cam;
    public GameObject nodeIndicatorPrefab;
    protected T graph;
    public Material lineRendererMaterial;
    public Material marchingAntsMaterial;
    HashSet<string> neighborEdge;
    Dictionary<HashSet<string>, LineRenderer> lineRenderers = new Dictionary<HashSet<string>, LineRenderer>(HashSet<string>.CreateSetComparer());
    Dictionary<HashSet<string>, LineRenderer> marchingAntsRenderers = new Dictionary<HashSet<string>, LineRenderer>(HashSet<string>.CreateSetComparer());
    Dictionary<string[], LineRenderer> lineRendererArrays = new Dictionary<string[], LineRenderer>();
    protected Dictionary<U, V> indicators = new Dictionary<U, V>();
    protected V mousedOverIndicator;
    public OverlayHandler overlayHandler;
    void Awake() {
        foreach (Transform child in transform) {
            if (child.name.ToLower().Contains("permanent")) continue;
            Destroy(child.gameObject);
        }
    }
    public void DisableOverlay() {
        indicators = new Dictionary<U, V>();
    }
    public virtual void OnOverlayActivate() {

    }

    void FixedUpdate() {
        if (indicators.Count() == 0) return;
        UpdateNodes();
    }

    public void Refresh(T graph) {
        this.graph = graph;
        if (graph == null || cam == null) {
            DisableOverlay();
            return;
        }
        RefreshIndicators();
    }
    public virtual void UpdateNodes() {
        foreach (KeyValuePair<U, V> kvp in indicators) {
            Vector3 screenPoint = cam.WorldToScreenPoint(kvp.Key.position);
            if (kvp.Key.GetVisibility() == NodeVisibility.unknown) {
                kvp.Value.gameObject.SetActive(false);
            } else {
                kvp.Value.gameObject.SetActive(true);
            }
            kvp.Value.SetScreenPosition(screenPoint);
            kvp.Value.SetGraphicalState(kvp.Key);
        }
    }
    public virtual void RefreshIndicators() {
        RefreshNodes();
        RefreshEdgeGraphicState();
    }
    public virtual void RefreshNodes() {
        string sceneName = SceneManager.GetActiveScene().name;
        foreach (U node in graph.nodes.Values) {
            if (node.sceneName != sceneName)
                continue;
            V indicator = GetIndicator(node);
            indicator.SetScreenPosition(cam.WorldToScreenPoint(node.position));
            indicator.Configure(node, graph, overlayHandler, NodeMouseOverCallback, NodeMouseExitCallback);

        }
    }
    public virtual void RefreshEdgeGraphicState() {
        string sceneName = SceneManager.GetActiveScene().name;
        if (graph.nodes.Count() == 0 || indicators.Count == 0) {
            return;
        }
        foreach (HashSet<string> edge in graph.edgePairs) {
            SetEdgeGraphicState(edge, sceneName);
            // string[] nodes = edge.ToArray();
            // U node1 = graph.nodes[nodes[0]];
            // U node2 = graph.nodes[nodes[1]];
            // if (node1.sceneName != sceneName || node2.sceneName != sceneName)
            //     continue;
            // LineRenderer renderer = GetLineRenderer(edge);
            // if (node1.visibility == NodeVisibility.mapped || node2.visibility == NodeVisibility.mapped) {
            //     SetLinePositions(renderer, node1, node2);
            //     SetEdgeState(renderer, node1, node2);
            // } else {
            //     SetTruncatedLinePositions(renderer, node1, node2);
            //     SetEdgeState(renderer, node1, node2);
            //     // renderer.enabled = false;
            // }
        }
    }

    protected void SetEdgeGraphicState(HashSet<string> edge, string sceneName) {
        string[] nodes = edge.ToArray();
        U node1 = graph.nodes[nodes[0]];
        U node2 = graph.nodes[nodes[1]];
        if (node1.sceneName != sceneName || node2.sceneName != sceneName)
            return;
        LineRenderer renderer = GetLineRenderer(edge);
        if (node1.visibility == NodeVisibility.mapped || node2.visibility == NodeVisibility.mapped) {
            SetLinePositions(renderer, node1, node2);
            SetEdgeState(renderer, node1, node2);
        } else {
            SetTruncatedLinePositions(renderer, node1, node2);
            SetEdgeState(renderer, node1, node2);
            // renderer.enabled = false;
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
        List<Vector3> points = pointsBetweenNodes(node1, node2);

        renderer.positionCount = points.Count;
        renderer.SetPositions(points.ToArray());
    }

    protected List<Vector3> pointsBetweenNodes(U node1, U node2) {
        Vector3 position1 = Toolbox.Round(node1.position, decimalPlaces: 1);
        Vector3 position2 = Toolbox.Round(node2.position, decimalPlaces: 1);

        // bool yFlag = Mathf.Abs(position1.y - position2.y) > 2f;
        // bool xFlag = Mathf.Abs(position1.x - position2.x) > 1f;
        // bool zFlag = Mathf.Abs(position1.z - position2.z) > 1f;

        List<Vector3> points = new List<Vector3>();
        points.Add(position1);
        // if (yFlag)
        points.Add(new Vector3(position1.x, position2.y, position1.z));
        // if (xFlag)
        points.Add(new Vector3(position2.x, position2.y, position1.z));
        // if (zFlag)
        points.Add(new Vector3(position2.x, position2.y, position2.z));

        points.Add(position2);

        return points;
    }

    protected void SetTruncatedLinePositions(LineRenderer renderer, U node1, U node2) {
        Vector3 position1 = Toolbox.Round(node1.position, decimalPlaces: 1);
        Vector3 position2 = Toolbox.Round(node2.position, decimalPlaces: 1);

        // do like regular line positioning- but measure the distance at each point.
        // feels good to write garbage code sometimes
        // float distanceBudget = 1f;
        FiniteLineBuilder lineBuilder = new FiniteLineBuilder(position1, 0.75f);
        lineBuilder.AddPoint(new Vector3(position1.x, position2.y, position1.z));
        lineBuilder.AddPoint(new Vector3(position2.x, position2.y, position1.z));
        lineBuilder.AddPoint(new Vector3(position2.x, position2.y, position2.z));

        List<Vector3> points = lineBuilder.GetPoints();
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
    protected LineRenderer GetMarchingAntsLineRenderer(HashSet<string> edge) {
        if (marchingAntsRenderers.ContainsKey(edge)) {
            return marchingAntsRenderers[edge];
        } else {
            LineRenderer renderer = InitializeLineRenderer();
            marchingAntsRenderers[edge] = renderer;
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
        renderer.textureMode = LineTextureMode.Tile;
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


public class FiniteLineBuilder {
    List<Vector3> points;
    Vector3 currentPoint;
    float distanceBudget;
    public FiniteLineBuilder(Vector3 initialPoint, float distancebudget) {
        this.currentPoint = initialPoint;
        this.distanceBudget = distancebudget;
        this.points = new List<Vector3>();
        points.Add(initialPoint);
    }

    public void AddPoint(Vector3 nextPoint) {
        if (distanceBudget <= 0) return;
        Vector3 displacement = (nextPoint - currentPoint);

        float distance = Mathf.Min(distanceBudget, displacement.magnitude);

        Vector3 finalPoint = currentPoint + (distance * displacement.normalized);

        points.Add(finalPoint);

        currentPoint = finalPoint;

        distanceBudget -= distance;
    }

    public List<Vector3> GetPoints() {
        return points;
    }
}