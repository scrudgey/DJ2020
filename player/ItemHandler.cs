using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Items;
using UnityEngine;
public record HvacUseResult {
    public bool activateHVAC;
    public HVACElement startElement;
    public HVACNetwork hVACNetwork;
    public HVACElement dismountElement;
    public static HvacUseResult Empty() => new HvacUseResult {
        activateHVAC = false,
        startElement = null,
        hVACNetwork = null,
        dismountElement = null
    };
}
public record ItemUseResult {
    public bool crouchDown;
    public bool waveArm;
    public bool doBurgle;
    public Ladder useLadder;
    public AttackSurface attackSurface;
    public bool emptyUse;
    public HvacUseResult hvacUseResult;
    public static ItemUseResult Empty() => new ItemUseResult {
        hvacUseResult = HvacUseResult.Empty()
    };
}
public class ItemHandler : MonoBehaviour, IBindable<ItemHandler> {
    public GunHandler gunHandler;
    public Action<ItemHandler> OnValueChanged { get; set; }
    public List<ItemInstance> items = new List<ItemInstance>();
    public int index;
    public ItemInstance activeItem;
    public AudioSource audioSource;
    public AudioClip[] emptyUse;
    public RocketLauncher rocketLauncher;
    public readonly float SUSPICION_TIMEOUT = 1.5f;

    void Awake() {
        audioSource = Toolbox.SetUpAudioSource(gameObject);
    }
    void Start() {
        OnItemEnter(activeItem);
    }
    public void LoadItemState(List<ItemTemplate> loadItems) {
        items = new List<ItemInstance>();
        foreach (ItemTemplate template in loadItems) {
            ItemInstance instance = ItemInstance.FactoryLoad(template);
            items.Add(instance);
        }
        ClearItem();
    }

    public ItemUseResult SetInputs(PlayerInput input) {
        if (input.incrementItem != 0) {
            int cycles = 0;
            while (cycles < 6) {
                cycles += 1;
                index += input.incrementItem;
                if (index < 0) {
                    index = items.Count - 1;
                } else if (index >= items.Count) {
                    index = 0;
                }
                if (activeItem != null)
                    break;
                if (activeItem == null && items[index] != null)
                    break;
            }
            SwitchToItem(items[index]);
        }
        if (input.selectItem != null) {
            ItemInstance instance = items.Where(item => item.template == input.selectItem).FirstOrDefault();
            if (activeItem == instance && instance.toggleable) {
                ClearItem();
            } else {
                SwitchToItem(instance);
            }
            index = items.IndexOf(instance);
        }
        if (activeItem is RocketLauncherItem) {
            if (input.Fire.FirePressed) {
                return UseItem(input);
            }
        }
        if (input.useItem) {
            return UseItem(input);
        } else return ItemUseResult.Empty();
    }
    void SwitchToItem(ItemInstance item) {
        OnItemExit(this.activeItem);
        this.activeItem = item;
        OnItemEnter(this.activeItem);
        OnValueChanged?.Invoke(this);
    }
    void ClearItem() {
        SwitchToItem(null);
        index = items.IndexOf(null);
    }
    ItemUseResult UseItem(PlayerInput input) {
        if (activeItem == null)
            return ItemUseResult.Empty();
        ItemUseResult result = activeItem.Use(this, input);
        if (result.emptyUse) {
            Toolbox.RandomizeOneShot(audioSource, emptyUse);
        }
        OnValueChanged?.Invoke(this);
        return result;
    }
    public void EvictSubweapon() {
        if (activeItem != null && activeItem.subweapon) {
            ClearItem();
        }
    }
    void OnItemEnter(ItemInstance item) {
        if (item == null)
            return;
        if (item.subweapon) {
            gunHandler?.SwitchToGun(-1);
        }
        switch (item) {
            case RocketLauncherItem rocketItem:
                Toolbox.RandomizeOneShot(audioSource, rocketItem.rocketData.deploySound);
                break;
            case CyberDeck:
                GameManager.I.playerManualHacker.deployed = true;
                if (GameManager.I.activeOverlayType == OverlayType.none)
                    GameManager.I.SetOverlay(OverlayType.limitedCyber);
                break;
            case IRGoggles goggles:
                GameManager.I.gameData.playerState.cyberEyesThermalBuff = true;
                GameManager.OnEyeVisibilityChange?.Invoke(GameManager.I.gameData.playerState);
                Toolbox.RandomizeOneShot(audioSource, goggles.goggleData.wearSounds);
                break;
            default:
                break;
        }
    }

    void OnItemExit(ItemInstance item) {
        if (item == null)
            return;
        switch (item) {
            case CyberDeck:
                if (GameManager.I.activeOverlayType == OverlayType.limitedCyber)
                    GameManager.I.SetOverlay(OverlayType.none);
                GameManager.I.playerManualHacker.deployed = false;
                GameManager.I.playerManualHacker.Disconnect();
                break;
            case IRGoggles:
                GameManager.I.gameData.playerState.cyberEyesThermalBuff = false;
                GameManager.OnEyeVisibilityChange?.Invoke(GameManager.I.gameData.playerState);
                break;
            default:
                break;
        }
    }
    public void ThrowGrenade(GrenadeData data, PlayerInput input) {
        float sin45 = 0.70710678118f;  // 1/√2

        Vector3 gunPosition = new Vector3(transform.position.x, transform.position.y + 0.45f, transform.position.z);
        Vector3 localPosition = (input.Fire.cursorData.groundPosition - gunPosition);
        Vector3 localDirection = localPosition.normalized;

        float distance = localPosition.magnitude;
        float initialSpeed = Mathf.Sqrt(distance * Mathf.Abs(Physics.gravity.y));
        Vector3 initialVelocity = initialSpeed * sin45 * Vector3.up + initialSpeed * sin45 * localDirection;
        GameObject obj = GameObject.Instantiate(data.grenadePrefab, gunPosition + (0.15f * localDirection), Quaternion.identity);
        Rigidbody body = obj.GetComponent<Rigidbody>();
        body.velocity = initialVelocity;
        foreach (Collider myCollider in transform.root.GetComponentsInChildren<Collider>()) {
            foreach (Collider grenadeCollider in obj.GetComponentsInChildren<Collider>()) {
                Physics.IgnoreCollision(myCollider, grenadeCollider, true);
            }
        }
    }
}
