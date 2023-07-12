using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GamePhase { none, levelPlay, vrMission, mainMenu, plan, afteraction, world }
public enum MenuType { none, console, dialogue, VRMissionFinish, escapeMenu, missionFail, missionSelect, gunshop, itemshop, lootshop, mainEscapeMenu, barShop, VREscapeMenu, importerShop, gunModShop, payDataShop, medicalShop }
public enum OverlayType { none, power, cyber, alarm }
public enum CursorType { none, gun, pointer, hand }
public enum InputMode { none, gun, cyber, aim, wallpressAim, burglar }
public struct PointerData {
    public Texture2D mouseCursor;
    public Vector2 hotSpot;
    public CursorMode cursorMode;
}
public partial class GameManager : Singleton<GameManager> {
    public GameData gameData;
    public AudioSource audioSource;
    public GameObject playerObject;
    public Collider playerCollider;
    public PlayerOutlineHandler playerOutlineHandler;
    public LightLevelProbe playerLightLevelProbe;
    public NeoClearsighter clearSighter2;

    [Header("input actions")]
    // UI input
    public InputActionReference showConsole;
    public InputActionReference escapeAction;


    // UI callbacks
    // public static Action OnMenuClosed;
    public static Action<GameObject> OnFocusChanged;
    // public static Action<MenuType> OnMenuChange;
    public static Action<PowerGraph> OnPowerGraphChange;
    public static Action<CyberGraph> OnCyberGraphChange;
    public static Action<AlarmGraph> OnAlarmGraphChange;
    public static Action<OverlayType, bool> OnOverlayChange;
    public static Action<InputMode, InputMode> OnInputModeChange; // TODO: legit? should be camera state change?
    public static Action<CursorType> OnCursorTypeChange;
    public static Action<String> OnCaptionChange;
    public static Action<PlayerState> OnEyeVisibilityChange;
    // UI state
    private bool toggleConsoleThisFrame;
    private bool escapePressedThisFrame;

    private bool nextOverlayThisFrame;
    private bool previousOverlayThisFrame;
    public MenuType activeMenuType;
    public OverlayType activeOverlayType = OverlayType.none;
    private CursorType _cursorType;
    bool resetMouseControl;
    public BurgleTargetData activeBurgleTargetData;
    public Vector3 playerPosition;

    public CursorType cursorType {
        get { return _cursorType; }
        set {
            if (_cursorType != value)
                SetCursor(value);
            _cursorType = value;
        }
    }
    private InputMode _inputMode;
    public InputMode inputMode {
        get { return _inputMode; }
    }
    // int numberFrames;
    public bool showDebugRays;
    // public InputController inputController;
    public UIController uiController;
    public CharacterCamera characterCamera;
    public CharacterController playerCharacterController;
    public void Start() {
        cursorType = CursorType.pointer;
        showDebugRays = true;
        showConsole.action.performed += HandleShowConsleAction;
        escapeAction.action.performed += HandleEscapeAction;

        CyberNodeIndicator.staticOnMouseOver += HandleCyberNodeMouseOver;
        CyberNodeIndicator.staticOnMouseExit += HandleCyberNodeMouseExit;
    }
    public void StartNewGame(string filename) {
        timePlayed = 0f;

        GameData data = GameData.TestInitialData() with {
            filename = filename
        };

        gameData = data;
        StartNewDay();
    }
    public void StartNewDay() {
        SetMarketData();
        SetDealData();
        SaveGameData();
        ReturnToApartment();
    }
    public void SetMarketData() {
        gameData.marketData = new MarketData() {
            preferences = new List<LootPreferenceData>{
                new LootPreferenceData(LootCategory.drug, 1),
                new LootPreferenceData(LootCategory.medical, 2),
            },
            description = "big shipment of synthetic biologicals coming in from the mainland tonight."
        };
    }
    public void SetDealData() {
        gameData.dealData = new List<DealData>() {
            DealData.FromLootData(Resources.Load("data/loot/drug/rush") as LootData, 10, LootCategory.drug, 3,
                "Somehow I ended up with too much of this stuff. Can you take it off my hands?"),
            DealData.FromLootData(Resources.Load("data/loot/drug/zyme") as LootData, 3, LootCategory.drug, 2,
                "A big crate of zyme fell off the truck at the docks. Now I have to move it, pronto."
            ),
        };
    }
    public void LoadGame(GameData loadData) {
        Debug.Log($"load {loadData.filename}");
        timePlayed = 0f;
        GameManager.I.gameData = loadData;
        GameManager.I.CloseMenu();
        GameManager.I.ReturnToApartment();
    }
    public void SaveGameData() {
        gameData.timePlayedInSeconds += timePlayed;
        gameData.lastPlayedTime = DateTime.Now;
        timePlayed = 0f;
        gameData.Save();
        uiController?.ShowSaveIndicator();
    }
    void HandleShowConsleAction(InputAction.CallbackContext ctx) {
        toggleConsoleThisFrame = ctx.ReadValueAsButton();
    }
    void HandleEscapeAction(InputAction.CallbackContext ctx) {
        escapePressedThisFrame = ctx.ReadValueAsButton();
    }
    // void LateUpdate() {
    //     numberFrames += 1;
    // }

