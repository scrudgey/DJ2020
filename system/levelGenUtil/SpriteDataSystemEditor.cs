#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpriteDataSystem))]
public class SpriteDataSystemEditor : Editor {
    static string[] spriteSheetTypes = new string[] { "Torso", "pistol", "smg", "rifle", "shotgun" };
    SpriteDataSystem spriteDataSystem;
    bool firstSprite;
    SpriteRenderer torsoSpriteRenderer;
    SpriteRenderer headSpriteRenderer;
    SpriteRenderer legSpriteRenderer;
    Sprite legSprite;
    Sprite torsoSprite;
    Sprite headSprite;
    int headOffsetX;
    int headOffsetY;
    int torsoOffsetX;
    int torsoOffsetY;
    int headIndex;
    int torsoIndex;
    int legIndex;
    bool overrideHeadDirection;
    bool headInFront;
    int sheetIndex;

    public Sprite[] legSprites;
    public Sprite[] torsoSprites;
    public Sprite[] headSprites;
    SpriteData currentTorsoData;
    SpriteDataLegs currentLegData;
    SpriteData previousTorsoData;
    SpriteDataLegs previousLegData;
    public override void OnInspectorGUI() {
        serializedObject.UpdateIfRequiredOrScript();

        base.OnInspectorGUI();

        spriteDataSystem = (SpriteDataSystem)target;

        torsoSpriteRenderer = (SpriteRenderer)base.serializedObject.FindProperty("torsoSpriteRenderer").objectReferenceValue;
        headSpriteRenderer = (SpriteRenderer)base.serializedObject.FindProperty("headSpriteRenderer").objectReferenceValue;
        legSpriteRenderer = (SpriteRenderer)base.serializedObject.FindProperty("legSpriteRenderer").objectReferenceValue;

        headOffsetX = base.serializedObject.FindProperty("headOffsetX").intValue;
        headOffsetY = base.serializedObject.FindProperty("headOffsetY").intValue;

        torsoOffsetX = base.serializedObject.FindProperty("torsoOffsetX").intValue;
        torsoOffsetY = base.serializedObject.FindProperty("torsoOffsetY").intValue;

        headIndex = base.serializedObject.FindProperty("headIndex").intValue;
        torsoIndex = base.serializedObject.FindProperty("torsoIndex").intValue;
        legIndex = base.serializedObject.FindProperty("legIndex").intValue;

        if (GUILayout.Button("Clear all")) {
            ClearData();
        }

        // set current data
        if (spriteDataSystem.torsoSpriteData != null && spriteDataSystem.legSpriteData != null && spriteDataSystem.torsoSpriteData.Count > 0 && spriteDataSystem.legSpriteData.Count > 0) {
            // Debug.Log($"torsoindex: {torsoIndex} torsoSprites: {torsoSprites.Length} torsoData: {spriteDataSystem.torsoSpriteData.Count}");
            currentTorsoData = spriteDataSystem.torsoSpriteData[torsoIndex];
            currentLegData = spriteDataSystem.legSpriteData[legIndex];
        }

        GUILayout.Space(50);
        sheetIndex = EditorGUILayout.Popup(sheetIndex, spriteSheetTypes);
        string sheetTypeString = spriteSheetTypes[sheetIndex];

        if (torsoSprites == null || torsoSprites.Length == 0) {
            ShowUninitialized(sheetTypeString);
        } else {
            ShowFullControls(sheetTypeString);
        }

        base.serializedObject.ApplyModifiedProperties();

        previousTorsoData = currentTorsoData;
        previousLegData = currentLegData;
    }

    void ShowUninitialized(string sheetTypeString) {

        GUILayout.Label("Controls");
        if (GUILayout.Button("Load Spritesheet")) {
            LoadSpriteData(sheetTypeString);
        }
        if (GUILayout.Button("Clear all")) {
            ClearData();
        }
    }

