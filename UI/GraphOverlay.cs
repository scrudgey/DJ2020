using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GraphOverlay<T, U, V> : MonoBehaviour, IGraphOverlay<T, U, V> where T : Graph<U, T> where U : Node where V : NodeIndicator<U, T> {
    public UIColorSet colorSet;
    public Camera cam;
    public GameObject nodeIndicatorPrefab;
    protected Graph<U, T> graph;
    public Material lineRendererMaterial;
    public Material marchingAntsMaterial;
    Dictionary<HashSet<string>, LineRenderer> lineRenderers = new Dictionary<HashSet<string>, LineRenderer>(HashSet<string>.CreateSetComparer());
    Dictionary<string[], LineRenderer> lineRendererArrays = new Dictionary<string[], LineRenderer>();

    protected Dictionary<U, V> indicators = new Dictionary<U, V>();
    void Awake() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
    }
    public void DisableOverlay() {
        indicators = new Dictionary<U, V>();
    }

    void Update() {
        if (indicators.Count() == 0) return;

        Vector3 playerPosition = GameManager.I.playerObject.transform.position;

        Dictionary<U, float> distances = indicators.Keys.ToDictionary(node => node, node => 0f);
        if (GameManager.I.playerObject != null) {
            // distances = indicators.Keys.ToDictionary(node => node, node => Vector3.Distance(node.position, camPosition));
            distances = indicators.Keys.ToDictionary(node => node, node => Mathf.Abs(node.position.y - playerPosition.y));
            float minDistance = distances.Values.Min();
            float maxDistance = distances.Values.Max();
            distances = distances.ToList().ToDictionary(kvp => kvp.Key, kvp => (kvp.Value - minDistance) / maxDistance);
        }
        foreach (KeyValuePair<U, V> kvp in indicators) {
            // float distance = Mathf.Clamp(Mathf.Abs(kvp.Key.position.y - playerPosition.y) / 2f, 0f, 1f);
            float distance = distances[kvp.Key];
            // Debug.Log($"{kvp.Value.node.nodeTitle} {distance}");
            Vector3 screenPoint = cam.WorldToScreenPoint(kvp.Key.position);
            kvp.Value.Configure(kvp.Key, graph, this);
            kvp.Value.SetScreenPosition(screenPoint);
            // kvp.Value.ApplyDistanceEffect(distance);
        }
        foreach (HashSet<string> edge in graph.edgePairs) {
            LineRenderer renderer = GetLineRenderer(edge);
            string[] nodes = edge.ToArray();
            U node1 = graph.nodes[nodes[0]];
            // U node2 = graph.nodes[nodes[1]];
            V indicator1 = indicators[node1];
            // V indicator2 = indicators[node2];
            renderer.material.color = indicator1.image.color;
        }
    }
    public void Refresh(T graph) {
        this.graph = graph;
        // Debug.Log($"refreshing graph {graph}");
        if (graph == null || cam == null) {
            DisableOverlay();
            return;
        }
        string sceneName = SceneManager.GetActiveScene().name;
        foreach (U node in graph.nodes.Values) {
            if (node.sceneName != sceneName)
                continue;
            V indicator = GetIndicator(node);
            // Vector3 screenPoint = cam.WorldToScreenPoint(node.position);
            indicator.SetScreenPosition(cam.WorldToScreenPoint(node.position));
            indicator.Configure(node, graph, this);
        }

        SetEdgeGraphicState();
    }
    public virtual void SetEdgeGraphicState() {
        string sceneName = SceneManager.GetActiveScene().name;

        if (graph.nodes.Count() == 0 || indicators.Count == 0) {
            return;
        }

        // Debug.LogWarning($"{typeof(T).FullName} setting edge graphic state with size {graph.edgePairs.Count}");
        foreach (HashSet<string> edge in graph.edgePairs) {
            // foreach (string[] edge in graph.edgeArrays) {
            LineRenderer renderer = GetLineRenderer(edge);
            string[] nodes = edge.ToArray();
            U node1 = graph.nodes[nodes[0]];
            U node2 = graph.nodes[nodes[1]];
            // V indicator1 = indicators[node1];
            V indicator1 = GetIndicator(node1);
            V indicator2 = GetIndicator(node2);
            // V indicator2 = indicators[node2];
            if (node1.sceneName != sceneName || node2.sceneName != sceneName)
                continue;
            SetLinePositions(renderer, node1, node2);
            // renderer.material.color = colorSet.enabledColor;
            renderer.colorGradient = Toolbox.Gradient2Color(indicator1.image.color, indicator2.image.color);


            // Gradient newGradient = new Gradient();
            // GradientColorKey[] colorKey;
            // GradientAlphaKey[] alphaKey;
            // // Populate the color keys at the relative time 0 and 1 (0 and 100%)
            // colorKey = new GradientColorKey[2];
            // colorKey[0].color = indicator1.image.color;
            // colorKey[0].time = 0.0f;
            // colorKey[1].color = indicator2.image.color;
            // colorKey[1].time = 1.0f;

            // // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
            // alphaKey = new GradientAlphaKey[2];
            // alphaKey[0].alpha = indicator1.image.color.a;
            // alphaKey[0].time = 0.0f;
            // alphaKey[1].alpha = indicator2.image.color.a;
            // alphaKey[1].time = 1.0f;

            // newGradient.SetKeys(colorKey, alphaKey);
            // renderer.colorGradient = newGradient;
        }
    }

    void SetLinePositions(LineRenderer renderer, U node1, U node2) {
        // bool yFlag = Mathf.Abs(node1.position.x - node2.position.x) > 2f;
        bool xFlag = Mathf.Abs(node1.position.x - node2.position.x) > 1f;
        bool zFlag = Mathf.Abs(node1.position.z - node2.position.z) > 1f;

        List<Vector3> points = new List<Vector3>();
        points.Add(node1.position);
        // if (yFlag)
        points.Add(new Vector3(node1.position.x, node2.position.y, node1.position.z));
        if (xFlag)
            points.Add(new Vector3(node2.position.x, node2.position.y, node1.position.z));

        if (zFlag)
            points.Add(new Vector3(node2.position.x, node2.position.y, node2.position.z));

        points.Add(node2.position);
        renderer.positionCount = points.Count;
        renderer.SetPositions(points.ToArray());
    }

    V GetIndicator(U node) {
        if (indicators.ContainsKey(node)) {
            return indicators[node];
        } else {
            Vector3 screenPoint = cam.WorldToScreenPoint(node.position);
            GameObject newIndicator = GameObject.Instantiate(nodeIndicatorPrefab);
            V indicator = newIndicator.GetComponent<V>();
            indicator.SetScreenPosition(screenPoint);
            indicator.Configure(node, graph, this);
            indicator.rectTransform.SetParent(transform, false);
            indicators[node] = indicator;
            return indicator;
        }
    }

    protected LineRenderer GetLineRenderer(HashSet<string> edge) {
        if (lineRenderers.ContainsKey(edge)) {
            return lineRenderers[edge];
        } else {
            // Debug.LogWarning($"{typeof(T).FullName} Initializing line renderer for edge: {string.Join(",", edge)}");
            LineRenderer renderer = InitializeLineRenderer();
            lineRenderers[edge] = renderer;
            lineRendererArrays[edge.ToArray()] = renderer;
            return renderer;
        }
    }
    protected LineRenderer GetLineRenderer(string[] edge) {
        if (lineRendererArrays.ContainsKey(edge)) {
            return lineRendererArrays[edge];
        } else {
            // Debug.LogWarning($"{typeof(T).FullName} Initializing line renderer for edge: {string.Join(",", edge)}");
            LineRenderer renderer = InitializeLineRenderer();
            // lineRenderers[edge] = renderer;
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
        renderer.shadowBias = 0.5f;
        renderer.generateLightingData = false;
        renderer.widthCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0.01f), new Keyframe(1f, 0.01f) });
        renderer.widthMultiplier = 0.2f;
        renderer.materials = new Material[1] { lineRendererMaterial };
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        // renderer.lightProbeUsage = Renderer.li;
        // renderer.reflectionProbeUsage = false;
        renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        renderer.allowOcclusionWhenDynamic = false;
        renderer.sortingLayerName = "UI";
        renderer.sortingOrder = 100;

        renderer.widthCurve = new AnimationCurve(new Keyframe(0, 0.05f), new Keyframe(1, 0.05f));
        return renderer;
    }
}
