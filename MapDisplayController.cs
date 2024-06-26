using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MapInput {
    public int floorIncrement;
    public int zoomIncrement;
    public float zoomFloatIncrement;
    public MapDisplay3DGenerator.Mode modeChange;
    public float thetaDelta;
    public Vector2 translationInput;
    public Vector3 jumpToPoint;
    public int jumpToFloor;
    public MapMarkerData selectedMapMarker;
    public MapMarkerData clickedMapMarker;

    public Objective selectedObjective;
    public Objective clickedObjective;
}

public class MapDisplayController : MonoBehaviour {
    public enum MapDisplayLegendType { none, markers, cyber, power, alarm }
    public MapDisplay3DGenerator mapDisplay3DGenerator;

    [Header("Containers")]
    public Transform insertionPointContainer;
    public Transform extractionPointContainer;
    public Transform objectivePointContainer;
    public Transform interestPointContainer;
    public GameObject insertionPointButtonPrefab;

    public GameObject insertionBox;
    public GameObject extractionBox;
    public GameObject objectiveBox;

    List<InsertionPointSelector> selectors;
    InsertionPointSelector extractionSelector;
    InsertionPointSelector insertionSelector;

    bool mouseOverMap;
    bool mapEngaged;


    int flootIncrementThisFrame;
    int zoomIncrementThisFrame;
    float zoomFloatIncrementThisFrame;
    MapDisplay3DGenerator.Mode modeChangeThisFrame;
    float thetaDeltaThisFrame;
    Vector2 translationInput;

    Vector3 jumpToPoint;
    int jumpToFloor = -1;
    MapMarkerData selectedMarkerData;
    MapMarkerData clickedMarkerData;

    Objective selectedObjective;
    Objective clickedObjective;

    LevelPlan plan;
    SceneData sceneData;

    MapDisplay3DView view;

    void Start() {
        GameManager.OnPlayerInput += UpdateWithInput;
    }
    void OnDestroy() {
        GameManager.OnPlayerInput -= UpdateWithInput;
    }
    public void OnMouseOverMap() {
        mouseOverMap = true;
    }
    public void OnMouseExitMap() {
        mouseOverMap = false;
    }

    public void InitializeWorld(SceneData sceneData, MapDisplay3DView view) {
        Initialize(sceneData, view);
        PopulateColumns();
        insertionBox.SetActive(false);
        extractionBox.SetActive(false);
        objectiveBox.SetActive(false);
    }

    public void Initialize(LevelTemplate levelTemplate, SceneData sceneData, LevelPlan plan, MapDisplay3DView view) {
        this.plan = plan;
        Initialize(sceneData, view);
        PopulateColumns();
        PopulateObjectiveColumn(levelTemplate);
    }

    public void Initialize(SceneData sceneData, MapDisplay3DView view) {
        this.view = view;
        this.sceneData = sceneData;
        SwitchLegend(MapDisplayLegendType.markers);
        selectors = new List<InsertionPointSelector>();
        List<Transform> columns = new List<Transform>{
            insertionPointContainer,
            extractionPointContainer,
            objectivePointContainer,
            interestPointContainer
        };
        foreach (Transform column in columns) {
            foreach (Transform child in column) {
                if (child.name.Contains("header")) continue;
                Destroy(child.gameObject);
            }
        }
    }

    public void HideInsertionPoints() {
        insertionBox.SetActive(false);
        foreach (InsertionPointSelector selector in selectors) {
            if (selector.data.markerType != MapMarkerData.MapMarkerType.extractionPoint) continue;
            if (selector.data.idn == plan.extractionPointIdn) continue;
            selector.gameObject.SetActive(false);
        }
    }
    // control
    public void UpdateWithInput(PlayerInput playerInput) {
        if (mouseOverMap && (playerInput.rightMouseDown || playerInput.mouseDown)) {
            mapEngaged = true;
        } else if (!playerInput.mouseDown && !playerInput.rightMouseDown) {
            mapEngaged = false;
        }

        if (playerInput.rightMouseDown && mapEngaged) {
            thetaDeltaThisFrame += playerInput.mouseDelta.x * 0.01f;
        } else if (playerInput.mouseDown && mapEngaged) {
            translationInput += playerInput.mouseDelta * 0.01f;
        }

        zoomFloatIncrementThisFrame = playerInput.zoomInput.y * Time.unscaledDeltaTime;
    }

