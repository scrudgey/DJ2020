using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BurglarCanvasController : MonoBehaviour {
    BurgleTargetData data;
    public RawImage rawImage;
    public GameObject UIElementPrefab;
    public Transform uiElementsContainer;
    public void Initialize(BurgleTargetData data) {
        foreach (Transform child in uiElementsContainer) {
            Destroy(child.gameObject);
        }
        this.data = data;
        data.target.EnableAttackSurface();
        rawImage.texture = data.target.renderTexture;
        rawImage.color = Color.white;
        Debug.Log($"Initialize with {data}");

        RectTransform containerRectTransform = uiElementsContainer.GetComponent<RectTransform>();
        foreach (AttackSurfaceElement element in data.target.GetComponentsInChildren<AttackSurfaceElement>()) {
            Debug.Log($"attack surface element: {element}");
            Rect bounds = Toolbox.GetTotalRenderBoundingBox(element.transform, data.target.attackCam);

            GameObject UIElement = GameObject.Instantiate(UIElementPrefab);
            UIElement.transform.SetParent(uiElementsContainer);

            RectTransform cursorRect = UIElement.GetComponent<RectTransform>();
            cursorRect.anchorMin = Vector2.zero;
            cursorRect.anchorMax = Vector2.zero;
            Image cursorImage = UIElement.GetComponent<Image>();

            Vector3 position = data.target.attackCam.WorldToViewportPoint(element.transform.position);
            position.x *= containerRectTransform.rect.width;
            position.y *= containerRectTransform.rect.height;
            cursorRect.anchoredPosition = position;
            cursorRect.sizeDelta = new Vector2(bounds.width, bounds.height);
            cursorImage.color = Color.red;
        }
    }
    public void TearDown() {
        foreach (Transform child in uiElementsContainer) {
            Destroy(child.gameObject);
        }
        if (data != null)
            data.target.DisableAttackSurface();
    }

    public void DoneButtonCallback() {
        GameManager.I.CloseBurglar();
    }


}
