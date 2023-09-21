using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
class ParallaxLayer {
    public List<RectTransform> rectTransforms;
    public float xFactor;
    public ParallaxLayer(GameObject rootObject) {

        RectTransform rootRect = rootObject.GetComponent<RectTransform>();
        Image rootImage = rootObject.GetComponent<Image>();

        GameObject secondObject = new GameObject();
        RectTransform secondRect = secondObject.AddComponent<RectTransform>();
        secondRect.sizeDelta = new Vector2(rootRect.rect.width, rootRect.rect.height);
        secondRect.SetParent(rootRect.parent, false);
        secondRect.SetSiblingIndex(rootRect.GetSiblingIndex());

        secondRect.localPosition = new Vector2(rootRect.rect.width, 0);
        Image secondImage = secondObject.AddComponent<Image>();
        secondImage.sprite = rootImage.sprite;

        rectTransforms = new List<RectTransform>();
        rectTransforms.Add(rootRect);
        rectTransforms.Add(secondRect);
    }

    public void Update(float timedelta) {
        foreach (RectTransform rectTransform in rectTransforms) {
            Vector3 currentPosition = rectTransform.localPosition;
            rectTransform.localPosition = new Vector2(currentPosition.x - (timedelta * xFactor), currentPosition.y);
            if (rectTransform.localPosition.x <= -1f * rectTransform.rect.width) {
                rectTransform.localPosition = new Vector2(rectTransform.rect.width, 0);
            }
        }
    }
}

public class SubwayAnimator : MonoBehaviour {
    public GameObject[] parallaxGameObjects;
    public Transform foregroundCar;
    List<ParallaxLayer> parallaxLayers;
    void Start() {
        parallaxLayers = new List<ParallaxLayer>();
        foreach (GameObject parallaxObject in parallaxGameObjects) {
            ParallaxLayer layer = new ParallaxLayer(parallaxObject);
            parallaxLayers.Add(layer);
        }
        parallaxLayers[0].xFactor = 180;
        parallaxLayers[1].xFactor = 350;

        StartCoroutine(BumpTheTrain());

    }
    void Update() {
        foreach (ParallaxLayer layer in parallaxLayers) {
            layer.Update(Time.deltaTime);
        }
    }
    IEnumerator BumpTheTrain() {
        Vector3 initialPosition = foregroundCar.localPosition;
        float interval = 1f;
        float dutyCycle = 0.1f;
        float bumpSpacing = 0.25f;
        int bumpCount = 2;
        while (true) {
            int bumpIndex = 0;
            float timer = 0;
            while (timer < interval) {
                timer += Time.deltaTime;
                yield return null;
            }
            while (bumpIndex < bumpCount) {
                timer = 0f;
                foregroundCar.localPosition = new Vector3(initialPosition.x, initialPosition.y - 10f, initialPosition.z);
                while (timer < dutyCycle) {
                    timer += Time.deltaTime;
                    yield return null;
                }
                timer = 0f;
                foregroundCar.localPosition = initialPosition;
                while (timer < bumpSpacing) {
                    timer += Time.deltaTime;
                    yield return null;
                }
                bumpIndex += 1;
            }
        }


    }
}
