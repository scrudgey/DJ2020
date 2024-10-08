using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;
using UnityEngine.UI;
public class KeySelectMenu : MonoBehaviour {
    public GameObject keyEntryPrefab;
    public Transform keyEntryContainer;
    public RectTransform rectTransform;
    public AudioSource audioSource;

    [Header("indicator")]
    public RectTransform lockIndicatorRect;
    public Image lockIndicatorImage;
    public GraphIconReference graphIconReference;
    public Sprite successSprite;
    public Sprite failSprite;
    public Color successColor;
    public Color failColor;
    public Color iconColor;
    [Header("sounds")]
    public AudioClip[] showSounds;
    public AudioClip[] hideSounds;
    public AudioClip[] successSounds;
    public AudioClip[] failSounds;
    List<DoorLock> targetLocks;
    Coroutine routine;
    Coroutine indicatorRoutine;
    float initialDistanceFromTarget;
    bool isShown;

    public void Initialize(List<DoorLock> targetLocks) {
        this.targetLocks = targetLocks;
        if (targetLocks.Count == 0) {
            Hide();
            return;
        }
        DoorLock doorLock = targetLocks[0];
        initialDistanceFromTarget = Vector3.Distance(doorLock.transform.position, GameManager.I.playerPosition);
        KeyType desiredKeyType = doorLock.lockType;
        HashSet<KeyData> keys = GameManager.I.gameData.levelState.delta.keys.Where(key => key.type == desiredKeyType).ToHashSet();
        PopulateKeyEntries(desiredKeyType, keys, doorLock);
        if (keys.Count > 0) {
            Toolbox.RandomizeOneShot(audioSource, showSounds);
            Show(keys.Count);
        }
    }
    void PopulateKeyEntries(KeyType type, HashSet<KeyData> keys, DoorLock doorLock) {
        foreach (Transform child in keyEntryContainer) {
            if (child.name == "outline") continue;
            if (child.name == "title") continue;
            Destroy(child.gameObject);
        }
        foreach (KeyData key in keys) {
            bool enabled = !doorLock.attemptedKeys.Contains(key);

            GameObject obj = GameObject.Instantiate(keyEntryPrefab);
            KeyEntry keyEntry = obj.GetComponent<KeyEntry>();
            keyEntry.Configure(ButtonCallback, key, enabled: enabled);
            obj.transform.SetParent(keyEntryContainer, false);
        }
    }
    void Update() {
        if (isShown && targetLocks.Count > 0) {
            DoorLock doorLock = targetLocks[0];
            float distance = Vector3.Distance(doorLock.transform.position, GameManager.I.playerPosition);
            if (distance / initialDistanceFromTarget > 1.25f) Hide();
        }
    }
    public void ButtonCallback(KeyData keyData) {
        bool success = false;
        targetLocks.ForEach((doorlock) => { success |= doorlock.TryKeyUnlock(keyData); });
        if (success) {
            Toolbox.RandomizeOneShot(audioSource, successSounds);
        } else {
            Toolbox.RandomizeOneShot(audioSource, failSounds);
            targetLocks.ForEach((doorlock) => { doorlock.attemptedKeys.Add(keyData); });
        }
        if (indicatorRoutine != null) StopCoroutine(indicatorRoutine);
        indicatorRoutine = StartCoroutine(BlinkIcon(keyData, success));
        ItemUseResult itemUseResult = ItemUseResult.Empty() with {
            waveArm = true
        };
        GameManager.I.playerCharacterController.HandleItemUseResult(itemUseResult);
        Hide();
        Toolbox.RandomizeOneShot(audioSource, hideSounds);
        CutsceneManager.I.HandleTrigger("key_menu_used");
    }
    public void Show(int numberEntries) {
        if (routine != null) {
            StopCoroutine(routine);
        }
        float targetHeight = numberEntries * 40f + 25;
        routine = StartCoroutine(Toolbox.Ease(null, 0.1f, 0f, targetHeight, PennerDoubleAnimation.ExpoEaseOut, (amount) => {
            rectTransform.sizeDelta = new Vector2(175, amount);
        }));
        isShown = true;
    }
    public void Hide() {
        if (routine != null) {
            StopCoroutine(routine);
        }
        float initialHeight = rectTransform.rect.height;
        routine = StartCoroutine(Toolbox.Ease(null, 0.1f, initialHeight, 0, PennerDoubleAnimation.ExpoEaseIn, (amount) => {
            rectTransform.sizeDelta = new Vector2(175, amount);
        }));
        isShown = false;
    }

    IEnumerator BlinkIcon(KeyData data, bool isSuccess) {
        lockIndicatorRect.gameObject.SetActive(true);

        Vector3 indicatorPosition = GameManager.I.playerPosition + 1.2f * Vector3.up;

        bool parity = false;
        float timer = 0f;
        float duration = 0.075f;
        int cycles = 6;
        int cycleIndex = 0;

        // SetIndicatorKeyIcon(data);
        SetIndicatorFeedback(isSuccess);


        while (cycleIndex < cycles) {
            timer += Time.unscaledDeltaTime;
            if (timer > duration) {
                timer = 0f;
                cycleIndex += 1;
                parity = !parity;
                if (parity) {
                    SetIndicatorKeyIcon(data);
                } else {
                    SetIndicatorFeedback(isSuccess);
                }
            }
            lockIndicatorRect.position = GameManager.I.characterCamera.Camera.WorldToScreenPoint(indicatorPosition);
            yield return null;
        }
        yield return Toolbox.BlinkEmphasis(lockIndicatorImage, duration: 0.05f);
        yield return new WaitForSecondsRealtime(0.3f);
        lockIndicatorRect.gameObject.SetActive(false);
    }

    void SetIndicatorKeyIcon(KeyData keyData) {
        lockIndicatorImage.sprite = graphIconReference.KeyinfoSprite(keyData);
        lockIndicatorImage.color = iconColor;
    }
    void SetIndicatorFeedback(bool success) {
        if (success) {
            lockIndicatorImage.sprite = successSprite;
            lockIndicatorImage.color = successColor;
        } else {
            lockIndicatorImage.sprite = failSprite;
            lockIndicatorImage.color = failColor;
        }
    }
}
