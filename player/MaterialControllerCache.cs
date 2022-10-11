
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class MaterialControllerCache {
    public CharacterCamera camera;
    public Dictionary<Collider, MaterialController> controllers = new Dictionary<Collider, MaterialController>();
    public MaterialControllerCache(CharacterCamera camera) {
        this.camera = camera;
    }
    public MaterialController get(Collider key) {
        if (controllers.ContainsKey(key)) {
            return controllers[key];
        } else {
            MaterialController controller = new MaterialController(key, camera);
            if (controller.childRenderers.Count == 0) {
                controller = null;
            }
            controllers[key] = controller;
            return controller;
        }
    }
}