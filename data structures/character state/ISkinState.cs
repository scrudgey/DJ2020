using UnityEngine;
public interface ISkinState {
    string legSkin { get; set; }
    string bodySkin { get; set; }
    string headSkin { get; set; }

    public void ApplySkinState(GameObject playerObject) {
        foreach (ISkinStateLoader skinLoader in playerObject.GetComponentsInChildren<ISkinStateLoader>()) {
            skinLoader.LoadSkinState(this);
        }
    }
}