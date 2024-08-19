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
    (string, string) neighborEdge;
    protected Dictionary<(string, string), LineRenderer> lineRenderers = new Dictionary<(string, string), LineRenderer>();
    Dictionary<(string, string), LineRenderer> soloLineRenders = new Dictionary<(string, string), LineRenderer>();
    Dictionary<(string, string), LineRenderer> marchingAntsRenderers = new Dictionary<(string, string), LineRenderer>();
    Dictionary<(string, string), LineRenderer> partialLineRenderers = new Dictionary<(string, string), LineRenderer>();
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
            if (kvp.Key.GetVisibility() == NodeVisibility.unknown || (kvp.Key.onlyShowIfHackDeployed && !GameManager.I.playerManualHacker.deployed)) {
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
            // TODO: only show nodes on my floor?
            indicator.SetScreenPosition(cam.WorldToScreenPoint(node.position));
            indicator.Configure(node, graph, overlayHandler, NodeMouseOverCallback, NodeMouseExitCallback);
        }
    }
    public virtual void RefreshEdgeGraphicState() {
        string sceneName = SceneManager.GetActiveScene().name;
        if (graph.nodes.Count() == 0 || indicators.Count == 0) {
            return;
        }
        foreach ((string, string) edge in graph.edgePairs) {
            SetEdgeGraphicState(edge, sceneName);
        }
    }

    protected void SetEdgeGraphicState((string, string) edge, string sceneName) {
        // string[] nodes = edge.ToArray();
        U node1 = graph.nodes[edge.Item1];
        U node2 = graph.nodes[edge.Item2];
        if (node1.sceneName != sceneName || node2.sceneName != sceneName)
            return;
        // bool bothNodesVisible = node1.visibility >= NodeVisibility.mystery && node2.visibility >= NodeVisibility.mystery;
        // to draw an edge: both nodes visible
        if (graph.edgeVisibility[(node1.idn, node2.idn)] == EdgeVisibility.known) {
            LineRenderer renderer = GetLineRenderer(edge);
            SetLinePositions(renderer, node1, node2);
            SetEdgeState(renderer, node1, node2);
            LineRenderer nodeRenderer1 = GetSoloLineRenderer(node1.idn, node2.idn);
            LineRenderer nodeRenderer2 = GetSoloLineRenderer(node2.idn, node1.idn);
            nodeRenderer1.enabled = false;
            nodeRenderer2.enabled = false;
        } else {
            if (node1.visibility >= NodeVisibility.mystery) {
                LineRenderer nodeRenderer = GetSoloLineRenderer(node1.idn, node2.idn);
                SetTruncatedLinePositions(nodeRenderer, node1, node2);
                SetEdgeState(nodeRenderer, node1, node2);
                nodeRenderer.enabled = true;
            }
            if (node2.visibility >= NodeVisibility.mystery) {
                LineRenderer nodeRenderer = GetSoloLineRenderer(node2.idn, node1.idn);
                SetTruncatedLinePositions(nodeRenderer, node2, node1);
                SetEdgeState(nodeRenderer, node1, node2);
                nodeRenderer.enabled = true;
            }
        }
    }
    public void SetEdgeState((string, string) edge) {
        // string[] nodes = edge.ToArray();
        U node1 = graph.nodes[edge.Item1];
        U node2 = graph.nodes[edge.Item2];
        LineRenderer renderer = GetLineRenderer(edge);
        SetEdgeState(renderer, node1, node2);
    }

    public abstract void SetEdgeState(LineRenderer renderer, U node1, U node2);

    protected void SetLinePositions(LineRenderer renderer, U node1, U node2) {
        List<Vector3> points;
        if (node1.straightLine || node2.straightLine) {
            points = new List<Vector3>() {
                node1.position,
                node2.position
            };
        } else {
            points = pointsBetweenNodes(node1.position, node2.position);
        }

        renderer.positionCount = points.Count;
        renderer.SetPositions(points.ToArray());
    }

    public static List<Vector3> pointsBetweenNodes(Vector3 position1, Vector3 position2) {
        position1 = Toolbox.Round(position1, decimalPlaces: 1);
        position2 = Toolbox.Round(position2, decimalPlaces: 1);

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
    public void ClearAllIndicatorsAndEdges() {
        foreach (Transform child in transform) {
            if (child.name.ToLower().Contains("permanent")) continue;
            Destroy(child.gameObject);
        }
        indicators = new Dictionary<U, V>();
        lineRenderers = new Dictionary<(string, string), LineRenderer>();
        soloLineRenders = new Dictionary<(string, string), LineRenderer>();
        partialLineRenderers = new Dictionary<(string, string), LineRenderer>();
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

    protected LineRenderer GetLineRenderer((string, string) edge) {
        if (lineRenderers.ContainsKey(edge)) {
            return lineRenderers[edge];
        } else {
            LineRenderer renderer = InitializeLineRenderer();
            renderer.name = $"edge_{edge.Item1}_{edge.Item2}";
            (string, string) edge2 = (edge.Item2, edge.Item1);
            lineRenderers[edge] = renderer;
            lineRenderers[edge2] = renderer;
            return renderer;
        }
    }
    protected LineRenderer GetSoloLineRenderer(string nodeid1, string nodeid2) {
        if (soloLineRenders.ContainsKey((nodeid1, nodeid2))) {
            return soloLineRenders[(nodeid1, nodeid2)];
        } else {
            LineRenderer renderer = InitializeLineRenderer();
            renderer.name = $"solo_{nodeid1}_{nodeid2}";
            soloLineRenders[(nodeid1, nodeid2)] = renderer;
            return renderer;
        }
    }
    protected LineRenderer GetMarchingAntsLineRenderer((string, string) edge) {
        if (marchingAntsRenderers.ContainsKey(edge)) {
            return marchingAntsRenderers[edge];
        } else {
            LineRenderer renderer = InitializeLineRenderer();
            // string[] edges = edge.ToArray();
            (string, string) edge2 = (edge.Item2, edge.Item1);
            renderer.name = $"marchingants_{edge.Item1}_{edge.Item2}";
            marchingAntsRenderers[edge] = renderer;
            marchingAntsRenderers[edge2] = renderer;
            return renderer;
        }
    }
    protected LineRenderer GetPartialLineRenderer((string, string) edge) {
        if (partialLineRenderers.ContainsKey(edge)) {
            return partialLineRenderers[edge];
        } else {
            LineRenderer renderer = InitializeLineRenderer();
            renderer.textureMode = LineTextureMode.Stretch;
            renderer.material = Resources.Load("materials/partialLine") as Material;
            // string[] edges = edge.ToArray();
            (string, string) edge2 = (edge.Item2, edge.Item1);
            renderer.name = $"partial_{edge.Item1}_{edge.Item2}";
            partialLineRenderers[edge] = renderer;
            marchingAntsRenderers[edge2] = renderer;
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
        renderer.widthMultiplier = 0.025f;
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
        (string, string) edge = (sourceidn, neighboridn);
        LineRenderer renderer = GetLineRenderer(edge);
        renderer.material.color = Color.white;
        neighborEdge = edge;
    }
    public void NeighborButtonMouseExit() {
        if (neighborEdge != default) {
            SetEdgeState(neighborEdge);
        }
    }


    public V GetIndicator(string nodeTitle) {
        foreach (KeyValuePair<U, V> kvp in indicators) {
            // Debug.Log($"{kvp.Key.nodeTitle}");
            if (kvp.Key.nodeTitle == nodeTitle) {
                return kvp.Value;
            }
        }
        return null;
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