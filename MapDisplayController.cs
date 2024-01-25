using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapInput {
    public int floorIncrement;
    public int zoomIncrement;
    public float zoomFloatIncrement;
    public MapDisplay3DGenerator.Mode modeChange;
    public float thetaDelta;
    public Vector2 translationInput;
}

public class MapDisplayController : MonoBehaviour {
    public enum MapDisplayLegendType { none, markers, cyber, power, alarm }
    public MapDisplay3DGenerator mapDisplay3DGenerator;

    [Header("Containers")]
    public bool doPopulateColumns;
    public Transform insertionPointContainer;
    public Transform extractionPointContainer;
    public Transform objectivePointContainer;
    public Transform interestPointContainer;
    public GameObject insertionPointButtonPrefab;

    InsertionPointSelector extractionSelector;
    InsertionPointSelector insertionSelector;
    MapMarkerIndicator extractionIndicator;
    MapMarkerIndicator insertionIndicator;

    bool mouseOverMap;
    bool mapEngaged;


    int flootIncrementThisFrame;
    int zoomIncrementThisFrame;
    float zoomFloatIncrementThisFrame;
    MapDisplay3DGenerator.Mode modeChangeThisFrame;
    float thetaDeltaThisFrame;
    Vector2 translationInput;

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

    public void Initialize() {
        if (doPopulateColumns)
            PopulateColumns();
    }

    // control
    public void UpdateWithInput(PlayerInput playerInput) {
        if (mouseOverMap && (playerInput.rightMouseDown || playerInput.mouseDown)) {
            mapEngaged = true;
        } else if (!playerInput.mouseDown && !playerInput.rightMouseDown) {
            mapEngaged = false;
        }

        if (playerInput.rightMouseDown && mapEngaged) {
            // thetaDeltaThisFrame += playerInput.mouseDelta.x * Time.unscaledDeltaTime;
            thetaDeltaThisFrame += playerInput.mouseDelta.x * 0.01f;
        } else if (playerInput.mouseDown && mapEngaged) {
            // translationInput += playerInput.mouseDelta * Time.unscaledDeltaTime;
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
            translationInput = translationInput
        };
        mapDisplay3DGenerator.UpdateWithInput(input);

        flootIncrementThisFrame = 0;
        zoomIncrementThisFrame = 0;
        zoomFloatIncrementThisFrame = 0;
        modeChangeThisFrame = MapDisplay3DGenerator.Mode.none;
        thetaDeltaThisFrame = 0;
        translationInput = Vector2.zero;
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
        List<Transform> columns = new List<Transform>{
            insertionPointContainer,
            extractionPointContainer,
            objectivePointContainer,
            interestPointContainer
        };
        foreach (Transform column in columns) {
            foreach (Transform child in column) {
                Destroy(child.gameObject);
            }
        }
        foreach (MapMarkerData data in mapDisplay3DGenerator.mapData) {
            if (data.markerType == MapMarkerData.MapMarkerType.insertionPoint ||
                data.markerType == MapMarkerData.MapMarkerType.extractionPoint ||
                data.markerType == MapMarkerData.MapMarkerType.objective ||
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

                // if (data.idn == plan.insertionPointIdn) {
                //     SelectInsertionPoint(selector, indicators[data]);
                // }
                // if (data.idn == plan.extractionPointIdn) {
                //     SelectExtractionPoint(selector, indicators[data]);
                // }

                // if (plan.insertionPointIdn == "" && insertionIndicator == null && data.markerType == MapMarkerData.MapMarkerType.insertionPoint) {
                //     SelectInsertionPoint(selector, indicators[data]);
                // }
                // if (plan.extractionPointIdn == "" && extractionIndicator == null && data.markerType == MapMarkerData.MapMarkerType.extractionPoint) {
                //     SelectExtractionPoint(selector, indicators[data]);
                // }
            }
        }
    }
    public void ColumnItemCallback(InsertionPointSelector selector) {
        // Toolbox.RandomizeOneShot(audioSource, columnButtonSound);
        // if (selector.data.floorNumber != selectedFloor) {
        //     ChangeFloorView(selector.data.floorNumber);
        // }
        // MapMarkerIndicator indicator = indicators[selector.data];
        // indicator.ShowClickPulse();
        // ScrollTo(indicator.transform);
        // if (selector.data.markerType == MapMarkerData.MapMarkerType.insertionPoint) {
        //     SelectInsertionPoint(selector, indicator);
        // } else if (selector.data.markerType == MapMarkerData.MapMarkerType.extractionPoint) {
        //     SelectExtractionPoint(selector, indicator);
        // }
    }

    void SelectInsertionPoint(InsertionPointSelector selector, MapMarkerIndicator indicator) {
        if (insertionIndicator != null) {
            insertionIndicator.ShowSelection(false);
        }
        if (insertionSelector != null) {
            insertionSelector.Check(false);
        }
        insertionIndicator = indicator;
        insertionSelector = selector;
        indicator.ShowSelection(true);
        selector.Check(true);
        // plan.insertionPointIdn = selector.data.idn;
    }
    void SelectExtractionPoint(InsertionPointSelector selector, MapMarkerIndicator indicator) {
        if (extractionIndicator != null) {
            extractionIndicator.ShowSelection(false);
        }
        if (extractionSelector != null) {
            extractionSelector.Check(false);
        }
        extractionIndicator = indicator;
        extractionSelector = selector;
        indicator.ShowSelection(true);
        selector.Check(true);

        // plan.extractionPointIdn = selector.data.idn;
    }
}
