using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Layer { def, shell, obj, skybox, shadowprobe, bulletPassThrough, interactive, interactor }
public class LayerUtil {
    private static Dictionary<Layer, string> layerNames = new Dictionary<Layer, string>{
        {Layer.def, "Default"},
        {Layer.shell, "Shell"},
        {Layer.obj, "Object"},
        {Layer.skybox, "skybox"},
        {Layer.shadowprobe, "shadowprobe"},
        {Layer.bulletPassThrough, "bulletPassThrough"},
        {Layer.interactive, "interactive"},
        {Layer.interactor, "interactor"}
    };

    public static LayerMask GetMask(params Layer[] layers) {
        var x = layers.Select(layer => layerNames[layer]).ToArray();
        return LayerMask.GetMask(x);
    }

}
