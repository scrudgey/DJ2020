using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Items;
using UnityEngine;
[System.Serializable]
public class Skin {
    // head
    public Octet<Sprite[]> headIdle = new Octet<Sprite[]>();
    public Sprite[] headSprites;
    // legs
    public Octet<Sprite[]> legsIdle = new Octet<Sprite[]>();
    public Octet<Sprite[]> legsWalk = new Octet<Sprite[]>();
    public Octet<Sprite[]> legsCrouch = new Octet<Sprite[]>();
    public Octet<Sprite[]> legsCrawl = new Octet<Sprite[]>();
    public Octet<Sprite[]> legsRun = new Octet<Sprite[]>();
    public Octet<Sprite[]> legsClimb = new Octet<Sprite[]>();
    public Octet<Sprite[]> legsJump = new Octet<Sprite[]>();
    public Octet<Sprite[]> legsDead = new Octet<Sprite[]>();
    public Octet<Sprite[]> legsKeelOver = new Octet<Sprite[]>();
    public Octet<Sprite[]> legsCorpse = new Octet<Sprite[]>();

    // unarmed
    public Octet<Sprite[]> unarmedIdle = new Octet<Sprite[]>();
    public Octet<Sprite[]> unarmedWalk = new Octet<Sprite[]>();
    public Octet<Sprite[]> unarmedCrouch = new Octet<Sprite[]>();
    public Octet<Sprite[]> unarmedCrawl = new Octet<Sprite[]>();
    public Octet<Sprite[]> unarmedRun = new Octet<Sprite[]>();
    public Octet<Sprite[]> unarmedClimb = new Octet<Sprite[]>();
    public Octet<Sprite[]> unarmedJump = new Octet<Sprite[]>();
    public Octet<Sprite[]> unarmedUse = new Octet<Sprite[]>();
    public Octet<Sprite[]> unarmedHandsUp = new Octet<Sprite[]>();
    public Octet<Sprite[]> unarmedDead = new Octet<Sprite[]>();
    public Octet<Sprite[]> unarmedKeelOver = new Octet<Sprite[]>();
    public Octet<Sprite[]> unarmedCorpse = new Octet<Sprite[]>();


    // pistol
    public Octet<Sprite[]> pistolIdle = new Octet<Sprite[]>();
    public Octet<Sprite[]> pistolShoot = new Octet<Sprite[]>();
    public Octet<Sprite[]> pistolRack = new Octet<Sprite[]>();
    public Octet<Sprite[]> pistolReload = new Octet<Sprite[]>();
    public Octet<Sprite[]> pistolRun = new Octet<Sprite[]>();
    public Octet<Sprite[]> pistolHolster = new Octet<Sprite[]>();

    // smg
    public Octet<Sprite[]> smgIdle = new Octet<Sprite[]>();
    public Octet<Sprite[]> smgShoot = new Octet<Sprite[]>();
    public Octet<Sprite[]> smgRack = new Octet<Sprite[]>();
    public Octet<Sprite[]> smgReload = new Octet<Sprite[]>();
    public Octet<Sprite[]> smgRun = new Octet<Sprite[]>();
    public Octet<Sprite[]> smgHolster = new Octet<Sprite[]>();


    // shotgun
    public Octet<Sprite[]> shotgunIdle = new Octet<Sprite[]>();
    public Octet<Sprite[]> shotgunShoot = new Octet<Sprite[]>();
    public Octet<Sprite[]> shotgunRack = new Octet<Sprite[]>();
    public Octet<Sprite[]> shotgunReload = new Octet<Sprite[]>();
    public Octet<Sprite[]> shotgunHolster = new Octet<Sprite[]>();


    // rifle
    public Octet<Sprite[]> rifleIdle = new Octet<Sprite[]>();
    public Octet<Sprite[]> rifleShoot = new Octet<Sprite[]>();
    public Octet<Sprite[]> rifleRack = new Octet<Sprite[]>();
    public Octet<Sprite[]> rifleReload = new Octet<Sprite[]>();
    public Octet<Sprite[]> rifleHolster = new Octet<Sprite[]>();

    // sword
    public Octet<Sprite[]> swordIdle = new Octet<Sprite[]>();
    public Octet<Sprite[]> swordHolster = new Octet<Sprite[]>();
    public Octet<Sprite[]> swordSwing = new Octet<Sprite[]>();

    // fence cutter
    public Octet<Sprite[]> fenceCutterIdle = new Octet<Sprite[]>();
    public Octet<Sprite[]> fenceCutterCut = new Octet<Sprite[]>();

    // skeleton
    public Octet<Sprite[]> skeletonSprites = new Octet<Sprite[]>();

    // data
    public SpriteData[] unarmedSpriteData;
    public SpriteData[] pistolSpriteData;
    public SpriteData[] smgSpriteData;
    public SpriteData[] rifleSpriteData;
    public SpriteData[] shotgunSpriteData;
    public SpriteData[] swordSpriteData;
    public Dictionary<string, SpriteData> allSpriteData;


