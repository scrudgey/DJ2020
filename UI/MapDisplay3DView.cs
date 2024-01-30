using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MapDisplay3DView : IBinder<MapDisplay3DGenerator> {
    enum Mode { plan, mission }
    Mode mode;
    public MapDisplay3DGenerator mapDisplay3Dgenerator;
    public MapDisplayController mapDisplayController;
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
    public Dictionary<Objective, MapMarkerIndicator> objectiveIndicators;
    Dictionary<string, ObjectiveLootSpawnpoint> objectiveLocations;
    MapMarkerIndicator playerMarker;
    List<Image> floorPips;
    int numberFloors;
    LevelTemplate template;
    LevelPlan plan;
    LevelState state;
    MapMarkerIndicator selectedIndicator;

    public void Initialize(LevelState state) {
        this.state = state;
        Initialize(state.template, state.plan);
        mapDisplay3Dgenerator.Initialize(state);
        mode = Mode.mission;
    }
    public void Initialize(LevelTemplate template, LevelPlan plan) {
        mode = Mode.plan;
        this.template = template;
        this.plan = plan;
        floorPips = new List<Image>();

        foreach (Transform child in markerContainer) {
            if (child.name == "mapview") continue;
            Destroy(child.gameObject);
        }
        indicators = new Dictionary<MapMarkerData, MapMarkerIndicator>();
        nodeMarkers = new Dictionary<string, MiniMapNodeMarker>();
        objectiveIndicators = new Dictionary<Objective, MapMarkerIndicator>();
        objectiveLocations = GameObject.FindObjectsOfType<ObjectiveLootSpawnpoint>().ToDictionary(s => s.idn, s => s);

        for (int i = floorPipContainer.childCount - 1; i >= 0; i--) {
            Transform pipChild = floorPipContainer.GetChild(i);
            Image pip = pipChild.GetComponent<Image>();
            floorPips.Add(pip);
        }

        mapDisplay3Dgenerator.Initialize(template, plan); // TODO: broken network graphs
        Bind(mapDisplay3Dgenerator.gameObject);

        mapDisplayController.Initialize(template, plan, this);

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
            kvp.Value.gameObject.SetActive(kvp.Value.floor == generator.currentFloor && kvp.Value.nodeVisibility == NodeVisibility.known);
        }

        foreach (MapMarkerData data in mapDisplay3Dgenerator.mapData) {
            if (data.markerType == MapMarkerData.MapMarkerType.insertionPoint && mode == Mode.mission) continue;
            if (data.markerType == MapMarkerData.MapMarkerType.extractionPoint && data.idn != plan.extractionPointIdn) {
                if (indicators.ContainsKey(data))
                    indicators[data].gameObject.SetActive(false);
                continue;
            }
            if (!indicators.ContainsKey(data)) {
                MapMarkerIndicator indicator = SpawnNewMapMarker();
                string indicatorText = data.markerType switch {
                    MapMarkerData.MapMarkerType.extractionPoint => "extraction point",
                    MapMarkerData.MapMarkerType.insertionPoint => "insertion point",
                    _ => data.markerName
                };
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


        if (mode == Mode.plan) {
            foreach (Objective objective in template.objectives) {
                if (objective.visibility == Objective.Visibility.unknown && !plan.objectiveLocations.ContainsKey(objective.name)) {
                    if (objectiveIndicators.ContainsKey(objective))
                        objectiveIndicators[objective].gameObject.SetActive(false);
                    continue;
                }

                // if objective is visible at template level, it must have only one position I guess (??)
                if (!objectiveIndicators.ContainsKey(objective)) {
                    MapMarkerIndicator objectiveIndicator = SpawnNewMapMarker();
                    objectiveIndicator.Configure("objective", MapMarkerData.MapMarkerType.extractionPoint, MapMarkerData.MapMarkerIcon.circle);
                    objectiveIndicators[objective] = objectiveIndicator;
                }

                string idn = plan.objectiveLocations.ContainsKey(objective.name) ? plan.objectiveLocations[objective.name] : objective.potentialSpawnPoints[0];
                Vector3 position = objective.SpawnPointLocation(idn);

                objectiveIndicators[objective].SetPosition(position, mapDisplay3Dgenerator, markerContainer);
                objectiveIndicators[objective].gameObject.SetActive(template.GetFloorForPosition(position) == generator.currentFloor);
            }
        } else if (mode == Mode.mission) {
            foreach (ObjectiveDelta delta in state.delta.objectiveDeltas) {
                if (delta.visibility == Objective.Visibility.unknown || delta.status != ObjectiveStatus.inProgress) {
                    if (objectiveIndicators.ContainsKey(delta.template))
                        objectiveIndicators[delta.template].gameObject.SetActive(false);
                    continue;
                }
                if (!objectiveIndicators.ContainsKey(delta.template)) {
                    MapMarkerIndicator objectiveIndicator = SpawnNewMapMarker();
                    objectiveIndicator.Configure("objective", MapMarkerData.MapMarkerType.extractionPoint, MapMarkerData.MapMarkerIcon.circle);
                    objectiveIndicators[delta.template] = objectiveIndicator;
                }
                Vector3 position = delta.GetPosition();
                objectiveIndicators[delta.template].SetPosition(position, mapDisplay3Dgenerator, markerContainer);
                objectiveIndicators[delta.template].gameObject.SetActive(template.GetFloorForPosition(position) == generator.currentFloor);
            }

            if (playerMarker == null) {
                playerMarker = SpawnNewMapMarker();
                playerMarker.Configure("", MapMarkerData.MapMarkerType.guard, MapMarkerData.MapMarkerIcon.circle);
            }
            playerMarker.SetPosition(GameManager.I.playerPosition, mapDisplay3Dgenerator, markerContainer);
            playerMarker.gameObject.SetActive(template.GetFloorForPosition(GameManager.I.playerPosition) == generator.currentFloor);

        }

        statsView.text = mapDisplay3Dgenerator.GetStatsString();

        if (generator.clickedObjective != null) {
            if (objectiveIndicators.ContainsKey(generator.clickedObjective)) {
                MapMarkerIndicator indicator = objectiveIndicators[generator.clickedObjective];
                indicator.ShowClickPulse();
            }
        } else if (generator.clickedMapMarker != null) {
            MapMarkerIndicator indicator = indicators[generator.clickedMapMarker];
            indicator.ShowClickPulse();
            // Toolbox.RandomizeOneShot(audioSource, columnButtonSound);
        }

        if (generator.clickedObjective != null) {
            if (selectedIndicator != null) {
                selectedIndicator.ShowSelection(false);
            }
            if (objectiveIndicators.ContainsKey(generator.clickedObjective)) {
                MapMarkerIndicator indicator = objectiveIndicators[generator.clickedObjective];
                indicator.ShowSelection(true);
                selectedIndicator = indicator;
            }
        } else if (generator.selectedMapMarker != null) {
            if (selectedIndicator != null) {
                selectedIndicator.ShowSelection(false);
            }
            MapMarkerIndicator indicator = indicators[generator.selectedMapMarker];
            indicator.ShowSelection(true);
            selectedIndicator = indicator;
        }

        HandleLegendMarkers();
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
