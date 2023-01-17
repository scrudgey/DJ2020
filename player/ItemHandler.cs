using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Items;
using UnityEngine;
public record ItemUseResult {
    public bool transitionToUseItem;
    public bool waveArm;
    public static ItemUseResult Empty() => new ItemUseResult {

    };
}
public class ItemHandler : MonoBehaviour, IBindable<ItemHandler> {

    public Action<ItemHandler> OnValueChanged { get; set; }

    public List<BaseItem> items = new List<BaseItem>();
    public int index;
    public BaseItem activeItem;
    public AudioSource audioSource;
    public RocketLauncher rocketLauncher;
    public readonly float SUSPICION_TIMEOUT = 1.5f;
    float burglarHighlightTimer;
    float burglarHighlightRefreshInterval = 1f;
    void Awake() {
        audioSource = Toolbox.SetUpAudioSource(gameObject);
    }
    void Start() {
        OnItemEnter(activeItem);
    }

    public ItemUseResult SetInputs(PlayerInput input) {
        if (input.incrementItem != 0) {
            index += input.incrementItem;
            if (index < 0) {
                index = items.Count - 1;
            } else if (index >= items.Count) {
                index = 0;
            }
            SwitchToItem(items[index]);
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
    void SwitchToItem(BaseItem item) {
        OnItemExit(this.activeItem);
        this.activeItem = item;
        OnItemEnter(this.activeItem);
        OnValueChanged?.Invoke(this);
    }
    void ClearItem() {
        SwitchToItem(null);
        index = items.IndexOf(null);
    }
    public void LoadItemState(string[] itemNames) {
        items = new List<BaseItem>();
        foreach (string itemName in itemNames) {
            BaseItem newItem = ItemInstance.LoadItem(itemName);
            items.Add(newItem);
        }
        items.Add(null);
        items = items.ToHashSet().ToList();
        ClearItem();
    }

    ItemUseResult UseItem(PlayerInput input) {
        if (activeItem == null)
            return ItemUseResult.Empty();
        return activeItem.Use(this, input);
    }

    void OnItemEnter(BaseItem item) {
        if (item == null)
            return;
        switch (item) {
            case RocketLauncherItem rocketItem:
                Toolbox.RandomizeOneShot(audioSource, rocketItem.rocketData.deploySound);
                break;
            case CyberDeck:
                GameManager.I.SetOverlay(OverlayType.cyber);
                break;
            case IRGoggles goggles:
                GameManager.I.gameData.playerState.cyberEyesThermalBuff = true;
                GameManager.OnEyeVisibilityChange?.Invoke(GameManager.I.gameData.playerState);
                Toolbox.RandomizeOneShot(audioSource, goggles.goggleData.wearSounds);
                break;
            case BurglarTools:
                // foreach (AttackSurface surface in GameObject.FindObjectsOfType<AttackSurface>()) {
                //     surface.EnableOutline();
                // }
                break;
            default:
                break;
        }
    }
    void Update() {
        if (activeItem is BurglarTools) {
            burglarHighlightTimer += Time.deltaTime;
            if (burglarHighlightTimer > burglarHighlightRefreshInterval) {
                burglarHighlightTimer -= burglarHighlightRefreshInterval;
                foreach (AttackSurface surface in GameObject.FindObjectsOfType<AttackSurface>()) {
                    if (Math.Abs(surface.transform.position.y - transform.position.y) < 1f) {
                        surface.EnableOutline();
                    } else {
                        surface.DisableOutline();
                    }
                }
            }

        }
    }
    void OnItemExit(BaseItem item) {
        if (item == null)
            return;
        switch (item) {
            case CyberDeck:
                GameManager.I.SetOverlay(OverlayType.none);
                break;
            case IRGoggles:
                GameManager.I.gameData.playerState.cyberEyesThermalBuff = false;
                GameManager.OnEyeVisibilityChange?.Invoke(GameManager.I.gameData.playerState);
                break;
            case BurglarTools:
                burglarHighlightTimer = 0f;
                foreach (AttackSurface surface in GameObject.FindObjectsOfType<AttackSurface>()) {
                    surface.DisableOutline();
                }
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