    public Octet<Sprite[]> gunIdleSprites(GunType type) {
        switch (type) {
            case GunType.smg:
                return smgIdle;
            case GunType.shotgun:
                return shotgunIdle;
            case GunType.pistol:
                return pistolIdle;
            case GunType.rifle:
                return rifleIdle;
            case GunType.sword:
                return swordIdle;
            default:
            case GunType.unarmed:
                return unarmedIdle;
        }
    }
    public Octet<Sprite[]> gunWalkSprites(GunType type) {
        switch (type) {
            case GunType.smg:
                return smgIdle;
            case GunType.shotgun:
                return shotgunIdle;
            case GunType.pistol:
                return pistolIdle;
            case GunType.rifle:
                return rifleIdle;
            default:
            case GunType.unarmed:
                return unarmedWalk;
        }
    }
    public Octet<Sprite[]> shootSprites(GunType type) {
        switch (type) {
            case GunType.smg:
                return smgShoot;
            case GunType.shotgun:
                return shotgunShoot;
            case GunType.rifle:
                return rifleShoot;
            case GunType.sword:
                return swordSwing;
            default:
            case GunType.pistol:
                return pistolShoot;
        }
    }
    public Octet<Sprite[]> gunRunSprites(GunType type) {
        switch (type) {
            default:
            case GunType.unarmed:
                return unarmedRun;
            case GunType.pistol:
                return pistolRun;
            case GunType.shotgun:
            case GunType.rifle:
            case GunType.smg:
                return smgRun;
        }
    }
    public Octet<Sprite[]> gunCrouchSprites(GunType type) {
        switch (type) {
            case GunType.unarmed:
                return unarmedCrouch;
            default:
                return gunIdleSprites(type);
        }
    }
    public Octet<Sprite[]> gunRackSprites(GunType type) {
        switch (type) {
            case GunType.smg:
                return smgRack;
            case GunType.rifle:
                return rifleRack;
            case GunType.shotgun:
                return shotgunRack;
            default:
            case GunType.pistol:
                return pistolRack;
        }
    }
    public Octet<Sprite[]> reloadSprites(GunType type) {
        switch (type) {
            default:
            case GunType.smg:
                return smgReload;
            case GunType.rifle:
                return rifleReload;
            case GunType.shotgun:
                return shotgunReload;
            case GunType.pistol:
                return pistolReload;
        }
    }
    public Octet<Sprite[]> holsterSprites(AnimationInput.GunAnimationInput input) {
        // |TODO: this doesn't work when guntype -> null
        if (input.gunType == GunType.unarmed) { // holstering
            switch (input.fromGunType) {
                default:
                case GunType.smg:
                    return smgHolster;
                case GunType.rifle:
                    return rifleHolster;
                case GunType.shotgun:
                    return shotgunHolster;
                case GunType.pistol:
                    return pistolHolster;
                case GunType.sword:
                    return swordHolster;
            }
        } else {
            switch (input.toGunType) {
                default:
                case GunType.smg:
                    return smgHolster;
                case GunType.rifle:
                    return rifleHolster;
                case GunType.shotgun:
                    return shotgunHolster;
                case GunType.pistol:
                    return pistolHolster;
                case GunType.sword:
                    return swordHolster;
            }
        }

    }
    private static Sprite[] loadSprites(string name, string sheet) {
        Sprite[] output = Resources.LoadAll<Sprite>(PathToSkinSpritesheet(name, sheet)) as Sprite[];
        // if (output.Length == 0) {
        //     return Resources.LoadAll<Sprite>(PathToSkinSpritesheet("generic", sheet)) as Sprite[];
        // } else return output;
        return output;
    }
    public static string PathToSkinDirectory(string name) {
        return $"sprites/spritesheets/{name}";
    }
    public static string PathToSkinSpritesheet(string name, string sheet) {
        return $"sprites/spritesheets/{name}/{sheet}";
    }
    public static Skin LoadSkin(string name) {
        Sprite[] legSprites = loadSprites(name, "legs");

        Sprite[] torsoSprites = loadSprites(name, "torso");

        Sprite[] pistolSprites = loadSprites(name, "pistol");

        Sprite[] smgSprites = loadSprites(name, "smg");

        Sprite[] shotgunSprites = loadSprites(name, "shotgun");

        Sprite[] rifleSprites = loadSprites(name, "rifle");

        Sprite[] swordSprites = loadSprites(name, "Sword");

        Sprite[] fenceCutterSprites = loadSprites(name, "Fence-cutter");

        Sprite[] headSprites = loadSprites(name, "head");

        Sprite[] skeletonSprites = Resources.LoadAll<Sprite>("sprites/spritesheets/Skeleton") as Sprite[];


        List<SpriteData> torsoSpriteData = LoadTorsoSpriteData(name, "Torso");
        List<SpriteData> pistolSpriteData = LoadTorsoSpriteData(name, "pistol");
        List<SpriteData> smgSpriteData = LoadTorsoSpriteData(name, "smg");
        List<SpriteData> rifleSpriteData = LoadTorsoSpriteData(name, "rifle");
        List<SpriteData> shotgunSpriteData = LoadTorsoSpriteData(name, "shotgun");


        Skin skin = new Skin();

        skin.skeletonSprites[Direction.down] = skeletonSprites;
        skin.skeletonSprites[Direction.rightDown] = skeletonSprites;
        skin.skeletonSprites[Direction.right] = skeletonSprites;
        skin.skeletonSprites[Direction.rightUp] = skeletonSprites;
        skin.skeletonSprites[Direction.up] = skeletonSprites;

        // head
        skin.headIdle[Direction.down] = new Sprite[] { headSprites[0], headSprites[5] };
        skin.headIdle[Direction.rightDown] = new Sprite[] { headSprites[1], headSprites[6] };
        skin.headIdle[Direction.right] = new Sprite[] { headSprites[2], headSprites[7] };
        skin.headIdle[Direction.rightUp] = new Sprite[] { headSprites[3], headSprites[8] };
        skin.headIdle[Direction.up] = new Sprite[] { headSprites[4], headSprites[9] };
        skin.headSprites = headSprites;

        // legs
        skin.legsIdle[Direction.down] = new Sprite[] { legSprites[0] };
        skin.legsIdle[Direction.rightDown] = new Sprite[] { legSprites[1] };
        skin.legsIdle[Direction.right] = new Sprite[] { legSprites[2] };
        skin.legsIdle[Direction.rightUp] = new Sprite[] { legSprites[3] };
        skin.legsIdle[Direction.up] = new Sprite[] { legSprites[4] };

        skin.legsWalk[Direction.down] = new Sprite[] { legSprites[5], legSprites[6], legSprites[7], legSprites[8] };
        skin.legsWalk[Direction.rightDown] = new Sprite[] { legSprites[9], legSprites[10], legSprites[11], legSprites[12] };
        skin.legsWalk[Direction.right] = new Sprite[] { legSprites[13], legSprites[14], legSprites[15], legSprites[16] };
        skin.legsWalk[Direction.rightUp] = new Sprite[] { legSprites[17], legSprites[18], legSprites[19], legSprites[20] };
        skin.legsWalk[Direction.up] = new Sprite[] { legSprites[21], legSprites[22], legSprites[23], legSprites[24] };

        skin.legsCrouch[Direction.rightDown] = new Sprite[] { legSprites[67] }; // 25
        skin.legsCrouch[Direction.rightUp] = new Sprite[] { legSprites[68] }; // 26

        skin.legsCrawl[Direction.down] = new Sprite[] { legSprites[27], legSprites[28], legSprites[29], legSprites[30] };
        skin.legsCrawl[Direction.rightDown] = new Sprite[] { legSprites[31], legSprites[32], legSprites[33], legSprites[34] };
        skin.legsCrawl[Direction.right] = new Sprite[] { legSprites[35], legSprites[36], legSprites[37], legSprites[38] };
        skin.legsCrawl[Direction.rightUp] = new Sprite[] { legSprites[39], legSprites[40], legSprites[41], legSprites[42] };
        skin.legsCrawl[Direction.up] = new Sprite[] { legSprites[43], legSprites[44], legSprites[45], legSprites[46] };

        skin.legsRun[Direction.down] = new Sprite[] { legSprites[47], legSprites[48], legSprites[49], legSprites[50] };
        skin.legsRun[Direction.rightDown] = new Sprite[] { legSprites[51], legSprites[52], legSprites[53], legSprites[54] };
        skin.legsRun[Direction.right] = new Sprite[] { legSprites[55], legSprites[56], legSprites[57], legSprites[58] };
        skin.legsRun[Direction.rightUp] = new Sprite[] { legSprites[59], legSprites[60], legSprites[61], legSprites[62] };
        skin.legsRun[Direction.up] = new Sprite[] { legSprites[63], legSprites[64], legSprites[65], legSprites[66] };

        skin.legsClimb[Direction.up] = new Sprite[] { legSprites[69], legSprites[70], legSprites[71], legSprites[72] };
        skin.legsClimb[Direction.rightUp] = new Sprite[] { legSprites[69], legSprites[70], legSprites[71], legSprites[72] };
        skin.legsClimb[Direction.right] = new Sprite[] { legSprites[69], legSprites[70], legSprites[71], legSprites[72] };
        skin.legsClimb[Direction.rightDown] = new Sprite[] { legSprites[69], legSprites[70], legSprites[71], legSprites[72] };
        skin.legsClimb[Direction.down] = new Sprite[] { legSprites[69], legSprites[70], legSprites[71], legSprites[72] };

        skin.legsJump[Direction.down] = new Sprite[] { legSprites[73] };
        skin.legsJump[Direction.rightDown] = new Sprite[] { legSprites[74] };
        skin.legsJump[Direction.right] = new Sprite[] { legSprites[75] };
        skin.legsJump[Direction.rightUp] = new Sprite[] { legSprites[76] };
        skin.legsJump[Direction.up] = new Sprite[] { legSprites[77] };

        skin.legsDead[Direction.down] = new Sprite[] { legSprites[88] };
        skin.legsDead[Direction.rightDown] = new Sprite[] { legSprites[89] };
        skin.legsDead[Direction.right] = new Sprite[] { legSprites[90] };
        skin.legsDead[Direction.rightUp] = new Sprite[] { legSprites[91] };
        skin.legsDead[Direction.up] = new Sprite[] { legSprites[92] };

        skin.legsKeelOver[Direction.down] = new Sprite[] { legSprites[93] };

        // unarmed
        skin.unarmedIdle[Direction.down] = new Sprite[] { torsoSprites[0] };
        skin.unarmedIdle[Direction.rightDown] = new Sprite[] { torsoSprites[1] };
        skin.unarmedIdle[Direction.right] = new Sprite[] { torsoSprites[2] };
        skin.unarmedIdle[Direction.rightUp] = new Sprite[] { torsoSprites[3] };
        skin.unarmedIdle[Direction.up] = new Sprite[] { torsoSprites[4] };

        skin.unarmedWalk[Direction.down] = new Sprite[] { torsoSprites[5], torsoSprites[6], torsoSprites[7], torsoSprites[8] };
        skin.unarmedWalk[Direction.rightDown] = new Sprite[] { torsoSprites[9], torsoSprites[10], torsoSprites[11], torsoSprites[12] };
        skin.unarmedWalk[Direction.right] = new Sprite[] { torsoSprites[13], torsoSprites[14], torsoSprites[15], torsoSprites[16] };
        skin.unarmedWalk[Direction.rightUp] = new Sprite[] { torsoSprites[17], torsoSprites[18], torsoSprites[19], torsoSprites[20] };
        skin.unarmedWalk[Direction.up] = new Sprite[] { torsoSprites[21], torsoSprites[22], torsoSprites[23], torsoSprites[24] };

        skin.unarmedCrouch[Direction.rightDown] = new Sprite[] { torsoSprites[67] }; // 25
        skin.unarmedCrouch[Direction.rightUp] = new Sprite[] { torsoSprites[68] }; // 26

        skin.unarmedCrawl[Direction.down] = new Sprite[] { torsoSprites[27], torsoSprites[28], torsoSprites[29], torsoSprites[30] };
        skin.unarmedCrawl[Direction.rightDown] = new Sprite[] { torsoSprites[31], torsoSprites[32], torsoSprites[33], torsoSprites[34] };
        skin.unarmedCrawl[Direction.right] = new Sprite[] { torsoSprites[35], torsoSprites[36], torsoSprites[37], torsoSprites[38] };
        skin.unarmedCrawl[Direction.rightUp] = new Sprite[] { torsoSprites[39], torsoSprites[40], torsoSprites[41], torsoSprites[42] };
        skin.unarmedCrawl[Direction.up] = new Sprite[] { torsoSprites[43], torsoSprites[44], torsoSprites[45], torsoSprites[46] };

        skin.unarmedRun[Direction.down] = new Sprite[] { torsoSprites[47], torsoSprites[48], torsoSprites[49], torsoSprites[50] };
        skin.unarmedRun[Direction.rightDown] = new Sprite[] { torsoSprites[51], torsoSprites[52], torsoSprites[53], torsoSprites[54] };
        skin.unarmedRun[Direction.right] = new Sprite[] { torsoSprites[55], torsoSprites[56], torsoSprites[57], torsoSprites[58] };
        skin.unarmedRun[Direction.rightUp] = new Sprite[] { torsoSprites[59], torsoSprites[60], torsoSprites[61], torsoSprites[62] };
        skin.unarmedRun[Direction.up] = new Sprite[] { torsoSprites[63], torsoSprites[64], torsoSprites[65], torsoSprites[66] };

        skin.unarmedClimb[Direction.up] = new Sprite[] { torsoSprites[69], torsoSprites[70], torsoSprites[71], torsoSprites[72] };
        skin.unarmedClimb[Direction.rightUp] = new Sprite[] { torsoSprites[69], torsoSprites[70], torsoSprites[71], torsoSprites[72] };
        skin.unarmedClimb[Direction.right] = new Sprite[] { torsoSprites[69], torsoSprites[70], torsoSprites[71], torsoSprites[72] };
        skin.unarmedClimb[Direction.rightDown] = new Sprite[] { torsoSprites[69], torsoSprites[70], torsoSprites[71], torsoSprites[72] };
        skin.unarmedClimb[Direction.down] = new Sprite[] { torsoSprites[69], torsoSprites[70], torsoSprites[71], torsoSprites[72] };

        skin.unarmedJump[Direction.down] = new Sprite[] { torsoSprites[73] };
        skin.unarmedJump[Direction.rightDown] = new Sprite[] { torsoSprites[74] };
        skin.unarmedJump[Direction.right] = new Sprite[] { torsoSprites[75] };
        skin.unarmedJump[Direction.rightUp] = new Sprite[] { torsoSprites[76] };
        skin.unarmedJump[Direction.up] = new Sprite[] { torsoSprites[77] };

        skin.unarmedUse[Direction.down] = new Sprite[] { torsoSprites[78] };
        skin.unarmedUse[Direction.rightDown] = new Sprite[] { torsoSprites[79] };
        skin.unarmedUse[Direction.right] = new Sprite[] { torsoSprites[80] };
        skin.unarmedUse[Direction.rightUp] = new Sprite[] { torsoSprites[81] };
        skin.unarmedUse[Direction.up] = new Sprite[] { torsoSprites[82] };

        skin.unarmedHandsUp[Direction.down] = new Sprite[] { torsoSprites[83] };
        skin.unarmedHandsUp[Direction.rightDown] = new Sprite[] { torsoSprites[84] };
        skin.unarmedHandsUp[Direction.right] = new Sprite[] { torsoSprites[85] };
        skin.unarmedHandsUp[Direction.rightUp] = new Sprite[] { torsoSprites[86] };
        skin.unarmedHandsUp[Direction.up] = new Sprite[] { torsoSprites[87] };

        skin.unarmedDead[Direction.down] = new Sprite[] { torsoSprites[88] };
        skin.unarmedDead[Direction.rightDown] = new Sprite[] { torsoSprites[89] };
        skin.unarmedDead[Direction.right] = new Sprite[] { torsoSprites[90] };
        skin.unarmedDead[Direction.rightUp] = new Sprite[] { torsoSprites[91] };
        skin.unarmedDead[Direction.up] = new Sprite[] { torsoSprites[92] };

        skin.unarmedKeelOver[Direction.down] = new Sprite[] { torsoSprites[93] };
        skin.unarmedCorpse[Direction.down] = new Sprite[] { torsoSprites[94] };

        // pistol

        skin.pistolIdle[Direction.down] = new Sprite[] { pistolSprites[0] };
        skin.pistolIdle[Direction.rightDown] = new Sprite[] { pistolSprites[3] };
        skin.pistolIdle[Direction.right] = new Sprite[] { pistolSprites[6] };
        skin.pistolIdle[Direction.rightUp] = new Sprite[] { pistolSprites[9] };
        skin.pistolIdle[Direction.up] = new Sprite[] { pistolSprites[12] };

        skin.pistolShoot[Direction.down] = new Sprite[] { pistolSprites[1], pistolSprites[2] };
        skin.pistolShoot[Direction.rightDown] = new Sprite[] { pistolSprites[4], pistolSprites[5] };
        skin.pistolShoot[Direction.right] = new Sprite[] { pistolSprites[7], pistolSprites[8] };
        skin.pistolShoot[Direction.rightUp] = new Sprite[] { pistolSprites[10], pistolSprites[11] };
        skin.pistolShoot[Direction.up] = new Sprite[] { pistolSprites[13], pistolSprites[14] };

        skin.pistolReload[Direction.down] = new Sprite[] { pistolSprites[15], pistolSprites[16], pistolSprites[17], pistolSprites[18], pistolSprites[19], pistolSprites[20] };
        skin.pistolReload[Direction.rightDown] = new Sprite[] { pistolSprites[24], pistolSprites[25], pistolSprites[26], pistolSprites[27], pistolSprites[28], pistolSprites[29] };
        skin.pistolReload[Direction.right] = new Sprite[] { pistolSprites[33], pistolSprites[34], pistolSprites[35], pistolSprites[36], pistolSprites[37], pistolSprites[38] };
        skin.pistolReload[Direction.rightUp] = new Sprite[] { pistolSprites[42], pistolSprites[43], pistolSprites[44], pistolSprites[45], pistolSprites[46], pistolSprites[47] };
        skin.pistolReload[Direction.up] = new Sprite[] { pistolSprites[51], pistolSprites[52], pistolSprites[53] };

        skin.pistolRack[Direction.down] = new Sprite[] { pistolSprites[21], pistolSprites[22], pistolSprites[23], pistolSprites[22] };
        skin.pistolRack[Direction.rightDown] = new Sprite[] { pistolSprites[30], pistolSprites[31], pistolSprites[32], pistolSprites[31] };
        skin.pistolRack[Direction.right] = new Sprite[] { pistolSprites[39], pistolSprites[40], pistolSprites[41], pistolSprites[40] };
        skin.pistolRack[Direction.rightUp] = new Sprite[] { pistolSprites[48], pistolSprites[49], pistolSprites[50], pistolSprites[49] };
        skin.pistolRack[Direction.up] = new Sprite[] { pistolSprites[53] };

        skin.pistolRun[Direction.down] = new Sprite[] { pistolSprites[54], pistolSprites[55], pistolSprites[56], pistolSprites[57] };
        skin.pistolRun[Direction.rightDown] = new Sprite[] { pistolSprites[58], pistolSprites[59], pistolSprites[60], pistolSprites[61] };
        skin.pistolRun[Direction.right] = new Sprite[] { pistolSprites[62], pistolSprites[63], pistolSprites[64], pistolSprites[65] };
        skin.pistolRun[Direction.rightUp] = new Sprite[] { pistolSprites[66], pistolSprites[67], pistolSprites[68], pistolSprites[69] };
        skin.pistolRun[Direction.up] = new Sprite[] { pistolSprites[70], pistolSprites[71], pistolSprites[72], pistolSprites[73] };

        skin.pistolHolster[Direction.down] = skin.pistolIdle[Direction.down];
        skin.pistolHolster[Direction.rightDown] = skin.pistolIdle[Direction.rightDown];
        skin.pistolHolster[Direction.right] = skin.pistolIdle[Direction.right];
        skin.pistolHolster[Direction.rightUp] = skin.pistolIdle[Direction.rightUp];
        skin.pistolHolster[Direction.up] = skin.pistolIdle[Direction.up];

        if (name == "Jack") {
            skin.pistolHolster[Direction.down] = new Sprite[] { pistolSprites[0], pistolSprites[74], pistolSprites[75], pistolSprites[76], torsoSprites[0] };
            skin.pistolHolster[Direction.rightDown] = new Sprite[] { pistolSprites[3], pistolSprites[77], pistolSprites[78], pistolSprites[79], torsoSprites[1] };
            skin.pistolHolster[Direction.right] = new Sprite[] { pistolSprites[6], pistolSprites[80], pistolSprites[81], pistolSprites[82], torsoSprites[2] };
            skin.pistolHolster[Direction.rightUp] = new Sprite[] { pistolSprites[9], pistolSprites[83], pistolSprites[84], pistolSprites[85], torsoSprites[3] };
            skin.pistolHolster[Direction.up] = new Sprite[] { pistolSprites[12], pistolSprites[86], pistolSprites[87], pistolSprites[88], torsoSprites[4] };
        }


        // smg

        skin.smgIdle[Direction.down] = new Sprite[] { smgSprites[0] };
        skin.smgIdle[Direction.rightDown] = new Sprite[] { smgSprites[3] };
        skin.smgIdle[Direction.right] = new Sprite[] { smgSprites[6] };
        skin.smgIdle[Direction.rightUp] = new Sprite[] { smgSprites[9] };
        skin.smgIdle[Direction.up] = new Sprite[] { smgSprites[12] };

        skin.smgShoot[Direction.down] = new Sprite[] { smgSprites[1], smgSprites[2] };
        skin.smgShoot[Direction.rightDown] = new Sprite[] { smgSprites[4], smgSprites[5] };
        skin.smgShoot[Direction.right] = new Sprite[] { smgSprites[7], smgSprites[8] };
        skin.smgShoot[Direction.rightUp] = new Sprite[] { smgSprites[10], smgSprites[11] };
        skin.smgShoot[Direction.up] = new Sprite[] { smgSprites[13], smgSprites[13] };

        skin.smgReload[Direction.down] = new Sprite[] { smgSprites[14], smgSprites[15], smgSprites[16], smgSprites[17], smgSprites[18], smgSprites[19] };
        skin.smgReload[Direction.rightDown] = new Sprite[] { smgSprites[22], smgSprites[23], smgSprites[24], smgSprites[25], smgSprites[26], smgSprites[27] };
        skin.smgReload[Direction.right] = new Sprite[] { smgSprites[30], smgSprites[31], smgSprites[32], smgSprites[33], smgSprites[34], smgSprites[35] };
        skin.smgReload[Direction.rightUp] = new Sprite[] { smgSprites[38], smgSprites[39], smgSprites[40], smgSprites[41], smgSprites[42], smgSprites[43] };
        skin.smgReload[Direction.up] = new Sprite[] { smgSprites[46], smgSprites[47], smgSprites[48], smgSprites[49], smgSprites[49], smgSprites[49] };

        skin.smgRack[Direction.down] = new Sprite[] { smgSprites[19], smgSprites[20], smgSprites[21], smgSprites[20] };
        skin.smgRack[Direction.rightDown] = new Sprite[] { smgSprites[27], smgSprites[28], smgSprites[29], smgSprites[28] };
        skin.smgRack[Direction.right] = new Sprite[] { smgSprites[35], smgSprites[36], smgSprites[37], smgSprites[36] };
        skin.smgRack[Direction.rightUp] = new Sprite[] { smgSprites[43], smgSprites[44], smgSprites[45], smgSprites[44] };
        skin.smgRack[Direction.up] = new Sprite[] { smgSprites[49] };

        skin.smgRun[Direction.down] = new Sprite[] { smgSprites[50], smgSprites[51], smgSprites[52], smgSprites[53] };
        skin.smgRun[Direction.rightDown] = new Sprite[] { smgSprites[54], smgSprites[55], smgSprites[56], smgSprites[57] };
        skin.smgRun[Direction.right] = new Sprite[] { smgSprites[58], smgSprites[59], smgSprites[60], smgSprites[61] };
        skin.smgRun[Direction.rightUp] = new Sprite[] { smgSprites[62], smgSprites[63], smgSprites[64], smgSprites[65] };
        skin.smgRun[Direction.up] = new Sprite[] { smgSprites[66], smgSprites[67], smgSprites[68], smgSprites[69] };

        skin.smgHolster[Direction.down] = skin.smgIdle[Direction.down];
        skin.smgHolster[Direction.rightDown] = skin.smgIdle[Direction.rightDown];
        skin.smgHolster[Direction.right] = skin.smgIdle[Direction.right];
        skin.smgHolster[Direction.rightUp] = skin.smgIdle[Direction.rightUp];
        skin.smgHolster[Direction.up] = skin.smgIdle[Direction.up];

        if (name == "Jack") {
            skin.smgHolster[Direction.down] = new Sprite[] { smgSprites[1], pistolSprites[74], pistolSprites[75], smgSprites[70], pistolSprites[74] };
            skin.smgHolster[Direction.rightDown] = new Sprite[] { smgSprites[4], pistolSprites[77], pistolSprites[78], smgSprites[71], pistolSprites[77] };
            skin.smgHolster[Direction.right] = new Sprite[] { smgSprites[7], pistolSprites[80], pistolSprites[81], smgSprites[72], pistolSprites[80] };
            skin.smgHolster[Direction.rightUp] = new Sprite[] { smgSprites[10], pistolSprites[83], pistolSprites[84], smgSprites[73], pistolSprites[83] };
            skin.smgHolster[Direction.up] = new Sprite[] { smgSprites[13], pistolSprites[86], pistolSprites[87], smgSprites[74], pistolSprites[86] };
        }

        // shotgun

        skin.shotgunIdle[Direction.down] = new Sprite[] { shotgunSprites[0] };
        skin.shotgunIdle[Direction.rightDown] = new Sprite[] { shotgunSprites[5] };
        skin.shotgunIdle[Direction.right] = new Sprite[] { shotgunSprites[10] };
        skin.shotgunIdle[Direction.rightUp] = new Sprite[] { shotgunSprites[14] };
        skin.shotgunIdle[Direction.up] = new Sprite[] { shotgunSprites[18] };

        skin.shotgunShoot[Direction.down] = new Sprite[] { shotgunSprites[1], shotgunSprites[2] };
        skin.shotgunShoot[Direction.rightDown] = new Sprite[] { shotgunSprites[6], shotgunSprites[7] };
        skin.shotgunShoot[Direction.right] = new Sprite[] { shotgunSprites[11], shotgunSprites[12] };
        skin.shotgunShoot[Direction.rightUp] = new Sprite[] { shotgunSprites[15], shotgunSprites[16] };
        skin.shotgunShoot[Direction.up] = new Sprite[] { shotgunSprites[19] };

        skin.shotgunRack[Direction.down] = new Sprite[] { shotgunSprites[3], shotgunSprites[4], shotgunSprites[3] };
        skin.shotgunRack[Direction.rightDown] = new Sprite[] { shotgunSprites[8], shotgunSprites[9], shotgunSprites[8] };
        skin.shotgunRack[Direction.right] = new Sprite[] { shotgunSprites[11], shotgunSprites[13], shotgunSprites[11] };
        skin.shotgunRack[Direction.rightUp] = new Sprite[] { shotgunSprites[15], shotgunSprites[17], shotgunSprites[15] };
        skin.shotgunRack[Direction.up] = new Sprite[] { shotgunSprites[19] };

        skin.shotgunReload[Direction.down] = new Sprite[] { shotgunSprites[20], shotgunSprites[21], shotgunSprites[22], shotgunSprites[23], shotgunSprites[24], shotgunSprites[25] };
        skin.shotgunReload[Direction.rightDown] = new Sprite[] { shotgunSprites[26], shotgunSprites[27], shotgunSprites[28], shotgunSprites[29], shotgunSprites[30], shotgunSprites[31] };
        skin.shotgunReload[Direction.right] = new Sprite[] { shotgunSprites[32], shotgunSprites[33], shotgunSprites[34], shotgunSprites[35], shotgunSprites[36], shotgunSprites[37] };
        skin.shotgunReload[Direction.rightUp] = new Sprite[] { shotgunSprites[38], shotgunSprites[39], shotgunSprites[40], shotgunSprites[41], shotgunSprites[42], shotgunSprites[43] };
        skin.shotgunReload[Direction.up] = new Sprite[] { shotgunSprites[44], shotgunSprites[45], shotgunSprites[46], shotgunSprites[47], shotgunSprites[48], shotgunSprites[49] };

        skin.shotgunHolster[Direction.down] = skin.shotgunIdle[Direction.down];
        skin.shotgunHolster[Direction.rightDown] = skin.shotgunIdle[Direction.rightDown];
        skin.shotgunHolster[Direction.right] = skin.shotgunIdle[Direction.right];
        skin.shotgunHolster[Direction.rightUp] = skin.shotgunIdle[Direction.rightUp];
        skin.shotgunHolster[Direction.up] = skin.shotgunIdle[Direction.up];

        if (name == "Jack") {
            skin.shotgunHolster[Direction.down] = new Sprite[] { shotgunSprites[1], pistolSprites[74], pistolSprites[75], shotgunSprites[50], pistolSprites[74] };
            skin.shotgunHolster[Direction.rightDown] = new Sprite[] { shotgunSprites[6], pistolSprites[74], pistolSprites[78], shotgunSprites[51], pistolSprites[74] };
            skin.shotgunHolster[Direction.right] = new Sprite[] { shotgunSprites[11], pistolSprites[80], pistolSprites[81], shotgunSprites[52], pistolSprites[80] };
            skin.shotgunHolster[Direction.rightUp] = new Sprite[] { shotgunSprites[15], pistolSprites[83], pistolSprites[84], shotgunSprites[53], pistolSprites[83] };
            skin.shotgunHolster[Direction.up] = new Sprite[] { shotgunSprites[19], pistolSprites[86], pistolSprites[87], shotgunSprites[54], pistolSprites[86] };
        }


        // rifle

        skin.rifleIdle[Direction.down] = new Sprite[] { rifleSprites[0] };
        skin.rifleIdle[Direction.rightDown] = new Sprite[] { rifleSprites[3] };
        skin.rifleIdle[Direction.right] = new Sprite[] { rifleSprites[6] };
        skin.rifleIdle[Direction.rightUp] = new Sprite[] { rifleSprites[9] };
        skin.rifleIdle[Direction.up] = new Sprite[] { rifleSprites[12] };

        skin.rifleShoot[Direction.down] = new Sprite[] { rifleSprites[1], rifleSprites[2] };
        skin.rifleShoot[Direction.rightDown] = new Sprite[] { rifleSprites[4], rifleSprites[5] };
        skin.rifleShoot[Direction.right] = new Sprite[] { rifleSprites[7], rifleSprites[8] };
        skin.rifleShoot[Direction.rightUp] = new Sprite[] { rifleSprites[10], rifleSprites[11] };
        skin.rifleShoot[Direction.up] = new Sprite[] { rifleSprites[13], rifleSprites[13] };

        skin.rifleReload[Direction.down] = new Sprite[] { rifleSprites[14], rifleSprites[15], rifleSprites[16], rifleSprites[17], rifleSprites[18], rifleSprites[19] };
        skin.rifleReload[Direction.rightDown] = new Sprite[] { rifleSprites[22], rifleSprites[23], rifleSprites[24], rifleSprites[25], rifleSprites[26], rifleSprites[27] };
        skin.rifleReload[Direction.right] = new Sprite[] { rifleSprites[30], rifleSprites[31], rifleSprites[32], rifleSprites[33], rifleSprites[34], rifleSprites[35] };
        skin.rifleReload[Direction.rightUp] = new Sprite[] { rifleSprites[38], rifleSprites[39], rifleSprites[40], rifleSprites[41], rifleSprites[42], rifleSprites[43] };
        skin.rifleReload[Direction.up] = new Sprite[] { rifleSprites[46], rifleSprites[47], rifleSprites[48], rifleSprites[49], rifleSprites[49], rifleSprites[49] };

        skin.rifleRack[Direction.down] = new Sprite[] { rifleSprites[19], rifleSprites[20], rifleSprites[21], rifleSprites[20] };
        skin.rifleRack[Direction.rightDown] = new Sprite[] { rifleSprites[27], rifleSprites[28], rifleSprites[29], rifleSprites[28] };
        skin.rifleRack[Direction.right] = new Sprite[] { rifleSprites[35], rifleSprites[36], rifleSprites[37], rifleSprites[36] };
        skin.rifleRack[Direction.rightUp] = new Sprite[] { rifleSprites[43], rifleSprites[44], rifleSprites[45], rifleSprites[44] };
        skin.rifleRack[Direction.up] = new Sprite[] { rifleSprites[49] };

        skin.rifleHolster[Direction.down] = skin.rifleIdle[Direction.down];
        skin.rifleHolster[Direction.rightDown] = skin.rifleIdle[Direction.rightDown];
        skin.rifleHolster[Direction.right] = skin.rifleIdle[Direction.right];
        skin.rifleHolster[Direction.rightUp] = skin.rifleIdle[Direction.rightUp];
        skin.rifleHolster[Direction.up] = skin.rifleIdle[Direction.up];

        if (name == "Jack") {
            skin.rifleHolster[Direction.down] = new Sprite[] { rifleSprites[1], pistolSprites[74], pistolSprites[75], rifleSprites[50], pistolSprites[74] };
            skin.rifleHolster[Direction.rightDown] = new Sprite[] { rifleSprites[4], pistolSprites[77], pistolSprites[78], rifleSprites[51], pistolSprites[77] };
            skin.rifleHolster[Direction.right] = new Sprite[] { rifleSprites[7], pistolSprites[80], pistolSprites[81], rifleSprites[52], pistolSprites[80] };
            skin.rifleHolster[Direction.rightUp] = new Sprite[] { rifleSprites[10], pistolSprites[83], pistolSprites[84], rifleSprites[53], pistolSprites[83] };
            skin.rifleHolster[Direction.up] = new Sprite[] { rifleSprites[13], pistolSprites[86], pistolSprites[87], rifleSprites[54], pistolSprites[86] };
        }

        if (name == "Jack") {
            skin.swordIdle[Direction.down] = new Sprite[] { swordSprites[5] };
            skin.swordIdle[Direction.rightDown] = new Sprite[] { swordSprites[11] };
            skin.swordIdle[Direction.right] = new Sprite[] { swordSprites[17] };
            skin.swordIdle[Direction.rightUp] = new Sprite[] { swordSprites[23] };
            skin.swordIdle[Direction.up] = new Sprite[] { swordSprites[29] };

            skin.swordHolster[Direction.down] = new Sprite[] { swordSprites[0], swordSprites[1], swordSprites[2], swordSprites[3], swordSprites[4] };
            skin.swordHolster[Direction.rightDown] = new Sprite[] { swordSprites[6], swordSprites[7], swordSprites[8], swordSprites[9], swordSprites[10] };
            skin.swordHolster[Direction.right] = new Sprite[] { swordSprites[12], swordSprites[13], swordSprites[14], swordSprites[15], swordSprites[16] };
            skin.swordHolster[Direction.rightUp] = new Sprite[] { swordSprites[18], swordSprites[19], swordSprites[20], swordSprites[21], swordSprites[22] };
            skin.swordHolster[Direction.up] = new Sprite[] { swordSprites[24], swordSprites[25], swordSprites[26], swordSprites[27], swordSprites[28] };

            skin.swordSwing[Direction.down] = new Sprite[] { swordSprites[30], swordSprites[31], swordSprites[5] };
            skin.swordSwing[Direction.rightDown] = new Sprite[] { swordSprites[32], swordSprites[33], swordSprites[11] };
            skin.swordSwing[Direction.right] = new Sprite[] { swordSprites[34], swordSprites[35], swordSprites[17] };
            skin.swordSwing[Direction.rightUp] = new Sprite[] { swordSprites[36], swordSprites[37], swordSprites[23] };
            skin.swordSwing[Direction.up] = new Sprite[] { swordSprites[38], swordSprites[39], swordSprites[29] };
        }

        if (name == "Jack") {
            skin.fenceCutterIdle[Direction.down] = new Sprite[] { fenceCutterSprites[0] };
            skin.fenceCutterIdle[Direction.rightDown] = new Sprite[] { fenceCutterSprites[3] };
            skin.fenceCutterIdle[Direction.right] = new Sprite[] { fenceCutterSprites[6] };
            skin.fenceCutterIdle[Direction.rightUp] = new Sprite[] { fenceCutterSprites[9] };
            skin.fenceCutterIdle[Direction.up] = new Sprite[] { fenceCutterSprites[12] };

            skin.fenceCutterCut[Direction.down] = new Sprite[] { fenceCutterSprites[1], fenceCutterSprites[2] };
            skin.fenceCutterCut[Direction.rightDown] = new Sprite[] { fenceCutterSprites[4], fenceCutterSprites[5] };
            skin.fenceCutterCut[Direction.right] = new Sprite[] { fenceCutterSprites[7], fenceCutterSprites[8] };
            skin.fenceCutterCut[Direction.rightUp] = new Sprite[] { fenceCutterSprites[10], fenceCutterSprites[11] };
            skin.fenceCutterCut[Direction.up] = new Sprite[] { fenceCutterSprites[13], fenceCutterSprites[14] };
        }

        // data
        skin.unarmedSpriteData = torsoSpriteData.ToArray();
        skin.pistolSpriteData = pistolSpriteData.ToArray();
        skin.smgSpriteData = smgSpriteData.ToArray();
        skin.rifleSpriteData = rifleSpriteData.ToArray();
        skin.shotgunSpriteData = shotgunSpriteData.ToArray();

        skin.allSpriteData = new Dictionary<string, SpriteData>();
        AddSpriteDataToDictionary(skin, torsoSpriteData, "Torso");
        AddSpriteDataToDictionary(skin, pistolSpriteData, "pistol");
        AddSpriteDataToDictionary(skin, smgSpriteData, "smg");
        AddSpriteDataToDictionary(skin, shotgunSpriteData, "shotgun");
        AddSpriteDataToDictionary(skin, rifleSpriteData, "rifle");
        if (name == "Jack") {
            List<SpriteData> swordSpriteData = LoadTorsoSpriteData(name, "Sword");
            skin.shotgunSpriteData = shotgunSpriteData.ToArray();
            skin.swordSpriteData = swordSpriteData.ToArray();
            AddSpriteDataToDictionary(skin, swordSpriteData, "Sword");
            AddSpriteDataToDictionary(skin, swordSpriteData, "Fence-cutter");
        }

        return skin;
    }

