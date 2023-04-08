using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class TitleController : MonoBehaviour {
    public AudioSource audioSource;
    [Header("Components")]
    public GameObject mainMenu;
    public Canvas VRCanvas;
    public Canvas saveDialogCanvas;
    public Canvas newGameCanvas;
    public Canvas loadGameCanvas;
    public Canvas alertCanvas;
    public AlertCanvasHandler alertCanvasHandler;
    public LoadGameMenuController loadGameMenuController;
    public Canvas vrMenuCanvas;
    public Color logoTintColor;

    [Header("Intro")]
    public Image logoImage;
    public AudioClip startSound;
    public Image logoTopImage;
    public Image logoBottomImage;
    public Image flashImage;
    public RectTransform logoTopRect;
    public RectTransform logoBottomRect;

    // [Hee]
    // public GameObject VRDesignMenu;

    Coroutine introCoroutine;
    bool menusStarted;
    void Start() {
        introCoroutine = StartCoroutine(DoIntro());
        mainMenu.SetActive(false);
        VRCanvas.enabled = false;
        loadGameCanvas.enabled = false;
        newGameCanvas.enabled = false;
        saveDialogCanvas.enabled = false;
        alertCanvas.enabled = false;
        GameManager.I.TransitionToPhase(GamePhase.mainMenu);
    }

    void Update() {
        if (Keyboard.current.anyKey.isPressed || Mouse.current.leftButton.isPressed || Mouse.current.rightButton.isPressed) {
            if (introCoroutine != null) {
                StopCoroutine(introCoroutine);
                introCoroutine = null;
                StartCoroutine(FlashAndShowLogo());
            }
            if (!menusStarted) {
                menusStarted = true;
                mainMenu.SetActive(true);
                logoImage.color = logoTintColor;
            }
        }
    }
    public void NewGameCallback() {
        mainMenu.SetActive(false);
        newGameCanvas.enabled = true;
    }
    public void CloseNewGameMenu() {
        mainMenu.SetActive(true);
        newGameCanvas.enabled = false;
    }
    public void LoadGameCallback() {
        // show load menu
        mainMenu.SetActive(false);
        loadGameCanvas.enabled = true;
        loadGameMenuController.Initialize();
    }
    public void CloseLoadMenu() {
        mainMenu.SetActive(true);
        loadGameCanvas.enabled = false;
    }
    public void NewVRMissionCallback() {
        mainMenu.SetActive(false);
        VRCanvas.enabled = true;
    }
    public void CancelVRMissionCallback() {
        mainMenu.SetActive(true);
        VRCanvas.enabled = false;
    }
    public void QuitGameCallback() {
        Application.Quit();
    }

    public void ShowAlert(string alertContent) {
        newGameCanvas.enabled = false;
        alertCanvas.enabled = true;
        alertCanvasHandler.ShowAlert(alertContent);
    }
    public void AlertCancelCallback() {
        newGameCanvas.enabled = true;
        alertCanvas.enabled = false;
    }

    public IEnumerator DoIntro() {
        yield return new WaitForSecondsRealtime(0.5f);
        audioSource.PlayOneShot(startSound);
        Vector2 topStartPosition = logoTopRect.anchoredPosition;
        Vector2 bottomStartPosition = logoBottomRect.anchoredPosition;
        float logoMoveDuration = 0.6f;
        float timer = 0f;

        flashImage.enabled = false;
        logoTopImage.enabled = true;
        logoBottomImage.enabled = true;
        logoImage.enabled = false;
        while (timer < logoMoveDuration) {
            timer += Time.unscaledDeltaTime;
            float topX = (float)PennerDoubleAnimation.Linear(timer, topStartPosition.x, -1f * topStartPosition.x, logoMoveDuration);
            Vector2 newPosition = new Vector2(topX, topStartPosition.y);
            logoTopRect.anchoredPosition = newPosition;
            yield return null;
        }

        timer = 0f;
        while (timer < logoMoveDuration) {
            timer += Time.unscaledDeltaTime;
            float topX = (float)PennerDoubleAnimation.Linear(timer, bottomStartPosition.x, -1f * bottomStartPosition.x, logoMoveDuration);
            Vector2 newPosition = new Vector2(topX, bottomStartPosition.y);
            logoBottomRect.anchoredPosition = newPosition;
            yield return null;
        }

        yield return FlashAndShowLogo();

        introCoroutine = null;
    }

    IEnumerator FlashAndShowLogo() {
        flashImage.enabled = true;
        float timer = 0f;
        while (timer < 0.1f) {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        flashImage.enabled = false;
        logoTopImage.enabled = false;
        logoBottomImage.enabled = false;
        logoImage.enabled = true;
        logoImage.color = Color.white;
    }
}
