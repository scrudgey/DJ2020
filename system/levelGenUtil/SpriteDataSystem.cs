using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpriteDataSystem : MonoBehaviour {
    public string skinName;
    [Header("Display")]
    public SpriteRenderer torsoSpriteRenderer;
    public SpriteRenderer headSpriteRenderer;
    public SpriteRenderer legSpriteRenderer;
    [Header("Sprite Data")]
    public int headOffsetX;
    public int headOffsetY;
    public int torsoOffsetX;
    public int torsoOffsetY;
    public int legIndex;
    public int torsoIndex;
    public int headIndex;
    // public int torsoDataIndex;
    [Header("Data Model")]
    public List<SpriteData> torsoSpriteData;
    public List<SpriteDataLegs> legSpriteData;

}