    static void AddSpriteDataToDictionary(Skin skin, List<SpriteData> list, string prefix) {
        int i = 0;
        foreach (SpriteData data in list) {
            string spritename = $"{prefix}_{i}";
            skin.allSpriteData[spritename] = data;
            i++;
        }
    }

    public Octet<Sprite[]> GetCurrentLegsOctet(LegsAnimation.State state, AnimationInput input) {
        if (input.state == CharacterState.keelOver) {
            return legsKeelOver;
        } else if (input.hitState == HitState.dead) {
            return legsDead;
        } else if (input.hitState == HitState.zapped) {
            return skeletonSprites;
        } else if (input.state == CharacterState.jumpPrep || input.state == CharacterState.landStun) {
            return legsCrouch;
        } else if (input.state == CharacterState.superJump || state == LegsAnimation.State.jump) {
            return legsJump;
        } else if (input.isProne && !(input.wallPressTimer > 0 || input.state == CharacterState.wallPress)) {
            return legsCrawl;
        }

        switch (state) {
            case LegsAnimation.State.walk:
                return legsWalk;
            case LegsAnimation.State.crawl:
                return legsCrawl;
            case LegsAnimation.State.crouch:
                return legsCrouch;
            case LegsAnimation.State.run:
                return legsRun;
            case LegsAnimation.State.climb:
                return legsClimb;
            default:
            case LegsAnimation.State.idle:
                return legsIdle;
        }
    }

