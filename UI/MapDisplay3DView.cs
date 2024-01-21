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
    public RectTransform mapRect;
    Dictionary<MapMarkerData, MapMarkerIndicator> indicators;
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

        for (int i = floorPipContainer.childCount - 1; i >= 0; i--) {
            Transform pipChild = floorPipContainer.GetChild(i);
            Image pip = pipChild.GetComponent<Image>();
            floorPips.Add(pip);
        }

        mapDisplay3Dgenerator.Initialize(levelState.template);
        Bind(mapDisplay3Dgenerator.gameObject);
    }
    MapMarkerIndicator SpawnNewMapMarker() {
        GameObject objM = GameObject.Instantiate(mapMarkerPrefab);
        objM.transform.SetParent(markerContainer, false);
        MapMarkerIndicator indicator = objM.GetComponent<MapMarkerIndicator>();

        // // why is this required?
        // foreach (MonoBehaviour component in objM.GetComponentsInChildren<MonoBehaviour>()) {
        //     component.enabled = true;
        // }
        return indicator;
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
            // if (kvp.Key.floorNumber == floor && (kvp.Value == extractionIndicator || kvp.Value == insertionIndicator)) {
            //     kvp.Value.ShowSelection(true);
            // }
        }

        foreach (MapMarkerData data in mapDisplay3Dgenerator.mapData) {
            if (data.markerType == MapMarkerData.MapMarkerType.insertionPoint) continue;
            if (!indicators.ContainsKey(data)) {
                MapMarkerIndicator indicator = SpawnNewMapMarker();
                indicator.Configure(data, ignoreInsertion: true);
                indicators[data] = indicator;
            }
            indicators[data].SetPosition(data.worldPosition, data.floorNumber, mapDisplay3Dgenerator, markerContainer);
        }
        if (playerMarker == null) {
            playerMarker = SpawnNewMapMarker();
            playerMarker.Configure("", MapMarkerData.MapMarkerType.guard, MapMarkerData.MapMarkerIcon.circle);
        }
        playerMarker.SetPosition(GameManager.I.playerPosition, 0, mapDisplay3Dgenerator, markerContainer);

        statsView.text = mapDisplay3Dgenerator.GetStatsString();
    }
}
