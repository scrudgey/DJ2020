public record InputProfile {
    public bool allowCameraControl;
    public bool allowBurglarInterface;
    public bool allowPlayerFireInput;
    public bool allowPlayerMovement;
    public bool allowePlayerItemSelect;
    public bool allowOverlayControl;
    public bool allowBurglarButton;

    public static readonly InputProfile allowAll = new InputProfile {
        allowCameraControl = true,
        allowPlayerFireInput = true,
        allowPlayerMovement = true,
        allowePlayerItemSelect = true,
        allowOverlayControl = true,
        allowBurglarInterface = true,
        allowBurglarButton = true
    };

    public static readonly InputProfile allowNone = new InputProfile {
        allowCameraControl = false,
        allowPlayerFireInput = false,
        allowPlayerMovement = false,
        allowePlayerItemSelect = false,
        allowOverlayControl = false,
        allowBurglarInterface = false,
        allowBurglarButton = false
    };
}