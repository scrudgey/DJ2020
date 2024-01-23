using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MapDisplay3DView : IBinder<MapDisplay3DGenerator> {
    public MapDisplay3DGenerator mapDisplay3Dgenerator;
    [Header("floor indicators")]
    public Transform floorPipContainer;
    public TextMeshProUGUI floorNumberCaption;
    public TextMeshProUGUI statsView;
    public Color selectedColor;
    public Color deselectedColor;
    [Header("map markers")]
    public RectTransform markerContainer;
    public GameObject mapMarkerPrefab;
    public GameObject nodeMarkerPrefab;
    public RectTransform mapRect;
    [Header("legend indicators")]
    public Image legendMarkersIndicator;
    public Image legendCyberIndicator;
    public Image legendPowerIndicator;
    public Image legendAlarmIndicator;
    public Color legendActiveColor;
    public Color legendInactiveColor;
    Dictionary<MapMarkerData, MapMarkerIndicator> indicators;
    Dictionary<string, MiniMapNodeMarker> nodeMarkers;
    MapMarkerIndicator playerMarker;
    List<Image> floorPips;
    int numberFloors;
    LevelTemplate template;
    LevelPlan plan;

    public void Initialize(LevelState levelState) {
        this.template = levelState.template;
        this.plan = levelState.plan;
        floorPips = new List<Image>();

        foreach (Transform child in markerContainer) {
            if (child.name == "mapview") continue;
            Destroy(child.gameObject);
        }
        indicators = new Dictionary<MapMarkerData, MapMarkerIndicator>();
        nodeMarkers = new Dictionary<string, MiniMapNodeMarker>();

        for (int i = floorPipContainer.childCount - 1; i >= 0; i--) {
            Transform pipChild = floorPipContainer.GetChild(i);
            Image pip = pipChild.GetComponent<Image>();
            floorPips.Add(pip);
        }

        mapDisplay3Dgenerator.Initialize(levelState);
        Bind(mapDisplay3Dgenerator.gameObject);
    }
    MapMarkerIndicator SpawnNewMapMarker() {
        GameObject objM = GameObject.Instantiate(mapMarkerPrefab);
        objM.transform.SetParent(markerContainer, false);
        MapMarkerIndicator indicator = objM.GetComponent<MapMarkerIndicator>();
        return indicator;
    }

    void RemoveMarker(MapMarkerData key) {
        MapMarkerIndicator indicator = indicators[key];
        Destroy(indicator.gameObject);
        indicators.Remove(key);
    }

    MiniMapNodeMarker SpawnNodeMarker() {
        GameObject objM = GameObject.Instantiate(nodeMarkerPrefab);
        objM.transform.SetParent(markerContainer, false);
        MiniMapNodeMarker indicator = objM.GetComponent<MiniMapNodeMarker>();
        return indicator;
    }

    void RemoveNodeMarker(string key) {
        MiniMapNodeMarker indicator = nodeMarkers[key];
        Destroy(indicator.gameObject);
        nodeMarkers.Remove(key);
    }

    //view
    override public void HandleValueChanged(MapDisplay3DGenerator generator) {
        for (int i = floorPipContainer.childCount - 1; i >= 0; i--) {
            Image floorPip = floorPips[i];
            if (i > generator.numberFloors) {
                floorPip.enabled = false;
            }
            if (i == generator.currentFloor) {
                floorPip.color = selectedColor;
            } else {
                floorPip.color = deselectedColor;
            }
        }
        floorNumberCaption.text = $"{template.sceneDescriptor}: floor {generator.currentFloor}";

        foreach (KeyValuePair<MapMarkerData, MapMarkerIndicator> kvp in indicators) {
            kvp.Value.gameObject.SetActive(kvp.Key.floorNumber == generator.currentFloor);
        }
        foreach (KeyValuePair<string, MiniMapNodeMarker> kvp in nodeMarkers) {
            kvp.Value.gameObject.SetActive(kvp.Value.floor == generator.currentFloor);
        }

        foreach (MapMarkerData data in mapDisplay3Dgenerator.mapData) {
            if (data.markerType == MapMarkerData.MapMarkerType.insertionPoint) continue;
            if (data.markerType == MapMarkerData.MapMarkerType.extractionPoint && data.idn != plan.extractionPointIdn) continue;

            if (!indicators.ContainsKey(data)) {
                MapMarkerIndicator indicator = SpawnNewMapMarker();

                string indicatorText = data.markerType == MapMarkerData.MapMarkerType.extractionPoint ? "extraction point" : data.markerName;

                indicator.Configure(indicatorText, data.markerType, data.markerIcon);
                indicators[data] = indicator;
            }
            indicators[data].SetPosition(data.worldPosition, mapDisplay3Dgenerator, markerContainer);
        }
        List<MapMarkerData> keysToRemove = new List<MapMarkerData>();
        foreach (MapMarkerData key in indicators.Keys) {
            if (!mapDisplay3Dgenerator.mapData.Contains(key)) {
                keysToRemove.Add(key);
            }
        }
        foreach (MapMarkerData key in keysToRemove) {
            RemoveMarker(key);
        }

        foreach (KeyValuePair<string, MarkerConfiguration> kvp in mapDisplay3Dgenerator.nodeData) {
            if (!nodeMarkers.ContainsKey(kvp.Key)) {
                MiniMapNodeMarker marker = SpawnNodeMarker();
                int floor = template.GetFloorForPosition(kvp.Value.worldPosition);
                marker.Configure(kvp.Value, generator, floor);
                nodeMarkers[kvp.Key] = marker;
            }
            nodeMarkers[kvp.Key].SetPosition(mapDisplay3Dgenerator, markerContainer);
        }
        List<string> nodeKeysToRemove = new List<string>();
        foreach (string key in nodeMarkers.Keys) {
            if (!mapDisplay3Dgenerator.nodeData.ContainsKey(key)) {
                nodeKeysToRemove.Add(key);
            }
        }
        foreach (string key in nodeKeysToRemove) {
            RemoveNodeMarker(key);
        }

        if (playerMarker == null) {
            playerMarker = SpawnNewMapMarker();
            playerMarker.Configure("", MapMarkerData.MapMarkerType.guard, MapMarkerData.MapMarkerIcon.circle);
        }
        playerMarker.SetPosition(GameManager.I.playerPosition, mapDisplay3Dgenerator, markerContainer);

        statsView.text = mapDisplay3Dgenerator.GetStatsString();

        HandleLegendMarkers();
    }


    // TODO: combine this with handle value changed- reflect loaded graph nodes.
    void DisplayGraph<T, W>(Graph<T, W> graph) where T : Node<T> where W : Graph<T, W> {
        foreach (T node in graph.nodes.Values) {
            int floor = template.GetFloorForPosition(node.position);
            if (floor == mapDisplay3Dgenerator.currentFloor) {

            }
        }
    }


    void HandleLegendMarkers() {
        legendMarkersIndicator.color = legendInactiveColor;
        legendCyberIndicator.color = legendInactiveColor;
        legendPowerIndicator.color = legendInactiveColor;
        legendAlarmIndicator.color = legendInactiveColor;

        switch (mapDisplay3Dgenerator.legendType) {
            case MapDisplayController.MapDisplayLegendType.markers:
                legendMarkersIndicator.color = legendActiveColor;
                break;
            case MapDisplayController.MapDisplayLegendType.alarm:
                legendAlarmIndicator.color = legendActiveColor;
                break;
            case MapDisplayController.MapDisplayLegendType.cyber:
                legendCyberIndicator.color = legendActiveColor;
                break;
            case MapDisplayController.MapDisplayLegendType.power:
                legendPowerIndicator.color = legendActiveColor;
                break;
        }
    }
}
