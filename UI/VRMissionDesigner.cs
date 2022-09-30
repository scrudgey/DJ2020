using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TMPro;
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

    public VRMissionTemplate template;
    public int selectedCharacter;
    public int selectedWeapon;

    [Header("scene controls")]
    public Image sceneImage;
    public TextMeshProUGUI sceneNameText;
    public TMP_Dropdown missionTypeDropdown;
    public TMP_Dropdown sensitivityDropdown;
    public TMP_InputField numberNPCInput;
    public TMP_InputField spawnIntervalInput;
    public Toggle alarmHQSelector;

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
    [Header("sounds")]
    public AudioSource audioSource;
    public AudioClip[] tabSounds;
    public AudioClip[] pickerCallbackSounds;
    public AudioClip[] weaponSelectorSounds;

    void Start() {
        foreach (Transform child in pickerContainer) {
            Destroy(child.gameObject);
        }
        tabOriginalColor = tab2.colors.normalColor;
        selectedCharacter = 1;
        template = VRMissionTemplate.Default();
        LoadVRMissionTemplate();
        OnDataChange();
    }
    void SaveVRMissionTemplate() {
        string path = VRMissionPath();
        try {
            using (FileStream fs = File.Open(path, FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs))
            using (JsonWriter jw = new JsonTextWriter(sw)) {
                JsonSerializer serializer = JsonSerializer.Create();
                serializer.Serialize(jw, template);
            }
            // Debug.Log($"wrote to {path}");
        }
        catch (Exception e) {
            Debug.LogError($"error writing to file: {path} {e}");
        }
    }
    void LoadVRMissionTemplate() {
        string path = VRMissionPath();
        try {
            // deserialize JSON directly from a file
            using (StreamReader file = File.OpenText(path)) {
                JsonSerializer serializer = new JsonSerializer();
                template = (VRMissionTemplate)serializer.Deserialize(file, typeof(VRMissionTemplate));
                Debug.Log($"successfully loaded VR mission template from {path}");
            }
        }
        catch (Exception e) {
            Debug.LogError($"error reading VR template file: {path} {e}");
        }
    }

    static public string VRMissionPath() => System.IO.Path.Join(Application.persistentDataPath, "vrmission.json");
    void InitializeWeaponPicker() {
        foreach (Transform child in pickerContainer) {
            Destroy(child.gameObject);
        }
        GunTemplate[] allGuns = Resources.LoadAll<GunTemplate>("data/guns");

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
        string sceneImagePath = $"data/scenegraphics/{template.sceneName}";
        Sprite sceneSprite = Resources.Load<Sprite>(sceneImagePath) as Sprite;
        sceneImage.sprite = sceneSprite;
        sceneNameText.text = template.sceneName;

        missionTypeDropdown.value = (int)template.missionType;
        sensitivityDropdown.value = (int)template.sensitivityLevel;
        numberNPCInput.text = template.maxNumberNPCs.ToString();
        spawnIntervalInput.text = template.NPCspawnInterval.ToString();
        alarmHQSelector.isOn = template.alarmHQEnabled;

        string legSkinName = selectedCharacter switch {
            1 => template.playerState.legSkin,
            2 => template.npc1State.legSkin,
            3 => template.npc2State.legSkin,
            _ => template.playerState.legSkin
        };
        string torsoSkinName = selectedCharacter switch {
            1 => template.playerState.bodySkin,
            2 => template.npc1State.bodySkin,
            3 => template.npc2State.bodySkin,
            _ => template.playerState.bodySkin
        };

        GunTemplate gun1 = selectedCharacter switch {
            1 => template.playerState.primaryGun,
            2 => template.npc1State.primaryGun,
            3 => template.npc2State.primaryGun,
            _ => template.playerState.primaryGun
        };
        GunTemplate gun2 = selectedCharacter switch {
            1 => template.playerState.secondaryGun,
            2 => template.npc1State.secondaryGun,
            3 => template.npc2State.secondaryGun,
            _ => template.playerState.secondaryGun
        };
        GunTemplate gun3 = selectedCharacter switch {
            1 => template.playerState.tertiaryGun,
            2 => template.npc1State.tertiaryGun,
            3 => template.npc2State.tertiaryGun,
            _ => template.playerState.tertiaryGun
        };

        Skin legsSkin = Skin.LoadSkin(legSkinName);
        Skin bodySkin = Skin.LoadSkin(torsoSkinName);
        headImage.sprite = bodySkin.headIdle[Direction.down][0];
        torsoImage.sprite = bodySkin.unarmedIdle[Direction.down][0];
        legsImage.sprite = legsSkin.legsIdle[Direction.down][0];

        healthInput.text = selectedCharacter switch {
            1 => template.playerState.fullHealthAmount.ToString(),
            2 => template.npc1State.fullHealthAmount.ToString(),
            3 => template.npc2State.fullHealthAmount.ToString(),
            _ => template.playerState.fullHealthAmount.ToString()
        };

        if (selectedCharacter == 1) {
            cyberLegsSelector.SetActive(true);
            cyberEyesSelector.SetActive(true);
            tertiataryWeaponSlotSelector.SetActive(true);
            tertiaryWeaponSelector.SetActive(template.playerState.thirdWeaponSlot);

            cyberLegsToggle.isOn = template.playerState.cyberlegsLevel > 0;
            cyberEyesToggle.isOn = template.playerState.cyberEyesThermal;
            extraWeaponToggle.isOn = template.playerState.thirdWeaponSlot;

            if (gun3 != null) {

                tertiaryWeaponImage.sprite = gun3.image;
                tertiaryWeaponCaption.text = gun3.name;
            } else {
                tertiaryWeaponImage.sprite = null;
                tertiaryWeaponCaption.text = "None";
            }
        } else {
            tertiaryWeaponSelector.SetActive(false);
            cyberLegsSelector.SetActive(false);
            cyberEyesSelector.SetActive(false);
            tertiataryWeaponSlotSelector.SetActive(false);
        }

        if (gun1 != null) {
            primaryWeaponImage.sprite = gun1.image;
            primaryWeaponCaption.text = gun1.name;
        } else {
            primaryWeaponImage.sprite = null;
            primaryWeaponCaption.text = "None";
        }

        if (gun2 != null) {
            secondaryWeaponImage.sprite = gun2.image;
            secondaryWeaponCaption.text = gun2.name;
        } else {
            secondaryWeaponImage.sprite = null;
            secondaryWeaponCaption.text = "None";
        }

        SaveVRMissionTemplate();
    }

    // scene controls
    public void ScenePreviousCallback() {
        template.sceneName = PreviousInList(SCENES, template.sceneName);
        OnDataChange();
    }
    public void SceneNextCallback() {
        template.sceneName = NextInList(SCENES, template.sceneName);
        OnDataChange();
    }
    public void MissionTypeCallback(TMP_Dropdown dropdown) {
        template.missionType = (VRMissionType)dropdown.value;
        OnDataChange();
    }
    public void SensitivityCallback(TMP_Dropdown dropdown) {
        template.sensitivityLevel = (SensitivityLevel)dropdown.value;
        OnDataChange();
    }
    public void NumberEnemiesCallback(TMP_InputField inputField) {
        inputField.text = Regex.Replace(inputField.text, @"[^a-zA-Z0-9 ]", "");
        template.maxNumberNPCs = int.Parse(inputField.text);
        OnDataChange();
    }
    public void SpawnIntervalCallback(TMP_InputField inputField) {
        inputField.text = Regex.Replace(inputField.text, @"[^a-zA-Z0-9 ]", "");
        template.NPCspawnInterval = float.Parse(inputField.text);
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
        Toolbox.RandomizeOneShot(audioSource, tabSounds, randomPitchWidth: 0.05f);
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
        Toolbox.RandomizeOneShot(audioSource, tabSounds, randomPitchWidth: 0.05f);

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
        Toolbox.RandomizeOneShot(audioSource, tabSounds, randomPitchWidth: 0.05f);
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
        template.playerState.bodySkin = PreviousInList(SPRITESHEETS, template.playerState.bodySkin);
        OnDataChange();
    }
    public void SkinHeadNext() {
        // TODO: handle separate head sheet
        template.playerState.bodySkin = NextInList(SPRITESHEETS, template.playerState.bodySkin);
        OnDataChange();
    }
    public void SkinTorsoPrevious() {
        template.playerState.bodySkin = PreviousInList(SPRITESHEETS, template.playerState.bodySkin);
        OnDataChange();
    }
    public void SkinTorsoNext() {
        template.playerState.bodySkin = NextInList(SPRITESHEETS, template.playerState.bodySkin);
        OnDataChange();
    }
    public void SkinLegsPrevious() {
        template.playerState.legSkin = PreviousInList(SPRITESHEETS, template.playerState.legSkin);
        OnDataChange();
    }
    public void SkinLegsNext() {
        template.playerState.legSkin = NextInList(SPRITESHEETS, template.playerState.legSkin);
        OnDataChange();
    }

    // stat callbacks
    public void HealthCallback(TMP_InputField inputField) {
        if (selectedCharacter == 1) {
            template.playerState.fullHealthAmount = float.Parse(inputField.text);
        } else if (selectedCharacter == 2) {
            template.npc1State.fullHealthAmount = float.Parse(inputField.text);
        } else if (selectedCharacter == 3) {
            template.npc2State.fullHealthAmount = float.Parse(inputField.text);
        }
    }
    public void AlarmHQCallback(Toggle toggle) {
        template.alarmHQEnabled = toggle.isOn;
        OnDataChange();
    }
    public void CyberLegsCallback(Toggle toggle) {
        template.playerState.cyberlegsLevel = toggle.isOn ? 1 : 0;
        OnDataChange();
    }
    public void CyberEyesCallback(Toggle toggle) {
        template.playerState.cyberEyesThermal = toggle.isOn;
        OnDataChange();
    }
    public void WeaponSlotCallback(Toggle toggle) {
        template.playerState.thirdWeaponSlot = toggle.isOn;
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
        Toolbox.RandomizeOneShot(audioSource, weaponSelectorSounds, randomPitchWidth: 0.05f);

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
        Toolbox.RandomizeOneShot(audioSource, weaponSelectorSounds, randomPitchWidth: 0.05f);

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
        Toolbox.RandomizeOneShot(audioSource, weaponSelectorSounds, randomPitchWidth: 0.05f);
    }
    public void WeaponClearCallback(int weaponIndex) {
        IGunHandlerTemplate state = selectedCharacter switch {
            1 => template.playerState,
            2 => template.npc1State,
            3 => template.npc2State,
            _ => template.playerState
        };
        switch (weaponIndex) {
            case 1:
                state.primaryGun = null;
                break;
            case 2:
                state.secondaryGun = null;
                break;
            case 3:
                state.tertiaryGun = null;
                break;
        }
        OnDataChange();
    }

    public void WeaponPickerCallback(WeaponPickerHandler handler) {
        IGunHandlerTemplate state = selectedCharacter switch {
            1 => template.playerState,
            2 => template.npc1State,
            3 => template.npc2State,
            _ => template.playerState
        };
        switch (selectedWeapon) {
            case 1:
                state.primaryGun = handler.gun;
                break;
            case 2:
                state.secondaryGun = handler.gun;
                break;
            case 3:
                state.tertiaryGun = handler.gun;
                break;
            default:
                break;
        }
        Toolbox.RandomizeOneShot(audioSource, pickerCallbackSounds, randomPitchWidth: 0.05f);
        OnDataChange();
    }

    public void StartMissionCallback() {
        GameManager.I.LoadVRMission(template);
    }
}