    void ShowFullControls(string sheetTypeString) {
        if (currentTorsoData != previousTorsoData && currentTorsoData != null && previousTorsoData != null) {
            TransferDataToEditorState();
        }
        if (currentLegData != previousLegData && currentLegData != null && previousLegData != null) {
            TransferDataToEditorState();
        }

        GUILayout.Label("Controls");
        if (GUILayout.Button("Load Spritesheet")) {
            LoadSpriteData(sheetTypeString);
        }


        GUILayout.Label("Spritesheet Navigation");
        if (GUILayout.Button("Next Head")) {
            headIndex += 1;
        }
        if (GUILayout.Button("Previous Head")) {
            headIndex -= 1;
        }
        if (GUILayout.Button("Next Frame")) {
            torsoIndex += 1;
            legIndex = torsoIndex;
            GUI.FocusControl(null);
        }
        if (GUILayout.Button("Previous Frame")) {
            torsoIndex -= 1;
            legIndex = torsoIndex;
            GUI.FocusControl(null);
        }

        GUILayout.Label("Individual Components");
        if (GUILayout.Button("Next Torso")) {
            torsoIndex += 1;
        }
        if (GUILayout.Button("Previous Torso")) {
            torsoIndex -= 1;
        }
        if (GUILayout.Button("Next Leg")) {
            legIndex += 1;
        }
        if (GUILayout.Button("Previous Leg")) {
            legIndex -= 1;
        }

        torsoIndex = Toolbox.ClampWrap(torsoIndex, 0, torsoSprites.Length - 1);
        headIndex = Toolbox.ClampWrap(headIndex, 0, headSprites.Length - 1);
        legIndex = Toolbox.ClampWrap(legIndex, 0, legSprites.Length - 1);

        overrideHeadDirection = GUILayout.Toggle(overrideHeadDirection, "override head");
        headInFront = GUILayout.Toggle(headInFront, "head in front");

        GUILayout.Label("Torso Sprite Data");
        if (GUILayout.Button("Load Torso Sprite Data")) {
            Debug.Log("Loading...");
            spriteDataSystem.torsoSpriteData = LoadTorsoData(spriteDataSystem.skinName, sheetTypeString);
        }
        GUILayout.Label("Leg Sprite Data");
        if (GUILayout.Button("Load Leg Sprite Data")) {
            Debug.Log("Loading...");
            spriteDataSystem.legSpriteData = LoadLegData(spriteDataSystem.skinName);
        }
        GUILayout.Label("Save");
        if (GUILayout.Button("Save Data")) {
            Save(spriteDataSystem.skinName, spriteDataSystem.torsoSpriteData, spriteDataSystem.legSpriteData, sheetTypeString);
            Debug.Log("Saved!");
        }
        SetSprites();
    }

    void ClearData() {
        torsoSprites = null;
        headSprites = null;
        legSprites = null;
        torsoIndex = 0;
        headIndex = 0;
        legIndex = 0;
        torsoSprite = null;
        legSprite = null;
        headSprite = null;

        spriteDataSystem.torsoSpriteData = null;
        spriteDataSystem.legSpriteData = null;

        SetSprites();
    }
    void LoadSpriteData(string sheetTypeString) {
        torsoSprites = Resources.LoadAll<Sprite>($"sprites/spritesheets/{spriteDataSystem.skinName}/{sheetTypeString}") as Sprite[];
        headSprites = Resources.LoadAll<Sprite>($"sprites/spritesheets/{spriteDataSystem.skinName}/Head") as Sprite[];
        legSprites = Resources.LoadAll<Sprite>($"sprites/spritesheets/{spriteDataSystem.skinName}/Legs") as Sprite[];
        torsoIndex = 0;
        headIndex = 0;
        legIndex = 0;
        torsoSprite = torsoSprites[0];
        legSprite = legSprites[0];
        headSprite = headSprites[0];

        spriteDataSystem.torsoSpriteData = LoadTorsoData(spriteDataSystem.skinName, sheetTypeString);
        if (spriteDataSystem.torsoSpriteData == null) {
            spriteDataSystem.torsoSpriteData = new List<SpriteData>(new SpriteData[torsoSprites.Length]);
        }

        spriteDataSystem.legSpriteData = LoadLegData(spriteDataSystem.skinName);
        if (spriteDataSystem.legSpriteData == null) {
            spriteDataSystem.legSpriteData = new List<SpriteDataLegs>(new SpriteDataLegs[legSprites.Length]);
        }

        SetSprites();
    }