    public void FloorIncrementButtonCallback(int increment) {
        flootIncrementThisFrame += increment;
    }
    public void ZoomIncrementButtonCallback(int increment) {
        zoomIncrementThisFrame += increment;
    }
    public void ModeButtonCallback() {
        modeChangeThisFrame = mapDisplay3DGenerator.mode switch {
            MapDisplay3DGenerator.Mode.playerfocus => MapDisplay3DGenerator.Mode.rotate,
            MapDisplay3DGenerator.Mode.rotate => MapDisplay3DGenerator.Mode.playerfocus,
            _ => MapDisplay3DGenerator.Mode.rotate
        };
    }

    void Update() {

        MapInput input = new MapInput() {
            floorIncrement = flootIncrementThisFrame,
            zoomIncrement = zoomIncrementThisFrame,
            zoomFloatIncrement = zoomFloatIncrementThisFrame,
            modeChange = modeChangeThisFrame,
            thetaDelta = thetaDeltaThisFrame,
            translationInput = translationInput,
            jumpToFloor = jumpToFloor,
            jumpToPoint = jumpToPoint,
            selectedMapMarker = selectedMarkerData,
            clickedMapMarker = clickedMarkerData,
            selectedObjective = selectedObjective,
            clickedObjective = clickedObjective
        };
        mapDisplay3DGenerator.UpdateWithInput(input);

        flootIncrementThisFrame = 0;
        zoomIncrementThisFrame = 0;
        zoomFloatIncrementThisFrame = 0;
        modeChangeThisFrame = MapDisplay3DGenerator.Mode.none;
        thetaDeltaThisFrame = 0;
        translationInput = Vector2.zero;
        jumpToFloor = -1;
        jumpToPoint = Vector3.zero;
        clickedObjective = null;
        clickedMarkerData = null;
    }

    public void LegendTypeCallback(string type) {
        MapDisplayLegendType newLegendType = type switch {
            "marker" => MapDisplayLegendType.markers,
            "cyber" => MapDisplayLegendType.cyber,
            "power" => MapDisplayLegendType.power,
            "alarm" => MapDisplayLegendType.alarm,
            _ => MapDisplayLegendType.none
        };
        SwitchLegend(newLegendType);
    }

    void SwitchLegend(MapDisplayLegendType newType) {
        if (newType == mapDisplay3DGenerator.legendType) {
            newType = MapDisplayLegendType.none;
        }
        mapDisplay3DGenerator.legendType = newType;

        selectedMarkerData = null;
        clickedMarkerData = null;
        selectedObjective = null;
        clickedObjective = null;

        switch (newType) {
            case MapDisplayLegendType.none:
                mapDisplay3DGenerator.ClearGraph();
                mapDisplay3DGenerator.ClearMarkers();
                break;
            case MapDisplayLegendType.markers:
                mapDisplay3DGenerator.ClearGraph();
                mapDisplay3DGenerator.LoadMarkers();
                break;
            case MapDisplayLegendType.alarm:
                mapDisplay3DGenerator.ClearMarkers();
                mapDisplay3DGenerator.DisplayAlarmGraph();
                break;
            case MapDisplayLegendType.cyber:
                mapDisplay3DGenerator.ClearMarkers();
                mapDisplay3DGenerator.DisplayCyberGraph();
                break;
            case MapDisplayLegendType.power:
                mapDisplay3DGenerator.ClearMarkers();
                mapDisplay3DGenerator.DisplayPowerGraph();
                break;
        }
    }