    public override void OnDestroy() {
        base.OnDestroy();
        showConsole.action.performed -= HandleShowConsleAction;
        escapeAction.action.performed -= HandleEscapeAction;

        CyberNodeIndicator.staticOnMouseOver -= HandleCyberNodeMouseOver;
        CyberNodeIndicator.staticOnMouseExit -= HandleCyberNodeMouseExit;
    }
    public void TransitionToPhase(GamePhase newState) {
        if (newState == gameData.phase)
            return;
        GamePhase tmpInitialState = gameData.phase;
        OnStateExit(tmpInitialState, newState);
        gameData.phase = newState;
        OnStateEnter(newState, tmpInitialState);
    }
    public void TransitionToInputMode(InputMode newInputMode) {
        if (newInputMode == inputMode)
            return;
        OnInputModeChange?.Invoke(inputMode, newInputMode);
        _inputMode = newInputMode;
        SetOverlay(activeOverlayType);
    }
    private void OnStateEnter(GamePhase state, GamePhase fromState) {
        // Debug.Log($"entering state {state} from {fromState}");
        switch (state) {
            case GamePhase.vrMission:
            case GamePhase.levelPlay:
                Time.timeScale = 1f;
                cursorType = CursorType.gun;
                TransitionToInputMode(InputMode.gun);
                break;
            case GamePhase.world:
                Time.timeScale = 1f;
                cursorType = CursorType.gun;
                TransitionToInputMode(InputMode.gun);
                characterCamera.disableLockOn = true;
                break;
            case GamePhase.mainMenu:
                Time.timeScale = 0f;
                cursorType = CursorType.pointer;
                break;
            default:
                break;
        }
    }
    public void OnStateExit(GamePhase state, GamePhase toState) {
        switch (state) {
            case GamePhase.none:
                break;
            default:
                break;
        }
    }

    void SetCursor(CursorType cursorType) {
        PointerData data = cursorType switch {
            CursorType.pointer => new PointerData() {
                // mouseCursor = Resources.Load("sprites/UI/elements/Aimpoint/Cursor/Aimpoint16 5") as Texture2D,
                // hotSpot = new Vector2(8, 8),
                mouseCursor = Resources.Load("sprites/UI/elements/Cursor32") as Texture2D,
                hotSpot = new Vector2(0, 0),
                cursorMode = CursorMode.Auto
            },
            CursorType.hand => new PointerData() {
                // mouseCursor = Resources.Load("sprites/UI/elements/Aimpoint/Cursor/Aimpoint16 5") as Texture2D,
                // hotSpot = new Vector2(8, 8),
                mouseCursor = Resources.Load("sprites/UI/elements/Hand") as Texture2D,
                hotSpot = new Vector2(12, 0),
                cursorMode = CursorMode.Auto
            },
            CursorType.gun => new PointerData {
                mouseCursor = Resources.Load("sprites/UI/elements/Aimpoint/Cursor/Aimpoint16 0") as Texture2D,
                hotSpot = new Vector2(8, 8),
                cursorMode = CursorMode.Auto
            },
            _ => new PointerData()
        };
        Cursor.SetCursor(data.mouseCursor, data.hotSpot, data.cursorMode);
    }