    public Octet<Sprite[]> GetCurrentTorsoOctet(AnimationInput input) {
        if (input.state == CharacterState.keelOver) {
            return unarmedKeelOver;
        } else if (input.hitState == HitState.dead) {
            return unarmedDead;
        } else if (input.hitState == HitState.zapped) {
            return skeletonSprites;
        } else if (input.isClimbing) {
            return unarmedClimb;
        } else if (input.state == CharacterState.superJump) {
            return unarmedJump;
        } else if (input.state == CharacterState.landStun || input.state == CharacterState.jumpPrep) {
            return gunCrouchSprites(input.gunInput.gunType);
        } else if (input.isProne && !(input.wallPressTimer > 0 || input.state == CharacterState.wallPress)) {
            return unarmedCrawl;
        } else if (input.armsRaised) {
            return unarmedHandsUp;
        } else if (input.wavingArm) {
            return unarmedUse;
        } else if (input.itemInput.activeItem != null) {
            if (input.itemInput.activeItem is RocketLauncherItem) {
                return unarmedUse;
            } else if (input.itemInput.activeItem is FenceCutterItem) {
                switch (input.itemInput.state) {
                    case ItemHandler.State.idle:
                        return fenceCutterIdle;
                    case ItemHandler.State.inUse:
                        return fenceCutterCut;
                }
            }
        }

        // gun states
        if (input.gunInput.gunType == GunType.sword || input.gunInput.fromGunType == GunType.sword) {
            switch (input.gunInput.gunState) {
                case GunHandler.GunStateEnum.holstering:
                    Debug.Log("setting sprites: swordholster");
                    return swordHolster;
                case GunHandler.GunStateEnum.shooting:
                    return swordSwing;
                default:
                case GunHandler.GunStateEnum.idle:
                    return swordIdle;
            }
        } else {
            switch (input.gunInput.gunState) {
                case GunHandler.GunStateEnum.holstering:
                    return holsterSprites(input.gunInput);
                case GunHandler.GunStateEnum.reloading:
                    return reloadSprites(input.gunInput.gunType);
                case GunHandler.GunStateEnum.racking:
                    return gunRackSprites(input.gunInput.gunType);
                case GunHandler.GunStateEnum.shooting:
                    return shootSprites(input.gunInput.gunType);
                default:
                    if (input.gunInput.aimWeapon) {
                        return shootSprites(input.gunInput.gunType);
                    } else if (input.isMoving) {
                        if (input.isCrouching) {
                            // crawl
                            return unarmedCrawl;
                        } else if (input.isRunning) {
                            // running
                            return gunRunSprites(input.gunInput.gunType);
                        } else {
                            // walk
                            return gunWalkSprites(input.gunInput.gunType);
                        }
                    } else { // not moving
                        if (input.isCrouching) {
                            return gunCrouchSprites(input.gunInput.gunType);
                        } else return gunIdleSprites(input.gunInput.gunType);
                    }
            }
        }
    }