    public void PopulateColumns() {
        foreach (MapMarkerData data in mapDisplay3DGenerator.allMapData) {
            if (data.markerType == MapMarkerData.MapMarkerType.insertionPoint ||
                data.markerType == MapMarkerData.MapMarkerType.extractionPoint ||
                data.markerType == MapMarkerData.MapMarkerType.pointOfInterest) {
                GameObject obj = GameObject.Instantiate(insertionPointButtonPrefab);
                InsertionPointSelector selector = obj.GetComponent<InsertionPointSelector>();
                Transform column = data.markerType switch {
                    MapMarkerData.MapMarkerType.insertionPoint => insertionPointContainer,
                    MapMarkerData.MapMarkerType.extractionPoint => extractionPointContainer,
                    MapMarkerData.MapMarkerType.objective => objectivePointContainer,
                    MapMarkerData.MapMarkerType.pointOfInterest => interestPointContainer
                };
                obj.transform.SetParent(column, false);

                selector.Configure(data, ColumnItemCallback);

                if (plan != null) {
                    if (data.idn == plan.insertionPointIdn) {
                        SelectInsertionPoint(selector);
                    }
                    if (data.idn == plan.extractionPointIdn) {
                        SelectExtractionPoint(selector);
                    }

                    if (plan.insertionPointIdn == "" && data.markerType == MapMarkerData.MapMarkerType.insertionPoint) {
                        SelectInsertionPoint(selector);
                    }
                    if (plan.extractionPointIdn == "" && data.markerType == MapMarkerData.MapMarkerType.extractionPoint) {
                        SelectExtractionPoint(selector);
                    }
                }

                selectors.Add(selector);
            }
        }
    }

    public void PopulateObjectiveColumn(LevelTemplate template) {
        // TODO: bonus objectives?
        foreach (Objective objective in template.AllObjectives()) {
            GameObject obj = GameObject.Instantiate(insertionPointButtonPrefab);
            InsertionPointSelector selector = obj.GetComponent<InsertionPointSelector>();
            obj.transform.SetParent(objectivePointContainer, false);
            bool visibile = objective.visibility == Objective.Visibility.known || plan.objectiveLocations.ContainsKey(objective.name);
            selector.Configure(objective, ObjectiveSelectorCallback, visibile);
        }
    }
    public void ColumnItemCallback(InsertionPointSelector selector) {
        jumpToFloor = selector.data.floorNumber;
        jumpToPoint = selector.data.worldPosition;

        selectedMarkerData = selector.data;
        clickedMarkerData = selector.data;

        selectedObjective = null;
        clickedObjective = null;

        if (selector.data.markerType == MapMarkerData.MapMarkerType.insertionPoint) {
            SelectInsertionPoint(selector);
        } else if (selector.data.markerType == MapMarkerData.MapMarkerType.extractionPoint) {
            SelectExtractionPoint(selector);
        }
    }
    public void ObjectiveSelectorCallback(InsertionPointSelector selector) {
        Objective objective = selector.objective;
        if (objective.visibility == Objective.Visibility.known || plan.objectiveLocations.ContainsKey(objective.name)) {
            string idn = plan.objectiveLocations.ContainsKey(objective.title) ? plan.objectiveLocations[objective.name] : objective.potentialSpawnPoints[0];
            jumpToPoint = objective.SpawnPointLocation(idn);
            jumpToFloor = sceneData.GetMapFloorForPosition(jumpToPoint);

            selectedMarkerData = null;
            clickedMarkerData = null;

            selectedObjective = objective;
            clickedObjective = objective;
        }
    }

    void SelectInsertionPoint(InsertionPointSelector selector) {
        if (insertionSelector != null) {
            insertionSelector.Check(false);
        }
        insertionSelector = selector;
        selector.Check(true);
        plan.insertionPointIdn = selector.data.idn;
    }
    void SelectExtractionPoint(InsertionPointSelector selector) {
        if (extractionSelector != null) {
            extractionSelector.Check(false);
        }
        extractionSelector = selector;
        selector.Check(true);
        plan.extractionPointIdn = selector.data.idn;
    }
}