    void TransferDataToEditorState() {
        headIndex = currentTorsoData.headSprite;
        // Debug.Log(torsoSprites.Length);
        torsoSprite = torsoSprites != null ? torsoSprites[torsoIndex] : null;
        headSprite = headSprites != null ? headSprites[headIndex] : null;
        headOffsetX = (int)currentTorsoData.headOffset.x;
        headOffsetY = (int)currentTorsoData.headOffset.y;
        headInFront = currentTorsoData.headInFrontOfTorso;
        overrideHeadDirection = currentTorsoData.overrideHeadDirection;

        torsoOffsetX = (int)currentLegData.torsoOffset.x;
        torsoOffsetY = (int)currentLegData.torsoOffset.y;
    }
    void SetSprites() {
        // set current data
        currentTorsoData.headSprite = headIndex;
        currentTorsoData.headOffset.x = headOffsetX;
        currentTorsoData.headOffset.y = headOffsetY;
        currentTorsoData.headInFrontOfTorso = headInFront;
        currentTorsoData.overrideHeadDirection = overrideHeadDirection;

        // currentLegData.legSprite = legIndex;
        currentLegData.torsoOffset.x = torsoOffsetX;
        currentLegData.torsoOffset.y = torsoOffsetY;

        // set state from data
        headIndex = currentTorsoData.headSprite;
        torsoSprite = torsoSprites != null ? torsoSprites[torsoIndex] : null;
        headSprite = headSprites != null ? headSprites[headIndex] : null;
        legSprite = legSprites != null ? legSprites[legIndex] : null;
        headOffsetX = (int)currentTorsoData.headOffset.x;
        headOffsetY = (int)currentTorsoData.headOffset.y;
        headInFront = currentTorsoData.headInFrontOfTorso;
        overrideHeadDirection = currentTorsoData.overrideHeadDirection;

        torsoOffsetX = (int)currentLegData.torsoOffset.x;
        torsoOffsetY = (int)currentLegData.torsoOffset.y;

        // propagate state to sprite data system component so it is reflected in editor fields
        spriteDataSystem.headOffsetX = headOffsetX;
        spriteDataSystem.headOffsetY = headOffsetY;
        spriteDataSystem.headIndex = headIndex;
        spriteDataSystem.torsoIndex = torsoIndex;
        spriteDataSystem.legIndex = legIndex;

        // set positions of sprite renderer stufff
        torsoSpriteRenderer.sprite = torsoSprite;
        headSpriteRenderer.sprite = headSprite;
        legSpriteRenderer.sprite = legSprite;
        headSpriteRenderer.transform.localPosition = new Vector3(headOffsetX / 100f, headOffsetY / 100f, 0);
        torsoSpriteRenderer.transform.localPosition = new Vector3(torsoOffsetX / 100f, torsoOffsetY / 100f, 0);
        if (headInFront) {
            headSpriteRenderer.sortingOrder = 100;
        } else {
            headSpriteRenderer.sortingOrder = -100;
        }
        if (overrideHeadDirection) {
            headSpriteRenderer.color = Color.white;
        } else {
            headSpriteRenderer.color = Color.red;
        }


    }
    void Save(string skin, List<SpriteData> spriteData, List<SpriteDataLegs> legData, string sheetTypeString) {
        Skin.SaveSpriteData(skin, spriteData, sheetTypeString);
        Skin.SaveLegSpriteData(skin, legData);
        AssetDatabase.Refresh();
    }
    List<SpriteData> LoadTorsoData(string skin, string sheetTypeString) {
        List<SpriteData> loadedData = Skin.LoadTorsoSpriteData(skin, sheetTypeString);
        if (loadedData == null) loadedData = new List<SpriteData>();
        while (loadedData.Count < torsoSprites.Length) {
            loadedData.Add(new SpriteData());
        }
        return loadedData;
    }
    List<SpriteDataLegs> LoadLegData(string skin) {
        List<SpriteDataLegs> loadedData = Skin.LoadLegSpriteData(skin);
        if (loadedData == null) loadedData = new List<SpriteDataLegs>();
        while (loadedData.Count < legSprites.Length) {
            loadedData.Add(new SpriteDataLegs());
        }
        return loadedData;
    }
}

#endif