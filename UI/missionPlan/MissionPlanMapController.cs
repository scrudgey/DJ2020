using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MissionPlanMapController : MonoBehaviour {
    public AudioSource audioSource;
    public AudioClip[] mapButtonSound;
    public AudioClip[] columnButtonSound;
    public ScrollRect scrollRect;
    public RectTransform scrollRectTransform;
    public RawImage mapImage;
    public RectTransform mapRect;
    public List<MapMarkerData> mapData;
    public List<Texture2D> mapImages;
    public GameObject mapMarkerPrefab;
    public TextMeshProUGUI floorText;
    [Header("Containers")]
    public Transform insertionPointContainer;
    public Transform extractionPointContainer;
    public Transform objectivePointContainer;
    public Transform interestPointContainer;
    public GameObject insertionPointButtonPrefab;
    Dictionary<MapMarkerData, MapMarkerIndicator> indicators;

    // Dictionary<MapMarkerIndicator, MapMarkerData> reverseIndicators;
    int selectedFloor;
    Vector2 initialRectSize;
    InsertionPointSelector extractionSelector;
    InsertionPointSelector insertionSelector;
    MapMarkerIndicator extractionIndicator;
    MapMarkerIndicator insertionIndicator;
    LevelTemplate template;
    LevelPlan plan;

    public void Initialize(GameData data, LevelTemplate template, LevelPlan plan) {
        this.template = template;
        this.plan = plan;
        mapData = MapMarker.LoadMapMetaData(template.levelName, template.sceneName);
        mapImages = MapMarker.LoadMapImages(template.levelName, template.sceneName);
        initialRectSize = new Vector2(mapRect.rect.width, mapRect.rect.height);
        InitializeMapMarkers();
        PopulateColumns();
        ChangeFloorView(1);
    }
    void InitializeMapMarkers() {
        foreach (Transform child in mapImage.transform) {
            Destroy(child.gameObject);
        }
        indicators = new Dictionary<MapMarkerData, MapMarkerIndicator>();
        foreach (MapMarkerData data in mapData) {
            GameObject obj = GameObject.Instantiate(mapMarkerPrefab);
            obj.transform.SetParent(mapImage.transform, false);
            MapMarkerIndicator indicator = obj.GetComponent<MapMarkerIndicator>();
            indicator.Configure(data, mapRect);
            indicators[data] = indicator;
            // why is this required?
            foreach (MonoBehaviour component in obj.GetComponentsInChildren<MonoBehaviour>()) {
                component.enabled = true;
            }

        }
    }

    void OnEnable() {
        if (extractionIndicator != null) {
            extractionIndicator.ShowSelection(true);
        }
        if (insertionIndicator != null) {
            insertionIndicator.ShowSelection(true);
        }
    }

    public void ChangeFloorView(int index) {
        index = Mathf.Clamp(index, 0, mapImages.Count - 1);
        selectedFloor = index;
        DisplayMapImage(selectedFloor);
        DisplayMapMarkers(selectedFloor);
        floorText.text = $"{template.sceneDescriptor}\nFloor {index}";
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
        foreach (MapMarkerData data in mapData) {
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
                selector.Configure(this, data);

                if (data.idn == plan.insertionPointIdn) {
                    SelectInsertionPoint(selector, indicators[data]);
                }
                if (data.idn == plan.extractionPointIdn) {
                    SelectExtractionPoint(selector, indicators[data]);
                }

                if (plan.insertionPointIdn == "" && insertionIndicator == null && data.markerType == MapMarkerData.MapMarkerType.insertionPoint) {
                    SelectInsertionPoint(selector, indicators[data]);
                }
                if (plan.extractionPointIdn == "" && extractionIndicator == null && data.markerType == MapMarkerData.MapMarkerType.extractionPoint) {
                    SelectExtractionPoint(selector, indicators[data]);
                }
            }
        }
    }

    public void ColumnItemCallback(InsertionPointSelector selector) {
        Toolbox.RandomizeOneShot(audioSource, columnButtonSound);
        if (selector.data.floorNumber != selectedFloor) {
            ChangeFloorView(selector.data.floorNumber);
        }
        MapMarkerIndicator indicator = indicators[selector.data];
        indicator.ShowClickPulse();
        ScrollTo(indicator.transform);
        if (selector.data.markerType == MapMarkerData.MapMarkerType.insertionPoint) {
            SelectInsertionPoint(selector, indicator);
        } else if (selector.data.markerType == MapMarkerData.MapMarkerType.extractionPoint) {
            SelectExtractionPoint(selector, indicator);
        }
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
        plan.insertionPointIdn = selector.data.idn;
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

        plan.extractionPointIdn = selector.data.idn;
    }
    void ScrollTo(Transform target) {
        Canvas.ForceUpdateCanvases();
        Vector2 newPosition = ((Vector2)scrollRect.transform.InverseTransformPoint(mapRect.position) - (Vector2)scrollRect.transform.InverseTransformPoint(target.position));
        newPosition.y -= (scrollRectTransform.rect.height / 2f);
        mapRect.anchoredPosition = newPosition;
    }
    public void DisplayMapImage(int floor) {
        mapImage.texture = mapImages[floor];
    }

    public void DisplayMapMarkers(int floor) {
        foreach (KeyValuePair<MapMarkerData, MapMarkerIndicator> kvp in indicators) {
            kvp.Value.gameObject.SetActive(kvp.Key.floorNumber == floor);
            if (kvp.Key.floorNumber == floor && (kvp.Value == extractionIndicator || kvp.Value == insertionIndicator)) {
                kvp.Value.ShowSelection(true);
            }
        }
    }

    public void UpFloorCallback() {
        Toolbox.RandomizeOneShot(audioSource, mapButtonSound);
        ChangeFloorView(selectedFloor + 1);
    }
    public void DownFloorCallback() {
        Toolbox.RandomizeOneShot(audioSource, mapButtonSound);
        ChangeFloorView(selectedFloor - 1);
    }
    public void ZoomInCallback() {
        Toolbox.RandomizeOneShot(audioSource, mapButtonSound);
        Vector2 finalScale = 1.1f * mapRect.localScale;
        finalScale.x = Mathf.Clamp(finalScale.x, 0.5f, 2f);
        finalScale.y = Mathf.Clamp(finalScale.y, 0.5f, 2f);
        mapRect.localScale = finalScale;
    }
    public void ZoomOutCallback() {
        Toolbox.RandomizeOneShot(audioSource, mapButtonSound);
        Vector2 finalScale = 0.9f * mapRect.localScale;
        finalScale.x = Mathf.Clamp(finalScale.x, 0.5f, 2f);
        finalScale.y = Mathf.Clamp(finalScale.y, 0.5f, 2f);
        mapRect.localScale = finalScale;
    }
}
