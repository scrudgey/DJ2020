using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpriteDataSystem : MonoBehaviour {
    public string skinName;
    [Header("Display")]
    public SpriteRenderer torsoSpriteRenderer;
    public SpriteRenderer headSpriteRenderer;
    [Header("Sprite Data")]
    public Sprite torsoSprite;
    public Sprite headSprite;
    public int headOffsetX;
    public int headOffsetY;
    public bool overrideHeadDirection;
    public bool headInFrontOfTorso;
    [Header("Data Model")]
    public List<SpriteData> spriteData;

    [HideInInspector]
    public Sprite[] torsoSprites;
    [HideInInspector]
    public Sprite[] headSprites;
    public int torsoIndex;
    public int headIndex;
    public int dataIndex;
}