    public static void SaveSpriteData(string skinName, List<SpriteData> spriteData, string sheetType) {
        XmlSerializer serializer = new XmlSerializer(typeof(List<SpriteData>));
        string path = Path.Combine(Application.dataPath, "Resources", Skin.PathToSkinDirectory(skinName), $"{sheetType}SpriteData.xml");
        Debug.Log($"saving torso sprite data to {path}...");
        if (File.Exists(path)) {
            File.Delete(path);
        }
        using (FileStream sceneStream = File.Create(path)) {
            serializer.Serialize(sceneStream, spriteData);
        }
    }
    public static void SaveLegSpriteData(string skinName, List<SpriteDataLegs> spriteData) {
        XmlSerializer serializer = new XmlSerializer(typeof(List<SpriteDataLegs>));
        string path = Path.Combine(Application.dataPath, "Resources", Skin.PathToSkinDirectory(skinName), $"LegSpriteData.xml");
        Debug.Log($"saving leg sprite data to {path}...");
        if (File.Exists(path)) {
            File.Delete(path);
        }
        using (FileStream sceneStream = File.Create(path)) {
            serializer.Serialize(sceneStream, spriteData);
        }
    }

    public static List<SpriteData> LoadTorsoSpriteData(string skinName, string sheetType) {
        XmlSerializer serializer = new XmlSerializer(typeof(List<SpriteData>));
        // string path = Path.Combine(Application.dataPath, "Resources", Skin.PathToSkinDirectory(skinName), $"{sheetType}SpriteData.xml");
        string path = Path.Combine(Skin.PathToSkinDirectory(skinName), $"{sheetType}SpriteData");
        TextAsset textAsset = Resources.Load<TextAsset>(path) as TextAsset;
        if (textAsset != null) {
            using (var reader = new System.IO.StringReader(textAsset.text)) {
                return serializer.Deserialize(reader) as List<SpriteData>;
            }
        } else {
            Debug.LogError($"sprite data file not found: {path}, skinName: {skinName}, sheetType: {sheetType}");
            return null;
        }
    }
    public static List<SpriteDataLegs> LoadLegSpriteData(string skinName) {
        XmlSerializer serializer = new XmlSerializer(typeof(List<SpriteDataLegs>));
        // string path = Path.Combine(Application.dataPath, "Resources", Skin.PathToSkinDirectory(skinName), $"{sheetType}SpriteData.xml");
        string path = Path.Combine(Skin.PathToSkinDirectory(skinName), $"LegSpriteData");
        TextAsset textAsset = Resources.Load<TextAsset>(path) as TextAsset;
        if (textAsset != null) {
            using (var reader = new System.IO.StringReader(textAsset.text)) {
                return serializer.Deserialize(reader) as List<SpriteDataLegs>;
            }
        } else {
            Debug.LogError($"leg sprite data file not found: {path}, skinName: {skinName}");
            return null;
        }
    }
}
