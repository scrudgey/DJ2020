using UnityEngine;

[System.Serializable]
public class SpriteData {
    public int headSprite;
    public Vector2 headOffset;
    public bool overrideHeadDirection;
    public bool headInFrontOfTorso;
}

[System.Serializable]
public class SpriteDataLegs {
    // public int legSprite;
    public Vector2 torsoOffset;
}