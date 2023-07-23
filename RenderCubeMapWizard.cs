#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
using UnityEngine;

public class RenderCubemapWizard : ScriptableWizard {
    public Transform renderFromPosition;
    public Cubemap cubemap;

    void OnWizardUpdate() {
        string helpString = "Select transform to render from and cubemap to render into";
        bool isValid = (renderFromPosition != null) && (cubemap != null);
    }

    void OnWizardCreate() {
        // create temporary camera for rendering
        GameObject go = new GameObject("CubemapCamera");
        Camera camera = go.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.Skybox;
        Material skyMat = Resources.Load("materials/skyline/cityskybox") as Material;
        RenderSettings.skybox = skyMat;

        Skybox skyBox = go.AddComponent<Skybox>();
        skyBox.material = skyMat;
        // camera
        // place it on the object
        go.transform.position = renderFromPosition.position;
        go.transform.rotation = Quaternion.identity;
        // render into cubemap
        go.GetComponent<Camera>().RenderToCubemap(cubemap);

        // destroy temporary camera
        DestroyImmediate(go);
    }

    [MenuItem("GameObject/Render into Cubemap")]
    static void RenderCubemap() {
        ScriptableWizard.DisplayWizard<RenderCubemapWizard>(
            "Render cubemap", "Render!");
    }
}
#endif