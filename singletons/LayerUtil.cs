using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public enum Layer { def, shell, obj, skybox, shadowprobe, bulletPassThrough, interactive, interactor, bulletOnly, clearsighterHide, attackSurface, laser, clearsighterBlock, linerender }
public enum NavLayer { def, KeyId1, KeyId2 }

public class LayerUtil {
    private static Dictionary<Layer, string> layerNames = new Dictionary<Layer, string>{
        {Layer.def, "Default"},
        {Layer.shell, "Shell"},
        {Layer.obj, "Object"},
        {Layer.skybox, "skybox"},
        {Layer.shadowprobe, "shadowprobe"},
        {Layer.bulletPassThrough, "bulletPassThrough"},
        {Layer.interactive, "interactive"},
        {Layer.interactor, "interactor"},
        {Layer.bulletOnly, "bulletOnly"},
        {Layer.clearsighterHide, "clearsighterHide"},
        {Layer.attackSurface, "attackSurface"},
        {Layer.laser, "laser"},
        {Layer.clearsighterBlock, "clearsighterBlock"},
        {Layer.linerender, "linerender"}
    };

    private static Dictionary<NavLayer, string> navLayerNames = new Dictionary<NavLayer, string>{
        {NavLayer.def, "Walkable"},
        {NavLayer.KeyId1, "KeyId1"},
        {NavLayer.KeyId2, "KeyId2"},
    };

    public static LayerMask GetLayerMask(params Layer[] layers) {
        var x = layers.Select(layer => layerNames[layer]).ToArray();
        return LayerMask.GetMask(x);
    }
    public static int GetLayer(Layer layer) => LayerMask.NameToLayer(layerNames[layer]);
    public static int GetNavLayerMask(params NavLayer[] layers) => layers
            .Select(layer => navLayerNames[layer])
            .Select(layerName => NavMesh.GetAreaFromName(layerName))
            .Select(layerIndex => 1 << layerIndex)
            .Aggregate((mask1, mask2) => mask1 | mask2);

    public static int KeySetToNavLayerMask(HashSet<int> keyIds) {
        HashSet<NavLayer> totalNavLayer = keyIds.Select(keyId => KeyIdToNavLayer(keyId)).ToHashSet();
        totalNavLayer.Add(NavLayer.def);
        return GetNavLayerMask(totalNavLayer.ToArray());
    }

    public static NavLayer KeyIdToNavLayer(int keyId) => keyId switch {
        1 => NavLayer.KeyId1,
        2 => NavLayer.KeyId2,
        _ => NavLayer.def
    };

}
