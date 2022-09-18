using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public class VRMissionDesigner : MonoBehaviour {
    static readonly List<string> SPRITESHEETS = new List<string>{
        "Jack",
        "civ_male",
        "civ_female"
    };
    static readonly List<string> SCENES = new List<string>{
        "VR",
        "VR_infiltration",
    };

    public VRMissionData data;
    public int selectedCharacter;
    public int selectedWeapon;

    [Header("scene controls")]
    public Image sceneImage;
    public TextMeshProUGUI sceneNameText;
    public TMP_Dropdown missionTypeDropdown;
    public TMP_Dropdown sensitivityDropdown;
    public TMP_InputField numberNPCInput;
    [Header("character controls")]
    public TextMeshProUGUI characterTitle;
    public Image headImage;
    public Image torsoImage;
    public Image legsImage;
    public TMP_InputField healthInput;
    public Toggle cyberLegsToggle;
    public Toggle cyberEyesToggle;
    public Toggle extraWeaponToggle;
    [Header("inv controls")]
    public Image primaryWeaponImage;
    public TextMeshProUGUI primaryWeaponCaption;
    public Image secondaryWeaponImage;
    public TextMeshProUGUI secondaryWeaponCaption;
    public Image tertiaryWeaponImage;
    public TextMeshProUGUI tertiaryWeaponCaption;
    public Button weapon1Button;
    public Button weapon2Button;
    public Button weapon3Button;

    [Header("player controls")]
    public GameObject tertiaryWeaponSelector;
    public GameObject cyberLegsSelector;
    public GameObject cyberEyesSelector;
    public GameObject tertiataryWeaponSlotSelector;

    [Header("picker")]
    public Transform pickerContainer;
    public GameObject weaponPickerPrefab;
    public GameObject pickerDivider;
    [Header("tabs")]
    public Button tab1;
    public Button tab2;
    public Button tab3;
    public GameObject tabOverlap1;
    public GameObject tabOverlap2;
    public GameObject tabOverlap3;
    public Color bodyMainColor;
    public Color weaponHighlightColor;
    public Color weaponDefaultColor;
    Color tabOriginalColor;

    void Start() {
        foreach (Transform child in pickerContainer) {
            Destroy(child.gameObject);
        }
        tabOriginalColor = tab2.colors.normalColor;
        selectedCharacter = 1;
        data = VRMissionData.DefaultData();
        OnDataChange();
    }
    void InitializeWeaponPicker() {
        foreach (Transform child in pickerContainer) {
            Destroy(child.gameObject);
        }
        Gun[] allGuns = Resources.LoadAll<Gun>("data/guns");

        // None button

        // create pistol divider
        instantiateDivider("Pistol");
        allGuns.Where(gun => gun.type == GunType.pistol).ToList()
            .ForEach(gun => {
                WeaponPickerHandler pickerHandler = instantiateWeaponPicker();
                pickerHandler.Configure(gun, this);
            });

        // create smg divider
        instantiateDivider("SMG");
        allGuns.Where(gun => gun.type == GunType.smg).ToList()
            .ForEach(gun => {
                WeaponPickerHandler pickerHandler = instantiateWeaponPicker();
                pickerHandler.Configure(gun, this);
            });

        // create rifle divider
        instantiateDivider("Rifle");
        allGuns.Where(gun => gun.type == GunType.rifle).ToList()
            .ForEach(gun => {
                WeaponPickerHandler pickerHandler = instantiateWeaponPicker();
                pickerHandler.Configure(gun, this);
            });

        // create shotgun divider
        instantiateDivider("Shotgun");
        allGuns.Where(gun => gun.type == GunType.shotgun).ToList()
            .ForEach(gun => {
                WeaponPickerHandler pickerHandler = instantiateWeaponPicker();
                pickerHandler.Configure(gun, this);
            });
    }
    WeaponPickerHandler instantiateWeaponPicker() {
        GameObject gameObject = GameObject.Instantiate(weaponPickerPrefab);
        gameObject.transform.SetParent(pickerContainer, false);
        return gameObject.GetComponent<WeaponPickerHandler>();
    }
    TextMeshProUGUI instantiateDivider(string caption) {
        GameObject gameObject = GameObject.Instantiate(pickerDivider);
        gameObject.transform.SetParent(pickerContainer, false);
        TextMeshProUGUI text = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        text.text = caption;
        return text;
    }

    void OnDataChange() {
        string sceneImagePath = $"data/scenegraphics/{data.sceneName}";
        // string sceneImagePath = $"data/scenegraphics/VR_infiltration";
        Sprite sceneSprite = Resources.Load<Sprite>(sceneImagePath) as Sprite;
        sceneImage.sprite = sceneSprite;
        sceneNameText.text = data.sceneName;

        missionTypeDropdown.value = (int)data.missionType;
        sensitivityDropdown.value = (int)data.sensitivityLevel;
        numberNPCInput.text = data.maxNumberNPCs.ToString();


        // TODO: change depending on selected tab

        string legSkinName = selectedCharacter switch {
            1 => data.playerState.legSkin,
            2 => data.npc1State.legSkin,
            3 => data.npc2State.legSkin,
            _ => data.playerState.legSkin
        };
        string torsoSkinName = selectedCharacter switch {
            1 => data.playerState.bodySkin,
            2 => data.npc1State.bodySkin,
            3 => data.npc2State.bodySkin,
            _ => data.playerState.bodySkin
        };

        Gun gun1 = selectedCharacter switch {
            1 => data.playerState.primaryGun.baseGun,
            2 => data.npc1State.primaryGun.baseGun,
            3 => data.npc2State.primaryGun.baseGun,
            _ => data.playerState.primaryGun.baseGun
        };
        Gun gun2 = selectedCharacter switch {
            1 => data.playerState.secondaryGun.baseGun,
            2 => data.npc1State.secondaryGun.baseGun,
            3 => data.npc2State.secondaryGun.baseGun,
            _ => data.playerState.secondaryGun.baseGun
        };
        Gun gun3 = selectedCharacter switch {
            1 => data.playerState.tertiaryGun.baseGun,
            2 => data.npc1State.tertiaryGun.baseGun,
            3 => data.npc2State.tertiaryGun.baseGun,
            _ => data.playerState.tertiaryGun.baseGun
        };

        Skin legsSkin = Skin.LoadSkin(legSkinName);
        Skin bodySkin = Skin.LoadSkin(torsoSkinName);
        headImage.sprite = bodySkin.headIdle[Direction.down][0];
        torsoImage.sprite = bodySkin.unarmedIdle[Direction.down][0];
        legsImage.sprite = legsSkin.legsIdle[Direction.down][0];

        healthInput.text = selectedCharacter switch {
            1 => data.playerState.fullHealthAmount.ToString(),
            2 => data.npc1State.fullHealthAmount.ToString(),
            3 => data.npc2State.fullHealthAmount.ToString(),
            _ => data.playerState.fullHealthAmount.ToString()
        };

        if (selectedCharacter == 1) {
            cyberLegsSelector.SetActive(true);
            cyberEyesSelector.SetActive(true);
            tertiataryWeaponSlotSelector.SetActive(true);
            tertiaryWeaponSelector.SetActive(data.playerState.thirdWeaponSlot);

            cyberLegsToggle.isOn = data.playerState.cyberlegsLevel > 0;
            cyberEyesToggle.isOn = data.playerState.cyberEyesThermal;
            extraWeaponToggle.isOn = data.playerState.thirdWeaponSlot;


            tertiaryWeaponImage.sprite = gun3.image;
            tertiaryWeaponCaption.text = gun3.name;
        } else {
            tertiaryWeaponSelector.SetActive(false);
            cyberLegsSelector.SetActive(false);
            cyberEyesSelector.SetActive(false);
            tertiataryWeaponSlotSelector.SetActive(false);
        }

        primaryWeaponImage.sprite = gun1.image;
        primaryWeaponCaption.text = gun1.name;

        secondaryWeaponImage.sprite = gun2.image;
        secondaryWeaponCaption.text = gun2.name;
    }

    // scene controls
    public void ScenePreviousCallback() {
        data.sceneName = PreviousInList(SCENES, data.sceneName);
        OnDataChange();
    }
    public void SceneNextCallback() {
        data.sceneName = NextInList(SCENES, data.sceneName);
        OnDataChange();
    }
    public void MissionTypeCallback(TMP_Dropdown dropdown) {
        data.missionType = (VRMissionType)dropdown.value;
        OnDataChange();
    }
    public void SensitivityCallback(TMP_Dropdown dropdown) {
        data.sensitivityLevel = (SensitivityLevel)dropdown.value;
        OnDataChange();
    }
    public void NumberEnemiesCallback(TMP_InputField inputField) {
        data.maxNumberNPCs = int.Parse(inputField.text);
        OnDataChange();
    }

    // tab controls
    public void TabPlayerCallback() {
        selectedCharacter = 1;
        characterTitle.text = "Player";
        OnDataChange();

        tabOverlap1.SetActive(true);
        tabOverlap2.SetActive(false);
        tabOverlap3.SetActive(false);

        ColorBlock cb = tab1.colors;
        cb.normalColor = bodyMainColor;
        cb.highlightedColor = bodyMainColor;
        tab1.colors = cb;

        ColorBlock cb2 = tab2.colors;
        cb2.normalColor = tabOriginalColor;
        cb2.highlightedColor = tabOriginalColor;
        tab2.colors = cb2;

        ColorBlock cb3 = tab3.colors;
        cb3.normalColor = tabOriginalColor;
        cb3.highlightedColor = tabOriginalColor;
        tab3.colors = cb3;
    }
    public void TabNPC1Callback() {
        selectedCharacter = 2;
        characterTitle.text = "Guard";
        OnDataChange();
        tabOverlap1.SetActive(false);
        tabOverlap2.SetActive(true);
        tabOverlap3.SetActive(false);

        ColorBlock cb = tab1.colors;
        cb.normalColor = tabOriginalColor;
        cb.highlightedColor = tabOriginalColor;
        tab1.colors = cb;

        ColorBlock cb2 = tab2.colors;
        cb2.normalColor = bodyMainColor;
        cb2.highlightedColor = bodyMainColor;
        tab2.colors = cb2;

        ColorBlock cb3 = tab3.colors;
        cb3.normalColor = tabOriginalColor;
        cb3.highlightedColor = tabOriginalColor;
        tab3.colors = cb3;
    }
    public void TabNPC2Callback() {
        selectedCharacter = 3;
        characterTitle.text = "Strike team";
        OnDataChange();
        tabOverlap1.SetActive(false);
        tabOverlap2.SetActive(false);
        tabOverlap3.SetActive(true);

        ColorBlock cb = tab1.colors;
        cb.normalColor = tabOriginalColor;
        cb.highlightedColor = tabOriginalColor;
        tab1.colors = cb;

        ColorBlock cb2 = tab2.colors;
        cb2.normalColor = tabOriginalColor;
        cb2.highlightedColor = tabOriginalColor;
        tab2.colors = cb2;

        ColorBlock cb3 = tab3.colors;
        cb3.normalColor = bodyMainColor;
        cb3.highlightedColor = bodyMainColor;
        tab3.colors = cb3;
    }


    string PreviousInList(List<string> list, string current) {
        int bodySpritesheetIndex = list.IndexOf(current) - 1;
        if (bodySpritesheetIndex < 0) {
            bodySpritesheetIndex = list.Count - 1;
        }
        return list[bodySpritesheetIndex];
    }
    string NextInList(List<string> list, string current) {
        int bodySpritesheetIndex = list.IndexOf(current) + 1;
        if (bodySpritesheetIndex >= list.Count) {
            bodySpritesheetIndex = 0;
        }
        return list[bodySpritesheetIndex];
    }
    // skin controls
    public void SkinHeadPrevious() {
        // TODO: handle separate head sheet
        data.playerState.bodySkin = PreviousInList(SPRITESHEETS, data.playerState.bodySkin);
        OnDataChange();
    }
    public void SkinHeadNext() {
        // TODO: handle separate head sheet
        data.playerState.bodySkin = NextInList(SPRITESHEETS, data.playerState.bodySkin);
        OnDataChange();
    }
    public void SkinTorsoPrevious() {
        data.playerState.bodySkin = PreviousInList(SPRITESHEETS, data.playerState.bodySkin);
        OnDataChange();
    }
    public void SkinTorsoNext() {
        data.playerState.bodySkin = NextInList(SPRITESHEETS, data.playerState.bodySkin);
        OnDataChange();
    }
    public void SkinLegsPrevious() {
        data.playerState.legSkin = PreviousInList(SPRITESHEETS, data.playerState.legSkin);
        OnDataChange();
    }
    public void SkinLegsNext() {
        data.playerState.legSkin = NextInList(SPRITESHEETS, data.playerState.legSkin);
        OnDataChange();
    }

    // stat callbacks
    public void HealthCallback(TMP_InputField inputField) {
        if (selectedCharacter == 1) {
            data.playerState.fullHealthAmount = float.Parse(inputField.text);
        } else if (selectedCharacter == 2) {
            data.npc1State.fullHealthAmount = float.Parse(inputField.text);
        } else if (selectedCharacter == 3) {
            data.npc2State.fullHealthAmount = float.Parse(inputField.text);
        }
    }
    public void CyberLegsCallback(Toggle toggle) {
        data.playerState.cyberlegsLevel = toggle.isOn ? 1 : 0;
        OnDataChange();
    }
    public void CyberEyesCallback(Toggle toggle) {
        data.playerState.cyberEyesThermal = toggle.isOn;
        OnDataChange();
    }
    public void WeaponSlotCallback(Toggle toggle) {
        data.playerState.thirdWeaponSlot = toggle.isOn;
        OnDataChange();
    }

    // weapon slot callbacks
    public void Weapon1Callback() {
        selectedWeapon = 1;
        InitializeWeaponPicker();


        ColorBlock cb = weapon1Button.colors;
        cb.normalColor = weaponHighlightColor;
        weapon1Button.colors = cb;

        ColorBlock cb2 = weapon2Button.colors;
        cb2.normalColor = weaponDefaultColor;
        weapon2Button.colors = cb2;

        ColorBlock cb3 = weapon3Button.colors;
        cb3.normalColor = weaponDefaultColor;
        weapon3Button.colors = cb3;
    }
    public void Weapon2Callback() {
        selectedWeapon = 2;
        InitializeWeaponPicker();

        ColorBlock cb = weapon1Button.colors;
        cb.normalColor = weaponDefaultColor;
        weapon1Button.colors = cb;

        ColorBlock cb2 = weapon2Button.colors;
        cb2.normalColor = weaponHighlightColor;
        weapon2Button.colors = cb2;

        ColorBlock cb3 = weapon3Button.colors;
        cb3.normalColor = weaponDefaultColor;
        weapon3Button.colors = cb3;
    }
    public void Weapon3Callback() {
        selectedWeapon = 3;
        InitializeWeaponPicker();

        ColorBlock cb = weapon1Button.colors;
        cb.normalColor = weaponDefaultColor;
        weapon1Button.colors = cb;

        ColorBlock cb2 = weapon2Button.colors;
        cb2.normalColor = weaponDefaultColor;
        weapon2Button.colors = cb2;

        ColorBlock cb3 = weapon3Button.colors;
        cb3.normalColor = weaponHighlightColor;
        weapon3Button.colors = cb3;
    }

    public void WeaponPickerCallback(WeaponPickerHandler handler) {
        IGunHandlerState state = selectedCharacter switch {
            1 => data.playerState,
            2 => data.npc1State,
            3 => data.npc2State,
            _ => data.playerState
        };
        switch (selectedWeapon) {
            case 1:
                state.primaryGun = new GunInstance(handler.gun);
                break;
            case 2:
                state.secondaryGun = new GunInstance(handler.gun);
                break;
            case 3:
                state.tertiaryGun = new GunInstance(handler.gun);
                break;
            default:
                break;
        }
        OnDataChange();
    }

    public void StartMissionCallback() {
        data.playerState.health = data.playerState.fullHealthAmount;
        data.npc1State.health = data.npc1State.fullHealthAmount;
        data.npc2State.health = data.npc2State.fullHealthAmount;
        GameManager.I.LoadVRMission(data);
    }
}
