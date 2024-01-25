using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MapDisplay3DGenerator : MonoBehaviour, IBindable<MapDisplay3DGenerator> {
    public enum Mode { none, playerfocus, rotate }
    public Action<MapDisplay3DGenerator> OnValueChanged { get; set; }
    public MapDisplayController.MapDisplayLegendType legendType;
    public Mode mode;
    public List<MeshRenderer> quads;
    public Material materialFloorHidden;
    public Material materialFloorHighlight;
    List<Texture2D> mapImages;
    public List<MapMarkerData> mapData;
    public Dictionary<string, MarkerConfiguration> nodeData;
    public GraphIconReference graphIconReference;
    [Header("camera")]
    public Camera mapCamera;
    public Transform cameraTransform;
    [Header("textures")]
    public PixelUpscaleRender upscaleRender;
    public RawImage mapViewImage;
    public RenderTexture texture_256;
    public RenderTexture texture_512;
    public RenderTexture texture_1024;
    public RenderTexture texture_2048;
    [Header("network")]
    public GameObject lineRenderObjectPrefab;
    public Transform graphContainer;

    // public Vector3 
    public int currentFloor;
    public int numberFloors;
    float theta;
    float thetaVelocity;
    Vector3 origin;
    float zoomFloatAmount;
    int zoomLevel;
    LevelTemplate template;

    CyberGraph cyberGraph;
    PowerGraph powerGraph;
    AlarmGraph alarmGraph;

    CyberGraph currentCyberGraph;
    PowerGraph currentPowerGraph;
    AlarmGraph currentAlarmGraph;

    public void Initialize(LevelState state) {
        Initialize(state.template);
        cyberGraph = state.delta.cyberGraph;
        powerGraph = state.delta.powerGraph;
        alarmGraph = state.delta.alarmGraph;
    }
    public void Initialize(LevelTemplate template) {
        this.template = template;
        // this.state = state;
        // TODO: somehow, display graph information from plan

        mapImages = MapMarker.LoadMapImages(template.levelName, template.sceneName);
        // mapData = MapMarker.LoadMapMetaData(template.levelName, template.sceneName);
        nodeData = new Dictionary<string, MarkerConfiguration>();
        numberFloors = mapImages.Count;
        // TODO: set theta based on character camera rotation offset
        theta = 3.925f;

        for (int i = 0; i < quads.Count; i++) {
            MeshRenderer renderer = quads[i];
            if (i >= mapImages.Count) {
                renderer.enabled = false;
            } else {
                renderer.enabled = true;
                renderer.material = materialFloorHidden;
                renderer.material.mainTexture = mapImages[i];
            }
        }
        zoomFloatAmount = 1.5f;
        SetZoomLevel(1);
        int playerFloor = template.GetFloorForPosition(GameManager.I.playerPosition);
        SelectFloor(playerFloor);
        ChangeMode(Mode.playerfocus);
    }

    void Update() {
        theta += thetaVelocity * Time.unscaledDeltaTime;
        if (theta < -6.28) {
            theta += 6.28f;
        } else if (theta > 6.28) {
            theta -= 6.28f;
        }

        float y = 1.375f + (0.125f * currentFloor);
        float x = 1.412f * Mathf.Cos(theta);
        float z = 1.412f * Mathf.Sin(theta);

        float rotX = 45;
        float rotY = 270 - (45 * (theta / 0.785f));
        float rotZ = 0f;

        if (rotY < 0) {
            rotY += 360f;
        }

        Quaternion targetRotation = Quaternion.Euler(rotX, rotY, rotZ);

        Vector3 targetPosition = new Vector3(x, y, z) + origin;
        Vector3 updatedPosition = targetPosition;

        cameraTransform.localPosition = updatedPosition;
        cameraTransform.localRotation = targetRotation;
    }

    void ChangeMode(Mode newMode) {
        mode = newMode;
        switch (mode) {
            case Mode.playerfocus:
                int floor = template.GetFloorForPosition(GameManager.I.playerPosition);
                origin = WorldToGeneratorPosition(GameManager.I.playerPosition) - transform.position;
                int playerFloor = template.GetFloorForPosition(GameManager.I.playerPosition);
                SelectFloor(playerFloor);
                thetaVelocity = 0f;
                break;
            case Mode.rotate:
                thetaVelocity = 0.5f;
                origin = Vector3.zero;
                break;
        }
    }
    void SelectFloor(int toFloor) {
        for (int i = 0; i < quads.Count; i++) {
            MeshRenderer renderer = quads[i];
            if (i >= mapImages.Count) {
                renderer.enabled = false;
            } else if (i > toFloor) {
                renderer.enabled = false;
            } else if (i == toFloor) {
                renderer.enabled = true;
                renderer.material = materialFloorHighlight;
                renderer.material.mainTexture = mapImages[toFloor];
            } else if (i < toFloor) {
                renderer.enabled = true;
                renderer.material = materialFloorHidden;
                renderer.material.mainTexture = mapImages[i];
            }
        }
        currentFloor = toFloor;
        // DisplayGraph(state.delta.cyberGraph);
        if (currentAlarmGraph != null) {
            DisplayGraph(currentAlarmGraph);
        } else if (currentCyberGraph != null) {
            DisplayGraph(currentCyberGraph);
        } else if (currentPowerGraph != null) {
            DisplayGraph(currentPowerGraph);
        }
    }
    void SetZoomLevel(int level) {
        // small: 1024 large:   2045    camera: 1
        // small: 512  large: 1024     camera: 0.5
        // small: 256  large:  512     camera: 0.25
        //                             camera: 0.125
        zoomLevel = level;
        switch (level) {
            case 0:
                upscaleRender.small = texture_1024;
                upscaleRender.large = texture_2048;
                mapCamera.orthographicSize = 1;
                break;
            case 1:
                upscaleRender.small = texture_512;
                upscaleRender.large = texture_1024;
                mapCamera.orthographicSize = 0.5f;
                break;
            case 2:
                upscaleRender.small = texture_256;
                upscaleRender.large = texture_512;
                mapCamera.orthographicSize = 0.25f;
                break;
            case 3:
                upscaleRender.small = texture_256;
                upscaleRender.large = texture_512;
                mapCamera.orthographicSize = 0.125f;
                break;
        }
        mapCamera.targetTexture = upscaleRender.large;
        mapViewImage.texture = upscaleRender.small;
    }
    public string GetStatsString() {
        return $"{zoomFloatAmount:F2}\n{theta:F2}\n({origin.x:F2}, {origin.z:F2})";
    }

    public Vector3 WorldToGeneratorPosition(Vector3 worldPosition, bool debug = false) {

        int floorNumber = template.GetFloorForPosition(worldPosition);

        Vector2 quadPosition = WorldToQuadPosition(worldPosition);

        // transform to map generator position
        MeshRenderer quad = quads[0];

        Vector3 generatorPosition = new Vector3(
                quad.bounds.extents.x * quadPosition.x * 2,
                0f,
                quad.bounds.extents.z * quadPosition.y * 2) + quad.transform.position - quad.bounds.extents;

        generatorPosition.y += floorNumber * 0.125f;

        if (debug) {
            Debug.Log(worldPosition);
            Debug.Log(quadPosition);
            Debug.Log(generatorPosition);
            Debug.DrawLine(worldPosition, quadPosition, Color.white, 5f);
            Debug.DrawLine(quadPosition, generatorPosition, Color.white, 5f);
        }

        return generatorPosition;
    }
    public Vector2 WorldToViewportPoint(Vector3 worldPosition) {
        Vector3 generatorPosition = WorldToGeneratorPosition(worldPosition);
        return mapCamera.WorldToViewportPoint(generatorPosition);
    }

    public Vector2 WorldToQuadPosition(Vector3 worldPosition) {
        return new Vector2(
            template.mapUnitNorth.x * worldPosition.x,
            template.mapUnitEast.y * worldPosition.z) + new Vector2(template.mapOrigin.x, template.mapOrigin.y);
    }

    public void UpdateWithInput(MapInput input) {
        theta += input.thetaDelta;
        if (input.thetaDelta != 0)
            thetaVelocity = 0f;

        Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(mapCamera.transform.rotation * Vector3.forward, Vector3.up).normalized;
        Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Vector3.up);

        if (input.translationInput != Vector2.zero) {
            Vector3 inputDirection = new Vector3(input.translationInput.x, 0, input.translationInput.y);
            Vector3 translation = -1f * (cameraPlanarRotation * inputDirection);
            origin += translation * mapCamera.orthographicSize;
            origin = Vector3.ClampMagnitude(origin, 1f);
        }

        if (input.zoomFloatIncrement + input.zoomIncrement != 0)
            HandleZoomInput(input.zoomFloatIncrement + input.zoomIncrement);

        if (input.floorIncrement != 0)
            HandleFloorIncrement(input.floorIncrement);

        if (input.modeChange != Mode.none) {
            ChangeMode(input.modeChange);
        }

        OnValueChanged?.Invoke(this);
    }
    void HandleZoomInput(float increment) {
        zoomFloatAmount += increment;
        zoomFloatAmount = Mathf.Min(zoomFloatAmount, 4f);
        zoomFloatAmount = Mathf.Max(zoomFloatAmount, 0f);
        if ((int)zoomFloatAmount != zoomLevel) {
            SetZoomLevel((int)zoomFloatAmount);
        }
    }
    void HandleFloorIncrement(int increment) {
        int targetFloor = currentFloor + increment;
        targetFloor = Mathf.Max(0, targetFloor);
        targetFloor = Mathf.Min(targetFloor, numberFloors - 1);
        SelectFloor(targetFloor);
    }

    public void DisplayGraph<T, W>(Graph<T, W> graph) where T : Node<T> where W : Graph<T, W> {
        ClearGraph();
        foreach (HashSet<string> edge in graph.edgePairs) {
            string[] nodes = edge.ToArray();
            T node1 = graph.nodes[nodes[0]];
            T node2 = graph.nodes[nodes[1]];

            int floor1 = template.GetFloorForPosition(node1.position);
            int floor2 = template.GetFloorForPosition(node2.position);

            if (floor1 != currentFloor || floor2 != currentFloor) continue;

            // if (node1.sceneName != sceneName || node2.sceneName != sceneName)
            //     return;
            if (graph.edgeVisibility[(node1.idn, node2.idn)] == EdgeVisibility.known) {
                LineRenderer renderer = GetLineRenderer(node1, node2);
                SetLinePositions(renderer, node1, node2);
            }
        }

        foreach (T node in graph.nodes.Values) {
            switch (node) {
                case CyberNode cybernode:
                    nodeData[node.idn] = cybernode.GetConfiguration(graphIconReference);
                    break;
                case AlarmNode alarmNode:
                    nodeData[node.idn] = alarmNode.GetConfiguration(graphIconReference);
                    break;
                case PowerNode powerNode:
                    nodeData[node.idn] = powerNode.GetConfiguration(graphIconReference);
                    break;
            }
        }

        switch (graph) {
            case CyberGraph cyberGraph:
                currentCyberGraph = cyberGraph;
                break;
            case PowerGraph powerGraph:
                currentPowerGraph = powerGraph;
                break;
            case AlarmGraph alarmGraph:
                currentAlarmGraph = alarmGraph;
                break;
        }
    }
    LineRenderer GetLineRenderer<T>(T node1, T node2) where T : Node<T> {
        GameObject lineObject = GameObject.Instantiate(lineRenderObjectPrefab, node1.position, Quaternion.identity);
        LineRenderer renderer = lineObject.GetComponent<LineRenderer>();
        lineObject.transform.SetParent(graphContainer);
        return renderer;
    }
    void SetLinePositions<T>(LineRenderer renderer, T node1, T node2) where T : Node<T> {
        List<Vector3> points = new List<Vector3>();

        Vector3 position1 = Toolbox.Round(node1.position, decimalPlaces: 1);
        Vector3 position2 = Toolbox.Round(node2.position, decimalPlaces: 1);

        points.Add(position1);
        points.Add(new Vector3(position2.x, position1.y, position1.z));
        points.Add(new Vector3(position2.x, position2.y, position2.z));
        points.Add(position2);

        points = points.Select(point => WorldToGeneratorPosition(point)).ToList();

        renderer.positionCount = points.Count;
        renderer.SetPositions(points.ToArray());
    }

    public void ClearGraph() {
        nodeData = new Dictionary<string, MarkerConfiguration>();
        foreach (Transform child in graphContainer) {
            Destroy(child.gameObject);
        }
        currentCyberGraph = null;
        currentAlarmGraph = null;
        currentPowerGraph = null;
    }
    public void ClearMarkers() {
        mapData = new List<MapMarkerData>();
    }
    public void LoadMarkers() {
        mapData = MapMarker.LoadMapMetaData(template.levelName, template.sceneName);
    }
    public void DisplayCyberGraph() {
        DisplayGraph(cyberGraph);
    }
    public void DisplayPowerGraph() {
        DisplayGraph(powerGraph);
    }
    public void DisplayAlarmGraph() {
        DisplayGraph(alarmGraph);
    }
}
