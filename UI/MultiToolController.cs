using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiToolController : MonoBehaviour {
    enum State { none, usbScan, wireConnect, ramConnect }
    State state;
    // mode: tool is out, usb not selected: show prompt
    // mode: usb selected: show "scanning..."
    // mode: usb selected, mouse over: show info on connected node
    // mode: usb connected to ram chip: show decode mode
    public AudioSource audioSource;
    public AudioClip[] numberButtonTone;
    [Header("state none")]
    public GameObject noneDisplay;
    public TextMeshProUGUI noneDisplayText;
    [Header("state scan")]
    public GameObject scanDisplay;
    public TextMeshProUGUI scanDisplayText;
    [Header("wire state")]
    public GameObject wireDisplay;
    public TextMeshProUGUI wireDisplayText;
    [Header("ram state")]
    public GameObject ramDisplay;
    public TextMeshProUGUI ramDisplayText;
    public Sprite ramConnectSprite;
    public List<Image> ramTopWaveImages;
    public List<Image> ramBottomWaveImages;
    public List<TextMeshProUGUI> ramBottomNumerals;
    public List<Sprite> waveforms;
    public List<Image> slotHighlights;
    public List<Image> slotIndicators;
    public Sprite correctSprite;
    public Color correctColor;
    public Sprite incorrectSprite;
    public Color incorrectColor;
    public Color waveformColor;
    public GameObject decodedMessage;
    [Header("input state")]
    public GameObject inputDisplay;
    public TextMeshProUGUI inputTextDisplay;
    public List<TextMeshProUGUI> inputNumerals;


    [Header("buttons")]
    public List<Button> buttons;

    int[] targetDigits = new int[4];
    int[] enteredDigits = new int[4];
    List<int> waveformPermutation;
    BurglarToolType currentTool;
    AttackSurfaceElement connectedElement;
    AttackSurfaceDoorLockChip currentDoorLockChip;
    int currentNumericSlotIndex;
    Coroutine highlightBlinkRoutine;
    public void Initialize() {
        decodedMessage.SetActive(false);
        ramDisplay.SetActive(false);
        wireDisplay.SetActive(false);
        scanDisplay.SetActive(false);
        noneDisplay.SetActive(true);
        ChangeState(State.none);
    }
    public void MouseOverUIElementCallback(AttackSurfaceElement element) {
        if (connectedElement != null) return;
        if (currentTool == BurglarToolType.usb) {
            if (element is AttackSurfaceGraphWire) {
                ChangeState(State.wireConnect);
                DisplayWire((AttackSurfaceGraphWire)element);
            } else if (element is AttackSurfaceDoorLockChip) {
                ChangeState(State.wireConnect);
                wireDisplayText.text = $"RAM chip detected: door lock code";
                // displayText.text = $"RAM chip detected: door lock code";
            } else {
                ChangeState(State.usbScan);
            }
        } else {
            ChangeState(State.none);
        }

    }
    public void MouseExitUIElementCallback(AttackSurfaceElement element) {
        if (connectedElement != null) return;
        ChangeState(State.usbScan);
    }

    public void OnToolSelect(BurglarToolType toolType) {
        currentTool = toolType;
        if (toolType == BurglarToolType.usb) {
            connectedElement = null;
            Debug.Log($"on tool select: {toolType}");
            ChangeState(State.usbScan);
        } else {
            if (connectedElement == null)
                ChangeState(State.none);
        }
    }

    public void OnUSBToolReset() {
        connectedElement = null;
        ChangeState(State.none);
    }
    void DisplayWire(AttackSurfaceGraphWire wire) {
        string type = "";
        string targetName = "";
        if (wire.isAlarm) {
            type = "alarm network";
            targetName = GameManager.I.GetAlarmNode(wire.toId).nodeTitle;
        } else if (wire.isCyber) {
            type = "cyber network";
            targetName = GameManager.I.GetCyberNode(wire.toId).nodeTitle;
        } else if (wire.isPower) {
            type = "power network";
            targetName = GameManager.I.GetPowerNode(wire.toId).nodeTitle;
        }
        wireDisplayText.text = $"connected\ntype:{type}\nconnected to: {targetName}";
    }
    void ChangeState(State newState) {
        Debug.Log($"change state: {newState}");
        ExitState(state);
        state = newState;
        EnterState(newState);
    }
    void EnterState(State newState) {
        switch (newState) {
            case State.none:
                foreach (Button button in buttons) {
                    button.interactable = false;
                }
                noneDisplay.SetActive(true);
                noneDisplayText.text = "select tool";
                break;
            case State.usbScan:
                foreach (Button button in buttons) {
                    button.interactable = false;
                }
                wireDisplay.SetActive(true);
                wireDisplayText.text = "scanning...";
                break;
            case State.wireConnect:
                foreach (Button button in buttons) {
                    button.interactable = false;
                }
                wireDisplay.SetActive(true);
                break;
            case State.ramConnect:
                currentNumericSlotIndex = 0;
                ramDisplay.SetActive(true);
                ramDisplayText.text = "power analysis";
                break;
        }
    }
    void ExitState(State oldState) {
        switch (oldState) {
            case State.none:
                noneDisplay.SetActive(false);
                break;
            case State.usbScan:
                wireDisplay.SetActive(false);
                break;
            case State.wireConnect:
                wireDisplay.SetActive(false);
                break;
            case State.ramConnect:
                ramDisplay.SetActive(false);
                break;
        }
    }

    public void HandleConnection(BurglarAttackResult result) {
        if (result == null) {
            OnUSBToolReset();
            return;
        } else {
            connectedElement = result.element;
            if (result.element is AttackSurfaceDoorLockChip) {
                ChangeState(State.ramConnect);
                InitializePowerAnalysis((AttackSurfaceDoorLockChip)result.element);
            }
        }
    }

    public void NumericButtonCallback(int digit) {
        if (currentDoorLockChip?.doorLock.isDecoded ?? false) return;
        Toolbox.RandomizeOneShot(audioSource, numberButtonTone);
        SetTargetDigit(currentNumericSlotIndex, digit);
        if (!currentDoorLockChip.doorLock.isDecoded) {
            currentNumericSlotIndex += 1;
            if (currentNumericSlotIndex > 3) {
                currentNumericSlotIndex = 0;
            }
            if (highlightBlinkRoutine != null) StopCoroutine(highlightBlinkRoutine);
            for (int i = 0; i < 4; i++) {
                slotHighlights[i].enabled = i == currentNumericSlotIndex;
                if (i == currentNumericSlotIndex)
                    highlightBlinkRoutine = StartCoroutine(blinker(slotHighlights[i]));
            }
        }
    }
    IEnumerator blinker(Image target) {
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(0.05f);
        while (true) {
            target.enabled = !target.enabled;
            yield return wait;
        }
    }

    void SetTargetDigit(int index, int value) {
        enteredDigits[index] = value;
        ramBottomNumerals[index].text = $"{value}";
        ramBottomWaveImages[index].enabled = true;
        ramBottomWaveImages[index].sprite = waveforms[waveformPermutation[value]];

        if (value == targetDigits[index]) {
            slotIndicators[index].color = correctColor;
            slotIndicators[index].sprite = correctSprite;
            EaseInIndicator(slotIndicators[index]);
        } else {
            slotIndicators[index].color = incorrectColor;
            slotIndicators[index].sprite = incorrectSprite;
            EaseInIndicator(slotIndicators[index]);
        }
        blinkWaveform(ramBottomWaveImages[index], value == targetDigits[index]);
        slotIndicators[index].enabled = true;
        CheckIfDecoded(currentDoorLockChip.doorLock);
    }
    void blinkWaveform(Image waveform, bool correct) {
        // float timer = 0f;
        // float duration = 0.0
        if (correct) {
            StartCoroutine(Toolbox.BlinkColor(waveform, waveformColor, Color.white));
        } else {
            StartCoroutine(Toolbox.BlinkColor(waveform, waveformColor, Color.red));
        }
    }
    void EaseInIndicator(Image indicator) {
        RectTransform rectTransform = indicator.GetComponent<RectTransform>();
        StartCoroutine(Toolbox.Ease(null, 0.2f, 0f, 1f, PennerDoubleAnimation.ExpoEaseIn, (amount) => {
            rectTransform.localScale = new Vector3(amount, 1f, 1f);
        }, unscaledTime: true));
    }

    void InitializePowerAnalysis(AttackSurfaceDoorLockChip chip) {
        currentDoorLockChip = chip;
        int lockId = chip.doorLock.lockId;
        waveformPermutation = chip.waveformPermutation;
        for (int i = 0; i < 4; i++) {
            int digit = (int)Mathf.Abs((lockId / (Mathf.Pow(10, 3 - i))) % 10);
            targetDigits[i] = digit;
            enteredDigits[i] = -1;
            ramTopWaveImages[i].sprite = waveforms[waveformPermutation[digit]];
            if (chip.doorLock.isDecoded) {
                ramBottomWaveImages[i].enabled = true;
                ramBottomWaveImages[i].sprite = waveforms[waveformPermutation[digit]];
                ramBottomNumerals[i].text = $"{digit}";
                slotHighlights[i].enabled = false;
                slotIndicators[i].enabled = true;
                slotIndicators[i].color = correctColor;
                slotIndicators[i].sprite = correctSprite;
            } else {
                ramBottomWaveImages[i].enabled = false;
                ramBottomNumerals[i].text = $"";
                slotHighlights[i].enabled = i == currentNumericSlotIndex;
                slotIndicators[i].enabled = false;
            }
        }
        foreach (Button button in buttons) {
            button.interactable = !chip.doorLock.isDecoded;
        }
        if (!chip.doorLock.isDecoded) {
            BlinkButtons();
            StartCoroutine(
                BlinkWaveforms(ramTopWaveImages)
            );
        }
    }


    void CheckIfDecoded(DoorLock doorLock) {
        bool match = true;
        for (int i = 0; i < 4; i++) {
            match &= targetDigits[i] == enteredDigits[i];
        }
        if (match && !doorLock.isDecoded) {
            doorLock.isDecoded = true;
            foreach (Button button in buttons) {
                button.interactable = false;
            }
            if (highlightBlinkRoutine != null) StopCoroutine(highlightBlinkRoutine);
            foreach (Image highlight in slotHighlights) {
                highlight.enabled = false;
            }
            GameManager.I.AddKey(doorLock.lockId, DoorLock.LockType.keycardCode, GameManager.I.playerPosition);
            StartCoroutine(Toolbox.ChainCoroutines(
                BlinkWaveforms(ramBottomWaveImages),
                BlinkWaveforms(ramBottomWaveImages),
                // BlinkWaveforms(ramBottomWaveImages)
                Toolbox.CoroutineFunc(() => {
                    StartCoroutine(Toolbox.BlinkColor(ramBottomNumerals[0], waveformColor));
                    StartCoroutine(Toolbox.BlinkColor(ramBottomNumerals[1], waveformColor));
                    StartCoroutine(Toolbox.BlinkColor(ramBottomNumerals[2], waveformColor));
                    StartCoroutine(Toolbox.BlinkColor(ramBottomNumerals[3], waveformColor));
                }),
                Toolbox.CoroutineFunc(() => BlinkDecodeMessage())
            ));
        }
    }

    void BlinkButtons() {
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(0.03f);
        ColorBlock normalColors = buttons[0].colors;
        ColorBlock colorBlock = buttons[0].colors;
        colorBlock.normalColor = Color.white;
        StartCoroutine(Toolbox.ChainCoroutines(
            Toolbox.CoroutineFunc(() => buttons[0].colors = colorBlock),
            wait,
            Toolbox.CoroutineFunc(() => {
                buttons[0].colors = normalColors;
                buttons[1].colors = colorBlock;
            }),
            wait,
            Toolbox.CoroutineFunc(() => {
                buttons[1].colors = normalColors;
                buttons[2].colors = colorBlock;
            }),
            wait,
            Toolbox.CoroutineFunc(() => {
                buttons[2].colors = normalColors;
                buttons[3].colors = colorBlock;
            }),
            wait,
            Toolbox.CoroutineFunc(() => {
                buttons[3].colors = normalColors;
                buttons[4].colors = colorBlock;
            }),
            wait,
            Toolbox.CoroutineFunc(() => {
                buttons[4].colors = normalColors;
                buttons[5].colors = colorBlock;
            }),
            wait,
            Toolbox.CoroutineFunc(() => {
                buttons[5].colors = normalColors;
                buttons[6].colors = colorBlock;
            }),
            wait,
            Toolbox.CoroutineFunc(() => {
                buttons[6].colors = normalColors;
                buttons[7].colors = colorBlock;
            }),
            wait,
            Toolbox.CoroutineFunc(() => {
                buttons[7].colors = normalColors;
                buttons[8].colors = colorBlock;
            }),
            wait,
            Toolbox.CoroutineFunc(() => {
                buttons[8].colors = normalColors;
                buttons[9].colors = colorBlock;
            }),
            wait,
            Toolbox.CoroutineFunc(() => {
                buttons[9].colors = normalColors;
            })
        ));
    }
    void BlinkDecodeMessage() {
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(0.08f);
        WaitForSecondsRealtime hangtime = new WaitForSecondsRealtime(2f);
        StartCoroutine(Toolbox.ChainCoroutines(
            Toolbox.CoroutineFunc(() => decodedMessage.SetActive(true)),
            wait,
            Toolbox.CoroutineFunc(() => decodedMessage.SetActive(false)),
            wait,
            Toolbox.CoroutineFunc(() => decodedMessage.SetActive(true)),
            hangtime,
            Toolbox.CoroutineFunc(() => decodedMessage.SetActive(false))
        ));
    }
    IEnumerator BlinkWaveforms(List<Image> waveforms) {
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(0.08f);
        return Toolbox.ChainCoroutines(
            Toolbox.CoroutineFunc(() => waveforms[0].color = Color.white),
            wait,
            Toolbox.CoroutineFunc(() => {
                waveforms[0].color = waveformColor;
                waveforms[1].color = Color.white;
            }),
            wait,
            Toolbox.CoroutineFunc(() => {
                waveforms[1].color = waveformColor;
                waveforms[2].color = Color.white;
            }),
            wait,
            Toolbox.CoroutineFunc(() => {
                waveforms[2].color = waveformColor;
                waveforms[3].color = Color.white;
            }),
            wait,
            Toolbox.CoroutineFunc(() => {
                waveforms[3].color = waveformColor;
            })
        );
    }
}
