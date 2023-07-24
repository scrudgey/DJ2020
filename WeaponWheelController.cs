using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class WeaponWheelController : MonoBehaviour {
    public bool revealed;
    public GameObject wheelObject;
    Vector2 cursorPosition;
    public RectTransform mouseCursor;
    public TextMeshProUGUI selectorText;
    public Transform optionContainer;
    public GameObject weaponWheelOptionPrefab;
    public List<WeaponWheelOption> wheelOptions;

    WeaponWheelOption selectedOption;
    float thetaDelta;
    public void Initialize() {
        wheelOptions = new List<WeaponWheelOption>();
        foreach (Transform child in optionContainer) {
            Destroy(child.gameObject);
        }
        List<GunState> guns = new List<GunState>();
        if (GameManager.I.gameData.playerState.primaryGun != null) {
            guns.Add(GameManager.I.gameData.playerState.primaryGun);
        }
        if (GameManager.I.gameData.playerState.secondaryGun != null) {
            guns.Add(GameManager.I.gameData.playerState.secondaryGun);
        }
        if (GameManager.I.gameData.playerState.tertiaryGun != null) {
            guns.Add(GameManager.I.gameData.playerState.tertiaryGun);
        }
        thetaDelta = 2f * Mathf.PI / guns.Count;
        float theta = 0f;
        foreach (GunState gun in guns) {
            WeaponWheelOption option = CreateSelectorOption(theta, gun);
            wheelOptions.Add(option);
            theta += thetaDelta;
        }
    }
    WeaponWheelOption CreateSelectorOption(float theta, GunState gun) {
        GameObject selector = GameObject.Instantiate(weaponWheelOptionPrefab);
        selector.transform.SetParent(optionContainer, false);
        RectTransform selectorRect = selector.GetComponent<RectTransform>();
        Vector2 position = 155f * new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
        selectorRect.anchoredPosition = position;

        WeaponWheelOption weaponWheelOption = selector.GetComponent<WeaponWheelOption>();
        weaponWheelOption.Initialize(gun);
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
                ResetCursor();
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
        if (revealed) {
            selectorText.text = "";
            UpdateCursor(input);
        }
    }
    public void HideWheel() {
        revealed = false;
        wheelObject.SetActive(false);
    }
    void ResetCursor() {
        cursorPosition = Vector2.zero;
    }
    void CloseMenu(ref PlayerInput input) {
        Debug.Log($"closing menu with theta {selectedOption}");
        if (selectedOption.gun == GameManager.I.gameData.playerState.primaryGun) {
            input.selectgun = 1;
            Debug.Log("**** 1");
        } else if (selectedOption.gun == GameManager.I.gameData.playerState.secondaryGun) {
            input.selectgun = 2;
            Debug.Log("**** 2");
        } else if (selectedOption.gun == GameManager.I.gameData.playerState.tertiaryGun) {
            input.selectgun = 3;
            Debug.Log("**** 3");
        }
    }
    void UpdateCursor(PlayerInput input) {
        cursorPosition += input.mouseDelta * 5f;
        cursorPosition = Vector2.ClampMagnitude(cursorPosition, 200f);
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
