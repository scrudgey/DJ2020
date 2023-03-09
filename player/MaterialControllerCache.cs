
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class MaterialControllerCache {
    public CharacterCamera camera;
    public Dictionary<Transform, MaterialController> controllers = new Dictionary<Transform, MaterialController>();
    public MaterialControllerCache(CharacterCamera camera) {
        this.camera = camera;
    }
    public MaterialController get(Collider key) {
        if (controllers.ContainsKey(key.transform.root)) {
            return controllers[key.transform.root];
        } else {
            MaterialController controller = new MaterialController(key, camera);
            if (controller.childRenderers.Count == 0) {
                controller = null;
            }
            controllers[key.transform.root] = controller;
            return controller;
        }
    }
}