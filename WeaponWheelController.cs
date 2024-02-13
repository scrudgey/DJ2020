using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class WeaponWheelController : MonoBehaviour {
    public RectTransform mainRect;
    public AudioSource audioSource;
    public AudioClip[] revealSound;
    public AudioClip[] mouseOverSound;
    public bool revealed;
    public GameObject wheelObject;
    Vector2 cursorPosition;
    public RectTransform mouseCursor;
    public TextMeshProUGUI selectorText;
    public Transform optionContainer;
    public GameObject weaponWheelOptionPrefab;
    public List<WeaponWheelOption> wheelOptions;
    Coroutine dialogueCoroutine;
    WeaponWheelOption selectedOption;
    WeaponWheelOption previousSelectedOption;
    float thetaDelta;
    public void Initialize() {
        wheelOptions = new List<WeaponWheelOption>();
        foreach (Transform child in optionContainer) {
            Destroy(child.gameObject);
        }
        List<WeaponState> guns = new List<WeaponState>();
        if (GameManager.I.gameData.playerState.primaryGun != null) {
            guns.Add(GameManager.I.gameData.playerState.primaryGun);
        }
        if (GameManager.I.gameData.playerState.secondaryGun != null) {
            guns.Add(GameManager.I.gameData.playerState.secondaryGun);
        }
        if (GameManager.I.gameData.playerState.tertiaryGun != null && GameManager.I.gameData.playerState.PerkThirdWeaponSlot()) {
            guns.Add(GameManager.I.gameData.playerState.tertiaryGun);
        }
        thetaDelta = 2f * Mathf.PI / (guns.Count + GameManager.I.gameData.levelState.plan.items.Where(item => item != null).Count() + 1);
        float theta = 0f;
        foreach (WeaponState gun in guns) {
            WeaponWheelOption option = CreateSelectorOption(theta, gun);
            theta += thetaDelta;
        }
        foreach (ItemTemplate item in GameManager.I.gameData.levelState.plan.items) {
            if (item == null) continue;
            WeaponWheelOption option = CreateSelectorOption(theta, item);
            theta += thetaDelta;
        }
        CreateHolsterOption(theta);
    }
    WeaponWheelOption CreateSelectorOption(float theta, WeaponState gun) {
        WeaponWheelOption weaponWheelOption = SpawnOption(theta);
        weaponWheelOption.Initialize(gun);
        return weaponWheelOption;
    }
    WeaponWheelOption CreateSelectorOption(float theta, ItemTemplate item) {
        WeaponWheelOption weaponWheelOption = SpawnOption(theta);
        weaponWheelOption.Initialize(item);
        return weaponWheelOption;
    }
    WeaponWheelOption CreateHolsterOption(float theta) {
        WeaponWheelOption weaponWheelOption = SpawnOption(theta);
        weaponWheelOption.InitializeHolster();
        return weaponWheelOption;
    }
    WeaponWheelOption SpawnOption(float theta) {
        GameObject selector = GameObject.Instantiate(weaponWheelOptionPrefab);
        selector.transform.SetParent(optionContainer, false);
        RectTransform selectorRect = selector.GetComponent<RectTransform>();
        Vector2 position = 155f * new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
        selectorRect.anchoredPosition = position;
        WeaponWheelOption weaponWheelOption = selector.GetComponent<WeaponWheelOption>();
        wheelOptions.Add(weaponWheelOption);
        return weaponWheelOption;
    }
    public void UpdateWithPlayerInput(ref PlayerInput input) {
        if (input.revealWeaponWheel != revealed) {
            revealed = input.revealWeaponWheel;
            wheelObject.SetActive(revealed);
            if (!revealed) {
                CloseMenu(ref input);
                Cursor.lockState = CursorLockMode.None;
            } else {
                Reveal();
            }
        }
        if (revealed) {
            selectorText.text = "";
            UpdateCursor(input);
        }
    }

    void Reveal() {
        Toolbox.RandomizeOneShot(audioSource, revealSound);
        dialogueCoroutine = StartCoroutine(Toolbox.ChainCoroutines(
            Toolbox.Ease(null, 0.25f, 0f, 1, PennerDoubleAnimation.ExpoEaseOut, (float height) => {
                mainRect.localScale = height * Vector2.one;
            }, unscaledTime: true),
            Toolbox.CoroutineFunc(() => {
                dialogueCoroutine = null;
            })
        ));
        ResetCursor();
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void HideWheel() {
        revealed = false;
        wheelObject.SetActive(false);
    }
    void ResetCursor() {
        cursorPosition = Vector2.zero;
    }
    void CloseMenu(ref PlayerInput input) {
        if (dialogueCoroutine != null) StopCoroutine(dialogueCoroutine);
        // Debug.Log($"closing menu with theta {selectedOption}");
        if (selectedOption == null) {
            return;
        } else if (selectedOption.weapon == GameManager.I.gameData.playerState.primaryGun) {
            input.selectgun = 1;
        } else if (selectedOption.weapon == GameManager.I.gameData.playerState.secondaryGun) {
            input.selectgun = 2;
        } else if (selectedOption.weapon == GameManager.I.gameData.playerState.tertiaryGun) {
            input.selectgun = 3;
        } else if (selectedOption.item != null) {
            input.selectItem = selectedOption.item;
        } else if (selectedOption.holster) {
            input.selectgun = -1;
        }
    }
    void UpdateCursor(PlayerInput input) {
        cursorPosition += input.mouseDelta * 5f;
        cursorPosition = Vector2.ClampMagnitude(cursorPosition, 220f);
        mouseCursor.anchoredPosition = cursorPosition;

        float magnitude = cursorPosition.magnitude;
        if (magnitude < 100f) {
            selectedOption = null;
            selectorText.text = "";
        } else {
            float theta = Mathf.Atan(cursorPosition.y / cursorPosition.x);
            if (cursorPosition.x < 0) {
                theta += Mathf.PI;
            } else if (cursorPosition.x > 0 && cursorPosition.y < 0) {
                theta += 2f * Mathf.PI;
            }
            int index = ThetaToIndex(theta);
            selectedOption = wheelOptions[index];
            selectorText.text = selectedOption.optionName;
        }

        foreach (WeaponWheelOption option in wheelOptions) {
            option.HandleMouseOver(option == selectedOption);
        }
        if (previousSelectedOption != selectedOption && selectedOption != null) {
            Toolbox.RandomizeOneShot(audioSource, mouseOverSound);
        }

        previousSelectedOption = selectedOption;
    }

    int ThetaToIndex(float theta) {
        for (int i = 0; i < wheelOptions.Count; i++) {
            if (theta < (i + 0.5) * thetaDelta) {
                return i;
            }
        }
        return 0;
    }

}
