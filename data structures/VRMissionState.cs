using UnityEngine;

public record VRMissionState {
    public VRMissionTemplate template;
    public VRMissionDelta data;

    public static VRMissionState Instantiate(VRMissionTemplate template) => new VRMissionState {
        template = template,
        data = VRMissionDelta.Empty()
    };
}