    public void ShowMenu(MenuType menuType, Action callback = null) {
        if (activeMenuType != MenuType.none) {
            CloseMenu();
        }
        activeMenuType = menuType;
        Time.timeScale = 0f;
        GameManager.I.uiController.HideUI();
        switch (menuType) {
            default:
                break;
            case MenuType.missionFail:
                if (!SceneManager.GetSceneByName("MissionFailMenu").isLoaded) {
                    LoadScene("MissionFailMenu", callback, unloadAll: false);
                }
                break;
            case MenuType.dialogue:
                if (!SceneManager.GetSceneByName("DialogueMenu").isLoaded) {
                    LoadScene("DialogueMenu", callback, unloadAll: false);
                }
                CloseBurglar();
                uiController.HideUI();

                break;
            case MenuType.VRMissionFinish:
                if (!SceneManager.GetSceneByName("VRMissionFinish").isLoaded) {
                    LoadScene("VRMissionFinish", callback, unloadAll: false);
                }
                break;
            case MenuType.escapeMenu:
                if (!SceneManager.GetSceneByName("EscapeMenu").isLoaded) {
                    LoadScene("EscapeMenu", callback, unloadAll: false);
                }
                break;
            case MenuType.VREscapeMenu:
                if (!SceneManager.GetSceneByName("VREscapeMenu").isLoaded) {
                    LoadScene("VREscapeMenu", callback, unloadAll: false);
                }
                break;
            case MenuType.mainEscapeMenu:
                if (!SceneManager.GetSceneByName("EscapeMenu").isLoaded) {
                    LoadScene("MainEscapeMenu", callback, unloadAll: false);
                }
                break;
            case MenuType.gunshop:
                if (!SceneManager.GetSceneByName("GunShop").isLoaded) {
                    LoadScene("GunShop", callback, unloadAll: false);
                }
                break;
            case MenuType.itemshop:
                if (!SceneManager.GetSceneByName("ItemShop").isLoaded) {
                    LoadScene("ItemShop", callback, unloadAll: false);
                }
                break;
            case MenuType.lootshop:
                if (!SceneManager.GetSceneByName("LootShop").isLoaded) {
                    LoadScene("LootShop", callback, unloadAll: false);
                }
                break;
            case MenuType.barShop:
                if (!SceneManager.GetSceneByName("BarShop").isLoaded) {
                    LoadScene("BarShop", callback, unloadAll: false);
                }
                break;
            case MenuType.importerShop:
                if (!SceneManager.GetSceneByName("ImporterShop").isLoaded) {
                    LoadScene("ImporterShop", callback, unloadAll: false);
                }
                break;
            case MenuType.gunModShop:
                if (!SceneManager.GetSceneByName("GunModShop").isLoaded) {
                    LoadScene("GunModShop", callback, unloadAll: false);
                }
                break;
            case MenuType.payDataShop:
                if (!SceneManager.GetSceneByName("PayDataShop").isLoaded) {
                    LoadScene("PayDataShop", callback, unloadAll: false);
                }
                break;
            case MenuType.medicalShop:
                if (!SceneManager.GetSceneByName("MedicalShop").isLoaded) {
                    LoadScene("MedicalShop", callback, unloadAll: false);
                }
                break;
            case MenuType.console:
                uiController.ShowTerminal();
                callback?.Invoke();
                break;
            case MenuType.missionSelect:
                uiController.HideUI();
                uiController.ShowMissionSelector(gameData);
                break;
        }
    }
    public void CloseMenu() {
        switch (activeMenuType) {
            default:
                break;
            case MenuType.console:
                uiController.HideTerminal();
                uiController.ShowUI();
                break;
            case MenuType.dialogue:
                uiController.ShowUI();
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("DialogueMenu"));
                break;
            case MenuType.escapeMenu:
                uiController.ShowUI();
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("EscapeMenu"));
                break;
            case MenuType.VREscapeMenu:
                uiController.ShowUI();
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("VREscapeMenu"));
                break;
            case MenuType.mainEscapeMenu:
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("MainEscapeMenu"));
                break;
            case MenuType.gunshop:
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("GunShop"));
                break;
            case MenuType.itemshop:
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("ItemShop"));
                break;
            case MenuType.lootshop:
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("LootShop"));
                break;
            case MenuType.barShop:
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("BarShop"));
                break;
            case MenuType.importerShop:
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("ImporterShop"));
                break;
            case MenuType.gunModShop:
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("GunModShop"));
                break;
            case MenuType.payDataShop:
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("PayDataShop"));
                break;
            case MenuType.medicalShop:
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("MedicalShop"));
                break;
            case MenuType.missionSelect:
                uiController.HideMissionSelector();
                break;
        }
        Time.timeScale = 1f;
        activeMenuType = MenuType.none;
    }

    public void IncrementOverlay(int increment) {
        int current = (int)activeOverlayType;
        int length = Enum.GetNames(typeof(OverlayType)).Length;
        int nextInt = (current + increment);

        if (nextInt < 0) {
            nextInt = length - 1;
        } else if (nextInt >= length) {
            nextInt = 0;
        }

        OverlayType newType = (OverlayType)nextInt;
        SetOverlay(newType);
    }
    public void SetOverlay(OverlayType newType) {
        // gameData.overlayIndex = (int)newType;
        activeOverlayType = newType;
        bool overlayEnabled = inputMode != InputMode.aim && inputMode != InputMode.wallpressAim && inputMode != InputMode.burglar;
        OnOverlayChange?.Invoke(newType, overlayEnabled);
    }

    public void Update() {

        if (isLoadingLevel)
            return;

        timePlayed += Time.unscaledDeltaTime;

        if (toggleConsoleThisFrame) {
            if (activeMenuType != MenuType.console) {
                ShowMenu(MenuType.console);
            } else {
                CloseMenu();
            }
        }

        if (escapePressedThisFrame) {
            HandleEscapePressed();
        }

        if (gameData.phase == GamePhase.world) {
            DoInputs();
        } else if (gameData.phase == GamePhase.levelPlay || gameData.phase == GamePhase.vrMission) {
            UpdateSuspicion();
            UpdateAlarm();
            UpdateReportTickets();
            UpdateGraphs();
            if (cutsceneIsRunning) {
                playerCharacterController.ResetMovement();
            } else {
                DoInputs();
            }
        } else {
            if (resetMouseControl) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                resetMouseControl = false;
            }
        }
        toggleConsoleThisFrame = false;
        escapePressedThisFrame = false;
        if (playerObject != null)
            playerPosition = playerObject.transform.position;
    }
    public void HandleEscapePressed() {
        if (gameData.phase == GamePhase.world) {
            if (activeMenuType == MenuType.none) {
                // TODO: show world pause menu
                ShowMenu(MenuType.mainEscapeMenu);
            } else {
                DoEscapeMenus();
            }
            escapePressedThisFrame = false;
        } else if (gameData.phase == GamePhase.levelPlay || gameData.phase == GamePhase.vrMission) {
            if (inputMode == InputMode.burglar) {

            } else {
                if (activeMenuType == MenuType.none) {
                    MenuType escapeMenuType = gameData.phase == GamePhase.levelPlay ? MenuType.escapeMenu : MenuType.VREscapeMenu;
                    ShowMenu(escapeMenuType);
                } else {
                    DoEscapeMenus();
                }
                escapePressedThisFrame = false;
            }
        }
    }
    void DoEscapeMenus() {
        if (activeMenuType == MenuType.console ||
            activeMenuType == MenuType.escapeMenu ||
            activeMenuType == MenuType.VREscapeMenu ||
            activeMenuType == MenuType.missionSelect ||
            activeMenuType == MenuType.mainEscapeMenu) {
            GameManager.I.CloseMenu();
        }
    }

    void DoInputs() {
        bool uiclick = EventSystem.current?.IsPointerOverGameObject() ?? true;
        PlayerInput playerInput = PlayerInput.none;
        if (Time.timeScale > 0) {
            playerInput = InputController.I.HandleCharacterInput(uiclick, escapePressedThisFrame);
            if (gameData.levelState != null)
                uiController?.UpdateWithPlayerInput(playerInput);
        }
        UpdateCursor(uiclick, playerInput);

        // still not 100% clean here
        CameraInput input = playerCharacterController.BuildCameraInput();
        characterCamera.UpdateWithInput(input);
    }

    void UpdateCursor(bool uiclick, PlayerInput playerInput) {
        if (inputMode == InputMode.burglar) {
            if (!uiclick) {
                cursorType = CursorType.gun;
            }
        } else {
            if (playerCharacterController.gunHandler.gunInstance == null) {
                cursorType = CursorType.pointer;
                // } else if (playerInput.Fire.cursorData.highlightableTargetData?.targetIsInRange ?? false) {
                //     cursorType = CursorType.pointer;
            } else if (uiclick) {
                cursorType = CursorType.pointer;
            } else {
                cursorType = CursorType.gun;
            }

            if (inputMode == InputMode.aim && activeMenuType == MenuType.none) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                resetMouseControl = true;
            } else {
                if (resetMouseControl) {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    resetMouseControl = false;
                }
            }
        }
    }

    public void HandleCyberNodeMouseOver(NodeIndicator<CyberNode, CyberGraph> indicator) {
        cursorType = CursorType.pointer;
        TransitionToInputMode(InputMode.cyber); // TODO: ?
    }
    public void HandleCyberNodeMouseExit(NodeIndicator<CyberNode, CyberGraph> indicator) {
        cursorType = CursorType.gun;
        TransitionToInputMode(InputMode.gun);
    }

    public void StartBurglar(BurgleTargetData data) {
        // TODO: enter menu state
        activeBurgleTargetData = data;
        uiController.ShowBurglar(data);
    }
    public void CloseBurglar(bool transitionCharacter = true) {
        uiController.HideBurglar();
        uiController.ShowUI();
        if (transitionCharacter)
            playerCharacterController.TransitionToState(CharacterState.normal);
        TransitionToInputMode(InputMode.gun);
    }
    public void ShowMissionSelectMenu() {
        ShowMenu(MenuType.missionSelect);
    }
    public void HideMissionSelectMenu() {
        CloseMenu();
    }

    public void ShowShopMenu(StoreType storeType, LootBuyerData lootBuyerData) => ShowMenu(storeType switch {
        StoreType.gun => MenuType.gunshop,
        StoreType.item => MenuType.itemshop,
        StoreType.loot => MenuType.lootshop,
        StoreType.bar => MenuType.barShop,
        StoreType.importer => MenuType.importerShop,
        StoreType.gunmod => MenuType.gunModShop,
        StoreType.paydata => MenuType.payDataShop,
        StoreType.medical => MenuType.medicalShop,
        _ => MenuType.none
    }, callback: storeType switch {
        StoreType.loot => () => {
            LootShopController lootShopController = GameObject.FindObjectOfType<LootShopController>();
            lootShopController.Initialize(lootBuyerData);
        }
        ,
        StoreType.bar => () => {
            BarShopController barShopController = GameObject.FindObjectOfType<BarShopController>();
            barShopController.Initialize();
        }
        ,
        StoreType.importer => () => {
            ImporterShopController importerShopController = GameObject.FindObjectOfType<ImporterShopController>();
            importerShopController.Initialize();
        }
        ,
        StoreType.gunmod => () => {
            GunModShopController gunModShopController = GameObject.FindObjectOfType<GunModShopController>();
            gunModShopController.Initialize();
        }
        ,
        StoreType.paydata => () => {
            PayDataShopController payDataShopController = GameObject.FindObjectOfType<PayDataShopController>();
            payDataShopController.Initialize();
        }
        ,
        StoreType.medical => () => {
            MedicalShopController medicalShopController = GameObject.FindObjectOfType<MedicalShopController>();
            medicalShopController.Initialize();
        }
        ,
        _ => null
    });

    public void HideShopMenu() {
        CloseMenu();
        uiController.ShowInteractiveHighlight();
    }
    public void ShowMissionPlanner(LevelTemplate template) {
        TransitionToPhase(GamePhase.plan);
        LoadScene("MissionPlan", () => {
            MissionPlanController controller = GameObject.FindObjectOfType<MissionPlanController>();
            controller.Initialize(gameData, template);
            Debug.Log("controller intialized");
        });
    }
    public void ReturnToMissionSelector() {
        LoadScene("Apartment", () => {
            // TODO: move player, open computer
            StartWorld("Apartment");
        });
    }
    public void ReturnToApartment() {
        LoadScene("Apartment", () => {
            Debug.Log("return to apartment callback");
            StartWorld("Apartment");
        });
    }

    public bool IsPlayerVisible(float distance) {
        // B < 10: -
        // 10 < B < 15: +
        // 15 < B < 30: ++
        // 30 < B < 60: +++
        // 60 < B < 80: ++++
        // 80 < B : +++++

        // Toolbox.GetP
        int lightLevel = playerLightLevelProbe.GetDiscreteLightLevel();
        bool isVisible = lightLevel switch {
            0 => distance < 2f,
            1 => distance < 3f,
            2 => distance < 6f,
            3 => distance < 8f,
            4 => distance < 15f,
            _ => distance < 50f
        };

        return isVisible;
    }
}
