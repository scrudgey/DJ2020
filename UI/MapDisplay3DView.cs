using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MapDisplay3DView : MonoBehaviour {
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
    bool mouseOverMap;
    bool mapEngaged;
    public enum MouseHeldType { none, left, right }
    MouseHeldType mouseHeldType;

    List<MapMarkerData> mapData;

    int numberFloors;
    LevelTemplate template;
    public void Initialize(LevelTemplate template) {
        this.template = template;
        List<Texture2D> mapImages = MapMarker.LoadMapImages(template.levelName, template.sceneName);
        mapData = MapMarker.LoadMapMetaData(template.levelName, template.sceneName);
        numberFloors = mapImages.Count;

        floorPips = new List<Image>();
        for (int i = floorPipContainer.childCount - 1; i >= 0; i--) {
            Transform pipChild = floorPipContainer.GetChild(i);
            Image pip = pipChild.GetComponent<Image>();
            floorPips.Add(pip);
            if (i > numberFloors) {
                pip.enabled = false;
            }
        }
        InitializeMapMarkers(template);

        mapDisplay3Dgenerator.Initialize(this, template, mapImages);
    }

    public void UpdateWithInput(PlayerInput playerInput) {
        if (playerInput.rightMouseDown && mouseOverMap && !mapEngaged) {
            mapEngaged = true;
            mouseHeldType = MouseHeldType.right;
        } else if (playerInput.mouseDown && mouseOverMap && !mapEngaged) {
            mapEngaged = true;
            mouseHeldType = MouseHeldType.left;
        } else if (!playerInput.mouseDown && !playerInput.rightMouseDown) {
            mapEngaged = false;
            mouseHeldType = MouseHeldType.none;
        }

        if (mouseOverMap || mapEngaged) {
            mapDisplay3Dgenerator.UpdateWithInput(playerInput, Time.unscaledDeltaTime, mouseHeldType);
        }
        foreach (KeyValuePair<MapMarkerData, MapMarkerIndicator> kvp in indicators) {
            kvp.Value.SetPosition(kvp.Key.worldPosition, kvp.Key.floorNumber, template, mapDisplay3Dgenerator, markerContainer);
        }
        playerMarker.SetPosition(GameManager.I.playerPosition, 0, template, mapDisplay3Dgenerator, markerContainer);
        statsView.text = mapDisplay3Dgenerator.GetStatsString();
    }


    public void OnMouseOverMap() {
        mouseOverMap = true;
    }
    public void OnMouseExitMap() {
        mouseOverMap = false;
    }
    public void OnFloorSelected(int fromFloor, int toFloor) {
        floorNumberCaption.text = $"{template.sceneDescriptor}: floor {toFloor}";
        floorPips[fromFloor].color = deselectedColor;
        floorPips[toFloor].color = selectedColor;
        DisplayMapMarkers(toFloor);
    }
    void InitializeMapMarkers(LevelTemplate template) {
        foreach (Transform child in markerContainer) {
            if (child.name == "mapview") continue;
            Destroy(child.gameObject);
        }
        indicators = new Dictionary<MapMarkerData, MapMarkerIndicator>();
        foreach (MapMarkerData data in mapData) {
            GameObject objM = GameObject.Instantiate(mapMarkerPrefab);
            objM.transform.SetParent(markerContainer, false);
            MapMarkerIndicator indicator = objM.GetComponent<MapMarkerIndicator>();
            indicator.Configure(data, template, mapDisplay3Dgenerator, markerContainer);
            indicators[data] = indicator;

            // why is this required?
            foreach (MonoBehaviour component in objM.GetComponentsInChildren<MonoBehaviour>()) {
                component.enabled = true;
            }
        }

        GameObject obj = GameObject.Instantiate(mapMarkerPrefab);
        obj.transform.SetParent(markerContainer, false);
        playerMarker = obj.GetComponent<MapMarkerIndicator>();
        playerMarker.Configure("", MapMarkerData.MapMarkerType.guard, MapMarkerData.MapMarkerIcon.circle);
        foreach (MonoBehaviour component in obj.GetComponentsInChildren<MonoBehaviour>()) {
            component.enabled = true;
        }
    }
    void DisplayMapMarkers(int floor) {
        foreach (KeyValuePair<MapMarkerData, MapMarkerIndicator> kvp in indicators) {
            kvp.Value.gameObject.SetActive(kvp.Key.floorNumber == floor);
            // if (kvp.Key.floorNumber == floor && (kvp.Value == extractionIndicator || kvp.Value == insertionIndicator)) {
            //     kvp.Value.ShowSelection(true);
            // }
        }
    }
}
