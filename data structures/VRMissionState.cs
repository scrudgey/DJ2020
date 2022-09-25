using UnityEngine;

public record VRMissionState {
    public VRMissionTemplate template;
    public VRMissionMutableData data;
    public static VRMissionState DefaultData() => new VRMissionState {
        template = VRMissionTemplate.Default(),
        data = VRMissionMutableData.Empty()
    };

    public static VRMissionState Instantiate(VRMissionTemplate template) => new VRMissionState {
        template = template,
        data = VRMissionMutableData.Empty()
    };
}