using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skin {
    public Octet<Sprite[]> legsIdle = new Octet<Sprite[]>();
    public Octet<Sprite[]> legsWalk = new Octet<Sprite[]>();
    public Octet<Sprite[]> legsCrouch = new Octet<Sprite[]>();

    // unarmed
    public Octet<Sprite[]> unarmedIdle = new Octet<Sprite[]>();
    public Octet<Sprite[]> unarmedWalk = new Octet<Sprite[]>();

    // pistol
    public Octet<Sprite[]> pistolIdle = new Octet<Sprite[]>();
    public Octet<Sprite[]> pistolShoot = new Octet<Sprite[]>();

    // smg
    public Octet<Sprite[]> smgIdle = new Octet<Sprite[]>();
    public Octet<Sprite[]> smgShoot = new Octet<Sprite[]>();

    // shotgun
    public Octet<Sprite[]> shotgunIdle = new Octet<Sprite[]>();
    public Octet<Sprite[]> shotgunShoot = new Octet<Sprite[]>();
    public Octet<Sprite[]> shotgunRack = new Octet<Sprite[]>();

    public Octet<Sprite[]> idleSprites(GunType type) {
        switch (type) {
            case GunType.smg:
                return smgIdle;
            case GunType.shotgun:
                return shotgunIdle;
            case GunType.pistol:
                return pistolIdle;
            default:
            case GunType.unarmed:
                return unarmedIdle;
        }
    }
    public Octet<Sprite[]> walkSprites(GunType type) {
        switch (type) {
            case GunType.smg:
                return smgIdle;
            case GunType.shotgun:
                return shotgunIdle;
            case GunType.pistol:
                return pistolIdle;
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
            default:
            case GunType.pistol:
                return pistolShoot;
        }
    }

    public static Skin LoadSkin(string name) {
        Sprite[] legSprites = Resources.LoadAll<Sprite>($"sprites/spritesheets/{name}/legs") as Sprite[];
        Sprite[] torsoSprites = Resources.LoadAll<Sprite>($"sprites/spritesheets/{name}/torso") as Sprite[];
        Sprite[] pistolSprites = Resources.LoadAll<Sprite>($"sprites/spritesheets/{name}/pistol") as Sprite[];
        Sprite[] smgSprites = Resources.LoadAll<Sprite>($"sprites/spritesheets/{name}/smg") as Sprite[];
        Sprite[] shotgunSprites = Resources.LoadAll<Sprite>($"sprites/spritesheets/{name}/shotgun") as Sprite[];

        Skin skin = new Skin();

        // TODO: clean this up a bit


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

        // TODO: set crouch sprite
        // TODO: set crawl sprite

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

        // pistol

        skin.pistolIdle[Direction.down] = new Sprite[] { pistolSprites[0] };
        skin.pistolIdle[Direction.rightDown] = new Sprite[] { pistolSprites[3] };
        skin.pistolIdle[Direction.right] = new Sprite[] { pistolSprites[6] };
        skin.pistolIdle[Direction.rightUp] = new Sprite[] { pistolSprites[9] };
        skin.pistolIdle[Direction.up] = new Sprite[] { pistolSprites[12] };

        skin.pistolShoot[Direction.down] = new Sprite[] { pistolSprites[1] };
        skin.pistolShoot[Direction.rightDown] = new Sprite[] { pistolSprites[4] };
        skin.pistolShoot[Direction.right] = new Sprite[] { pistolSprites[7] };
        skin.pistolShoot[Direction.rightUp] = new Sprite[] { pistolSprites[10] };
        skin.pistolShoot[Direction.up] = new Sprite[] { pistolSprites[13] };

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

        return skin;
    }
}
