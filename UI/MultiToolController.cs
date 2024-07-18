using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiToolController : MonoBehaviour {
    enum State { none, usbScan, wireConnect, ramConnect, input }
    State state;
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
    public HorizontalLayoutGroup inputNumeralLayoutGroup;

    [Header("buttons")]
    public List<Button> buttons;

    int[] targetDigits = new int[4];
    int[] enteredDigits = new int[4];
    List<int> waveformPermutation;
    BurglarToolType currentTool;
    AttackSurfaceElement connectedElement;
    AttackSurfaceDoorLockChip currentDoorLockChip;
    AttackSurfaceInputChip currentInputChip;
    int currentNumericSlotIndex;
    Coroutine highlightBlinkRoutine;
    Coroutine wireDisplayRoutine;
    public void Initialize() {
        decodedMessage.SetActive(false);
        ramDisplay.SetActive(false);
        wireDisplay.SetActive(false);
        scanDisplay.SetActive(false);
        noneDisplay.SetActive(true);
        inputDisplay.SetActive(false);
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
                wireDisplayText.text = $"RAM chip detected\n\ncontent: door lock code";
            } else if (element is AttackSurfaceInputChip) {
                ChangeState(State.wireConnect);
                wireDisplayText.text = $"I/O chip detected\n\ntype: door code input";
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
            type = "alarm";
            targetName = GameManager.I.GetAlarmNode(wire.toId).nodeTitle;
        } else if (wire.isCyber) {
            type = "cyber";
            targetName = GameManager.I.GetCyberNode(wire.toId).nodeTitle;
        } else if (wire.isPower) {
            type = "power";
            targetName = GameManager.I.GetPowerNode(wire.toId).nodeTitle;
        }
        wireDisplayText.text = $"network analysis\n\ntype: {type}\n\nconnection: {targetName}";
    }
    void ChangeState(State newState) {
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
                noneDisplayText.text = "electronic multitool v2.0";
                break;
            case State.usbScan:
                foreach (Button button in buttons) {
                    button.interactable = false;
                }
                wireDisplay.SetActive(true);
                if (wireDisplayRoutine != null) {
                    StopCoroutine(wireDisplayRoutine);
                }
                wireDisplayRoutine = StartCoroutine(AnimateWireDisplay());
                break;
            case State.wireConnect:
                foreach (Button button in buttons) {
                    button.interactable = false;
                }
                if (wireDisplayRoutine != null) {
                    StopCoroutine(wireDisplayRoutine);
                }
                wireDisplay.SetActive(true);
                break;
            case State.ramConnect:
                ramDisplay.SetActive(true);
                ramDisplayText.text = "power analysis";
                break;
            case State.input:
                inputDisplay.SetActive(true);
                inputTextDisplay.text = "input door code:";
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
            case State.input:
                inputDisplay.SetActive(false);
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
            } else if (result.element is AttackSurfaceInputChip) {
                ChangeState(State.input);
                InitializeInputChip((AttackSurfaceInputChip)result.element);
            }
        }
    }

    public void NumericButtonCallback(int digit) {
        Toolbox.RandomizeOneShot(audioSource, numberButtonTone);
        if (state == State.ramConnect) {
            if (currentDoorLockChip?.doorLock.isDecoded ?? false) return;
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
        } else if (state == State.input) {
            InputDigit(digit);
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
        // Debug.Log($"permutations: {waveformPermutation.Count}\twaveforms: {waveforms.Count}\tdigit-1: {value - 1}\tpermutation: {waveformPermutation[value - 1]}");

        ramBottomWaveImages[index].sprite = waveforms[waveformPermutation[value - 1]];

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
        currentNumericSlotIndex = 0;
        currentDoorLockChip = chip;
        int lockId = chip.doorLock.lockId;
        waveformPermutation = chip.waveformPermutation;
        for (int i = 0; i < 4; i++) {
            int digit = (int)Mathf.Abs((lockId / (Mathf.Pow(10, 3 - i))) % 10);
            targetDigits[i] = digit;
            enteredDigits[i] = -1;

            ramTopWaveImages[i].sprite = waveforms[waveformPermutation[digit - 1]];
            if (chip.doorLock.isDecoded) {
                ramBottomWaveImages[i].enabled = true;
                ramBottomWaveImages[i].sprite = waveforms[waveformPermutation[digit - 1]];
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
    void InitializeInputChip(AttackSurfaceInputChip chip) {
        currentInputChip = chip;
        int lockId = chip.doorLock.lockId;
        currentNumericSlotIndex = 0;
        foreach (Button button in buttons) {
            button.interactable = true;
        }
        for (int i = 0; i < 4; i++) {
            int digit = (int)Mathf.Abs((lockId / (Mathf.Pow(10, 3 - i))) % 10);
            targetDigits[i] = digit;
            enteredDigits[i] = -1;
            inputNumerals[i].text = "-";
            inputNumerals[i].color = waveformColor;
        }
        BlinkButtons();
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
            GameManager.I.AddKey(doorLock.lockId, KeyType.keycardCode, GameManager.I.playerPosition);
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

    void InputDigit(int digit) {
        enteredDigits[currentNumericSlotIndex] = digit;
        inputNumerals[currentNumericSlotIndex].text = $"{digit}";
        currentNumericSlotIndex += 1;
        if (currentNumericSlotIndex > 3) {
            bool match = true;
            int keyId = 0;
            for (int i = 0; i < 4; i++) {
                match &= targetDigits[i] == enteredDigits[i];
                keyId += enteredDigits[i] * (int)Mathf.Pow(10, 3 - i);
            }
            KeyData keyData = new KeyData(KeyType.keycard, keyId);

            if (currentInputChip.keycardReader != null) {
                bool success = currentInputChip.keycardReader.AttemptSingleKey(keyData);
                if (success)
                    currentInputChip.elevatorController?.EnableTemporaryAuthorization();
            } else {
                bool success = currentInputChip.doorLock.TryKeyUnlock(keyData);
                if (success)
                    currentInputChip.elevatorController?.EnableTemporaryAuthorization();
            }

            // InitializeInputChip(currentInputChip);
            if (match) {
                StartCoroutine(SuccessfulDigitInput());
            } else {
                StartCoroutine(FailDigitInput());
            }
        }
    }

    IEnumerator SuccessfulDigitInput() {
        foreach (Button button in buttons) {
            button.interactable = false;
        }
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(1f);
        WaitForSecondsRealtime shortWait = new WaitForSecondsRealtime(0.08f);

        yield return Toolbox.ChainCoroutines(
             Toolbox.CoroutineFunc(() => inputNumerals[0].color = Color.white),
             shortWait,
             Toolbox.CoroutineFunc(() => {
                 inputNumerals[0].color = waveformColor;
                 inputNumerals[1].color = Color.white;
             }),
             shortWait,
             Toolbox.CoroutineFunc(() => {
                 inputNumerals[1].color = waveformColor;
                 inputNumerals[2].color = Color.white;
             }),
             shortWait,
             Toolbox.CoroutineFunc(() => {
                 inputNumerals[2].color = waveformColor;
                 inputNumerals[3].color = Color.white;
             }),
             shortWait,
             Toolbox.CoroutineFunc(() => {
                 inputNumerals[3].color = waveformColor;
             })
         );
        yield return wait;
        InitializeInputChip(currentInputChip);
    }

    IEnumerator FailDigitInput() {
        foreach (Button button in buttons) {
            button.interactable = false;
        }

        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(1f);
        yield return wait;
        foreach (TextMeshProUGUI numeral in inputNumerals) {
            numeral.color = Color.red;
        }
        yield return Toolbox.Ease(null, 0.5f, 200f, 0f, PennerDoubleAnimation.ElasticEaseOut, (amount) => {
            inputNumeralLayoutGroup.padding.left = (int)amount;
            inputNumeralLayoutGroup.padding.right = -(int)amount;
        });
        // }
        InitializeInputChip(currentInputChip);

    }

    IEnumerator AnimateWireDisplay() {
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(0.3f);
        while (true) {
            wireDisplayText.text = "scanning...";
            yield return wait;
            wireDisplayText.text = "scanning";
            yield return wait;
            wireDisplayText.text = "scanning.";
            yield return wait;
            wireDisplayText.text = "scanning..";
            yield return wait;
        }
    }
}
