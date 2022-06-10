using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpriteDataSystem))]
public class SpriteDataSystemEditor : Editor {
    SpriteDataSystem spriteDataSystem;
    bool firstSprite;

    // get values
    SpriteRenderer torsoSpriteRenderer;
    SpriteRenderer headSpriteRenderer;
    Sprite torsoSprite;
    Sprite headSprite;
    int headOffsetX;
    int headOffsetY;
    bool overrideHeadDirection;
    bool headInFront;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        spriteDataSystem = (SpriteDataSystem)target;

        torsoSpriteRenderer = (SpriteRenderer)base.serializedObject.FindProperty("torsoSpriteRenderer").objectReferenceValue;
        headSpriteRenderer = (SpriteRenderer)base.serializedObject.FindProperty("headSpriteRenderer").objectReferenceValue;

        // get values
        torsoSprite = (Sprite)base.serializedObject.FindProperty("torsoSprite").objectReferenceValue;
        headSprite = (Sprite)base.serializedObject.FindProperty("headSprite").objectReferenceValue;
        headOffsetX = base.serializedObject.FindProperty("headOffsetX").intValue;
        headOffsetY = base.serializedObject.FindProperty("headOffsetY").intValue;

        GUILayout.Space(50);

        GUILayout.Label("Controls");
        if (GUILayout.Button("Load Skin")) {
            Sprite[] output = Resources.LoadAll<Sprite>($"sprites/spritesheets/{spriteDataSystem.skinName}/Torso") as Sprite[];
            Sprite[] headOutput = Resources.LoadAll<Sprite>($"sprites/spritesheets/{spriteDataSystem.skinName}/Head") as Sprite[];
            spriteDataSystem.torsoSprites = output;
            spriteDataSystem.headSprites = headOutput;
            spriteDataSystem.torsoIndex = 0;
            spriteDataSystem.headIndex = 0;
            SetSprites();
        }

        GUILayout.Label("Spritesheet Navigation");

        if (GUILayout.Button("Next Torso")) {
            spriteDataSystem.torsoIndex += 1;
            if (spriteDataSystem.torsoIndex >= spriteDataSystem.torsoSprites.Length) {
                spriteDataSystem.torsoIndex = 0;
            }
            torsoSprite = spriteDataSystem.torsoSprites[spriteDataSystem.torsoIndex];
            spriteDataSystem.torsoSprite = torsoSprite;
            SetSprites();
        }
        if (GUILayout.Button("Previous Torso")) {
            spriteDataSystem.torsoIndex -= 1;
            if (spriteDataSystem.torsoIndex < 0) {
                spriteDataSystem.torsoIndex = spriteDataSystem.torsoSprites.Length - 1;
            }
            torsoSprite = spriteDataSystem.torsoSprites[spriteDataSystem.torsoIndex];
            spriteDataSystem.torsoSprite = torsoSprite;
        }


        if (GUILayout.Button("Next Head")) {
            spriteDataSystem.headIndex += 1;
            if (spriteDataSystem.headIndex >= spriteDataSystem.headSprites.Length) {
                spriteDataSystem.headIndex = 0;
            }
            headSprite = spriteDataSystem.headSprites[spriteDataSystem.headIndex];
            spriteDataSystem.headSprite = headSprite;
        }
        if (GUILayout.Button("Previous Head")) {
            spriteDataSystem.headIndex -= 1;
            if (spriteDataSystem.headIndex < 0) {
                spriteDataSystem.headIndex = spriteDataSystem.headSprites.Length - 1;
            }
            headSprite = spriteDataSystem.headSprites[spriteDataSystem.headIndex];
            spriteDataSystem.headSprite = headSprite;
        }

        // overrideHeadDirection = EditorGUI.Toggle("Show Close Button");
        overrideHeadDirection = GUILayout.Toggle(overrideHeadDirection, "override head");
        headInFront = GUILayout.Toggle(headInFront, "head in front");


        GUILayout.Label("Sprite Data");
        if (GUILayout.Button("Add")) {
            TorsoSpriteData newData = new TorsoSpriteData {
                torsoSprite = spriteDataSystem.torsoIndex,
                headSprite = spriteDataSystem.headIndex,
                headOffset = new Vector2(headOffsetX, headOffsetY),
                headInFrontOfTorso = headInFront,
                overrideHeadDirection = overrideHeadDirection
            };
            spriteDataSystem.torsoSpriteData.Add(newData);
        }
        if (GUILayout.Button("Save Sprite Data")) {
            Save(spriteDataSystem.skinName, spriteDataSystem.torsoSpriteData);
            Debug.Log("Saved!");
        }
        if (GUILayout.Button("Load Sprite Data")) {
            Debug.Log("Loading...");
        }

        GUILayout.Label("Edit Specific Data");
        if (GUILayout.Button("Edit Sprite Data")) {
            ReloadSpriteData();
        }
        if (GUILayout.Button("Next Sprite Data")) {
            spriteDataSystem.dataIndex += 1;
            if (spriteDataSystem.dataIndex >= spriteDataSystem.torsoSprites.Length) {
                spriteDataSystem.dataIndex = 0;
            }
            ReloadSpriteData();
        }
        if (GUILayout.Button("Previous Sprite Data")) {
            spriteDataSystem.dataIndex -= 1;
            if (spriteDataSystem.dataIndex < 0) {
                spriteDataSystem.dataIndex = spriteDataSystem.torsoSprites.Length - 1;
            }
            ReloadSpriteData();
        }

        // call update on your serializedObject before testing and changing properties.
        // work with the serialized property's methods to make changes: InsertArrayElementAtIndex should be useful to you
        // when you're finished a round of changes, call ApplyModifiedProperties on the object

        SetSprites();
    }

    void ReloadSpriteData() {
        TorsoSpriteData data = spriteDataSystem.torsoSpriteData[spriteDataSystem.dataIndex];
        spriteDataSystem.torsoIndex = data.torsoSprite;
        spriteDataSystem.headIndex = data.headSprite;
        torsoSprite = spriteDataSystem.torsoSprites[spriteDataSystem.torsoIndex];
        headSprite = spriteDataSystem.headSprites[spriteDataSystem.headIndex];

        spriteDataSystem.torsoSprite = torsoSprite;
        spriteDataSystem.headSprite = headSprite;

        headOffsetX = (int)data.headOffset.x;
        headOffsetY = (int)data.headOffset.y;
        headInFront = data.headInFrontOfTorso;
        overrideHeadDirection = data.overrideHeadDirection;

        spriteDataSystem.headInFrontOfTorso = headInFront;
        spriteDataSystem.overrideHeadDirection = overrideHeadDirection;
        spriteDataSystem.headOffsetX = headOffsetX;
        spriteDataSystem.headOffsetY = headOffsetY;
    }

    void SetSprites() {
        // set positions of sprite renderer stufff
        torsoSpriteRenderer.sprite = torsoSprite;
        headSpriteRenderer.sprite = headSprite;
        headSpriteRenderer.transform.localPosition = new Vector3(headOffsetX / 100f, headOffsetY / 100f, 0);
        if (headInFront) {
            headSpriteRenderer.sortingOrder = 100;
        } else {
            headSpriteRenderer.sortingOrder = -100;
        }
        headSpriteRenderer.enabled = overrideHeadDirection;
        base.serializedObject.ApplyModifiedProperties();
    }
    void Save(string skin, List<TorsoSpriteData> spriteData) {
        Skin.SaveSpriteData(skin, spriteData);
    }
}
