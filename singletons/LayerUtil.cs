using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum Layer { def, shell, obj, skybox, shadowprobe, bulletPassThrough }
public class LayerUtil {
    private static Dictionary<Layer, string> layerNames = new Dictionary<Layer, string>{
        {Layer.def, "Default"},
        {Layer.shell, "Shell"},
        {Layer.obj, "Object"},
        {Layer.skybox, "skybox"},
        {Layer.shadowprobe, "shadowprobe"},
        {Layer.bulletPassThrough, "bulletPassThrough"}
    };

    public static LayerMask GetMask(params Layer[] layers) {
        var x = layers.Select(layer => layerNames[layer]).ToArray();
        return LayerMask.GetMask(x);
    }

}
