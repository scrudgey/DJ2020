using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MapDisplay3DView : IBinder<MapDisplay3DGenerator> {
    enum Mode { plan, mission }
    Mode mode;
    public MapDisplay3DGenerator mapDisplay3Dgenerator;
    public MapDisplayController mapDisplayController;
    public AudioSource audioSource;
    [Header("floor indicators")]
    public Transform floorPipContainer;
    public TextMeshProUGUI floorNumberCaption;
    public TextMeshProUGUI statsView;
    public Color selectedColor;
    public Color deselectedColor;
    [Header("buttons")]
    public Button cyberButton;
    public Button powerButton;
    public Button alarmButton;
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
    [Header("map info")]
    public GraphIconReference graphIconReference;
    public TextMeshProUGUI nodeNumberText;
    public Image nodeNumberBox;
    public GameObject nodeNumberBoxObject;
    public Dictionary<Objective, MapMarkerIndicator> objectiveIndicators;

    [Header("flavor")]
    public RectTransform selectionBox;
    public Image selectionBoxImage;
    public TextMeshProUGUI flavorText;
    public RawImage mapRawImage;
    [Header("sfx")]
    public AudioClip[] floorChangeSound;

    Coroutine selectionBoxCorotuine;
    Coroutine floorBlitCoroutine;
    Coroutine mapShakeCoroutine;
    Coroutine flavorTextBlitCoroutine;

    Dictionary<MapMarkerData, MapMarkerIndicator> indicators;
    Dictionary<string, MiniMapNodeMarker> nodeMarkers;

    int selectedFloor;
    Dictionary<string, ObjectiveLootSpawnpoint> objectiveLocations;
    MapMarkerIndicator playerMarker;
    List<Image> floorPips;
    int numberFloors;
    // LevelTemplate template;
    SceneData sceneData;
    LevelPlan plan;
    LevelState state;
    MapMarkerIndicator selectedIndicator;

    List<Objective> allObjectives = new List<Objective>();
    List<ObjectiveDelta> allObjectiveDeltas = new List<ObjectiveDelta>();

    public void InitializeWorldMode(SceneData sceneData) {
        mapDisplay3Dgenerator.Initialize(sceneData);
        mapDisplayController.InitializeWorld(sceneData, this);
        Initialize(sceneData);
        cyberButton.interactable = false;
        powerButton.interactable = false;
        alarmButton.interactable = false;
    }

    public void Initialize(LevelState state, SceneData sceneData) {
        this.state = state;
        allObjectiveDeltas = state.delta.AllObjectives();
        Initialize(state.template, state.plan, sceneData);
        mapDisplay3Dgenerator.Initialize(state, sceneData);
        mode = Mode.mission;
        mapDisplayController.HideInsertionPoints();
    }
    public void Initialize(LevelTemplate template, LevelPlan plan, SceneData sceneData) {
        this.plan = plan;
        allObjectives = template.AllObjectives();
        mapDisplay3Dgenerator.Initialize(template, sceneData, plan);
        mapDisplayController.Initialize(template, sceneData, plan, this);
        Initialize(sceneData);
        mode = Mode.plan;
    }

    public void Initialize(SceneData sceneData) {
        mode = Mode.mission;
        this.sceneData = sceneData;
        floorPips = new List<Image>();
        selectionBoxImage.enabled = false;

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

        Bind(mapDisplay3Dgenerator.gameObject);
        selectedFloor = mapDisplay3Dgenerator.currentFloor;
        floorNumberCaption.text = $"{sceneData.sceneDescriptor}: floor {mapDisplay3Dgenerator.currentFloor}";
        flavorText.text = "";
        mapRawImage.uvRect = new Rect(0, 0, 1, 1);
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
            if (i >= generator.numberFloors) {
                floorPip.enabled = false;
            }
            if (i == generator.currentFloor) {
                floorPip.color = selectedColor;
            } else {
                floorPip.color = deselectedColor;
            }
        }
        if (selectedFloor != generator.currentFloor) {
            float amount = generator.currentFloor > selectedFloor ? -1f : 1f;
            selectedFloor = generator.currentFloor;
            BlitFloorText($"{sceneData.sceneDescriptor}: floor {generator.currentFloor}");
            ShakeMp(amount);
            Toolbox.RandomizeOneShot(audioSource, floorChangeSound);
        }

        foreach (KeyValuePair<MapMarkerData, MapMarkerIndicator> kvp in indicators) {
            kvp.Value.gameObject.SetActive(kvp.Key.floorNumber == generator.currentFloor);
        }
        foreach (KeyValuePair<string, MiniMapNodeMarker> kvp in nodeMarkers) {
            kvp.Value.gameObject.SetActive(kvp.Value.floor == generator.currentFloor && kvp.Value.nodeVisibility == NodeVisibility.known);
        }

        foreach (MapMarkerData data in mapDisplay3Dgenerator.mapData) {
            if (data.markerType == MapMarkerData.MapMarkerType.objective || data.markerType == MapMarkerData.MapMarkerType.anchor) continue;
            if (data.markerType == MapMarkerData.MapMarkerType.insertionPoint && mode == Mode.mission) continue;
            if (plan != null) {
                if (data.markerType == MapMarkerData.MapMarkerType.extractionPoint && data.idn != plan.extractionPointIdn) {
                    if (indicators.ContainsKey(data))
                        indicators[data].gameObject.SetActive(false);
                    continue;
                }
                if (data.markerType == MapMarkerData.MapMarkerType.insertionPoint && data.idn != plan.insertionPointIdn) {
                    if (indicators.ContainsKey(data))
                        indicators[data].gameObject.SetActive(false);
                    continue;
                }
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
                int floor = sceneData.GetMapFloorForPosition(kvp.Value.worldPosition);
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
            foreach (Objective objective in allObjectives) {
                if (objective.visibility == Objective.Visibility.unknown && !plan.objectiveLocations.ContainsKey(objective.name)) {
                    if (objectiveIndicators.ContainsKey(objective))
                        objectiveIndicators[objective].gameObject.SetActive(false);
                    continue;
                }

                // if objective is visible at template level, it must have only one position I guess (??)
                if (!objectiveIndicators.ContainsKey(objective)) {
                    MapMarkerIndicator objectiveIndicator = SpawnNewMapMarker();
                    objectiveIndicator.Configure("objective", MapMarkerData.MapMarkerType.objective, MapMarkerData.MapMarkerIcon.circle);
                    objectiveIndicators[objective] = objectiveIndicator;
                }

                string idn = plan.objectiveLocations.ContainsKey(objective.name) ? plan.objectiveLocations[objective.name] : objective.potentialSpawnPoints[0];
                Vector3 position = objective.SpawnPointLocation(idn);

                objectiveIndicators[objective].SetPosition(position, mapDisplay3Dgenerator, markerContainer);
                objectiveIndicators[objective].gameObject.SetActive(sceneData.GetMapFloorForPosition(position) == generator.currentFloor);
            }
        } else if (mode == Mode.mission) {
            foreach (ObjectiveDelta delta in allObjectiveDeltas) {
                if (!delta.hasLocation) continue;
                if (delta.visibility == Objective.Visibility.unknown || delta.status != ObjectiveStatus.inProgress) {
                    if (objectiveIndicators.ContainsKey(delta.template))
                        objectiveIndicators[delta.template].gameObject.SetActive(false);
                    continue;
                }
                if (!objectiveIndicators.ContainsKey(delta.template)) {
                    MapMarkerIndicator objectiveIndicator = SpawnNewMapMarker();
                    objectiveIndicator.Configure("objective", MapMarkerData.MapMarkerType.objective, MapMarkerData.MapMarkerIcon.circle);
                    objectiveIndicators[delta.template] = objectiveIndicator;
                }
                Vector3 position = delta.GetPosition();
                objectiveIndicators[delta.template].SetPosition(position, mapDisplay3Dgenerator, markerContainer);
                objectiveIndicators[delta.template].gameObject.SetActive(sceneData.GetMapFloorForPosition(position) == generator.currentFloor);
            }

            if (playerMarker == null) {
                playerMarker = SpawnNewMapMarker();
                playerMarker.Configure("", MapMarkerData.MapMarkerType.guard, MapMarkerData.MapMarkerIcon.circle);
            }
            playerMarker.SetPosition(GameManager.I.playerPosition, mapDisplay3Dgenerator, markerContainer);
            playerMarker.gameObject.SetActive(sceneData.GetMapFloorForPosition(GameManager.I.playerPosition) == generator.currentFloor);
        }

        statsView.text = mapDisplay3Dgenerator.GetStatsString();


        if (generator.clickedObjective != null) {
            if (objectiveIndicators.ContainsKey(generator.clickedObjective)) {
                MapMarkerIndicator indicator = objectiveIndicators[generator.clickedObjective];
                ShowSelectionBox(indicator);
                BlitFlavorText(generator.clickedObjective.ToString());
            }
        } else if (generator.clickedMapMarker != null) {
            if (indicators.ContainsKey(generator.clickedMapMarker)) {
                MapMarkerIndicator indicator = indicators[generator.clickedMapMarker];
                ShowSelectionBox(indicator);
                BlitFlavorText(generator.clickedMapMarker.ToFlavorText());
            }
        }

        if (generator.selectedObjective != null) {
            if (selectedIndicator != null) {
                selectedIndicator.ShowSelection(false);
            }
            if (objectiveIndicators.ContainsKey(generator.selectedObjective)) {
                MapMarkerIndicator indicator = objectiveIndicators[generator.selectedObjective];
                indicator.ShowSelection(true);
                selectedIndicator = indicator;
                selectionBox.position = indicator.rectTransform.position;
            }
        } else if (generator.selectedMapMarker != null && generator.selectedMapMarker.idn != "") {
            if (selectedIndicator != null) {
                selectedIndicator.ShowSelection(false);
            }
            MapMarkerIndicator indicator = indicators[generator.selectedMapMarker];
            indicator.ShowSelection(true);
            selectedIndicator = indicator;
            selectionBox.position = indicator.rectTransform.position;
        }

        if (generator.currentGraphTitle != "") {
            nodeNumberBoxObject.SetActive(true);
            nodeNumberText.text = $"{generator.currentGraphTitle}: {generator.numberDiscoveredNodes}/{generator.numberTotalNodes}";
            Color color = generator.currentGraphTitle switch {
                "cyber" => graphIconReference.minimapCyberColor,
                "power" => graphIconReference.minimapPowerColor,
                "alarm" => graphIconReference.minimapAlarmColor,
                _ => graphIconReference.minimapCyberColor
            };
            nodeNumberText.color = color;
            nodeNumberBox.color = color;
        } else {
            nodeNumberBoxObject.SetActive(false);
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

    void ShowSelectionBox(MapMarkerIndicator indicator) {
        if (selectionBoxCorotuine != null) {
            StopCoroutine(selectionBoxCorotuine);
        }
        selectionBoxCorotuine = StartCoroutine(ActivateSelectionBox(indicator));
    }
    void BlitFloorText(string text) {
        if (!gameObject.activeInHierarchy) return;
        if (floorBlitCoroutine != null) {
            StopCoroutine(floorBlitCoroutine);
        }
        floorBlitCoroutine = StartCoroutine(Toolbox.BlitText(floorNumberCaption, text, interval: 0.01f));
    }
    void BlitFlavorText(string text) {
        if (flavorTextBlitCoroutine != null) {
            StopCoroutine(flavorTextBlitCoroutine);
        }
        flavorTextBlitCoroutine = StartCoroutine(Toolbox.BlitText(flavorText, text));
    }
    void ShakeMp(float amount) {
        if (mapShakeCoroutine != null) {
            StopCoroutine(mapShakeCoroutine);
        }
        mapShakeCoroutine = StartCoroutine(ActivateMapShake(amount));
    }
    IEnumerator ActivateSelectionBox(MapMarkerIndicator indicator) {
        WaitForSecondsRealtime briefPause = new WaitForSecondsRealtime(0.025f);
        WaitForSecondsRealtime longerPause = new WaitForSecondsRealtime(0.1f);
        selectionBoxImage.enabled = true;


        selectionBox.sizeDelta = new Vector2(200f, 200f);
        selectionBoxImage.color = Color.white;
        yield return briefPause;
        selectionBoxImage.color = graphIconReference.minimapCyberColor;
        yield return longerPause;

        yield return Toolbox.BlinkVis(selectionBoxImage, () => {
            selectionBox.sizeDelta = new Vector2(150f, 150f);
            selectionBoxImage.color = Color.white;
        });
        yield return briefPause;
        selectionBoxImage.color = graphIconReference.minimapCyberColor;
        yield return longerPause;

        yield return Toolbox.BlinkVis(selectionBoxImage, () => {
            selectionBox.sizeDelta = new Vector2(100f, 100f);
            selectionBoxImage.color = Color.white;
        });
        yield return briefPause;
        selectionBoxImage.color = graphIconReference.minimapCyberColor;
        yield return longerPause;


        yield return Toolbox.BlinkEmphasis(selectionBoxImage);
        yield return new WaitForSecondsRealtime(2.5f);
        selectionBoxImage.enabled = false;

        // selectionBox.gameObject.SetActive(false);
    }

    IEnumerator ActivateMapShake(float amount) {
        Rect rect = mapRawImage.uvRect;
        yield return Toolbox.Ease(null, 0.1f, amount * -1, 0, PennerDoubleAnimation.BackEaseOut, (amount) => {
            mapRawImage.uvRect = new Rect(0, amount, 1, 1);
        }, unscaledTime: true);
    }
}
