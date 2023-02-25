using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TimerManager;
using DG.Tweening;

public class MainMenu : MonoBehaviour
{
    public GameDifficulty gameDifficulty = GameDifficulty.Easy;
    public bool showCheeseLocations = false;

    public Button playButton;
    public Button easyButton;
    public Button normalButton;
    public Button hardButton;
    public Button insaneButton;
    public Button controlsButton;
    public Button creditsButton;
    public Button quitButton;
    public Button cheeseButton;

    public Image controlsWindow;
    public Image creditsWindow;

    public AudioSource musicAudioSource;
    public AudioSource buttonAudioSource;
    public AudioSource playButtonAudioSource;

    public GameObject mainMenuCanvas;
    public GameObject screenTransitionCanvas;
    public Image screenTransitionImage;

    public static MainMenu Instance;

    private bool changingScene;

    public enum GameDifficulty {
        Easy,
        Normal,
        Hard,
        Insane
    }

    private void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("MainMenuScene"));
        SetDefaultDifficulty();
        showCheeseLocations = false;
    }

    private void SetDefaultDifficulty() {
        if (gameDifficulty == GameDifficulty.Easy) {
            Easy();
        } else if (gameDifficulty == GameDifficulty.Normal) {
            Normal();
        } else if (gameDifficulty == GameDifficulty.Hard) {
            Hard();
        } else if (gameDifficulty == GameDifficulty.Insane) {
            Insane();
        }
    }

    public void Easy() {
        if (changingScene) { return; }
        ResetPreviousColor();
        gameDifficulty = GameDifficulty.Easy;
        easyButton.GetComponent<Image>().color = new Color32(0, 255, 0, 255);
        buttonAudioSource.Play();
    }

    public void Normal() {
        if (changingScene) { return; }
        ResetPreviousColor();
        gameDifficulty = GameDifficulty.Normal;
        normalButton.GetComponent<Image>().color = new Color32(0, 255, 0, 255);
        buttonAudioSource.Play();
    }

    public void Hard() {
        if (changingScene) { return; }
        ResetPreviousColor();
        gameDifficulty = GameDifficulty.Hard;
        hardButton.GetComponent<Image>().color = new Color32(0, 255, 0, 255);
        buttonAudioSource.Play();
    }

    public void Insane() {
        if (changingScene) { return; }
        ResetPreviousColor();
        gameDifficulty = GameDifficulty.Insane;
        insaneButton.GetComponent<Image>().color = new Color32(0, 255, 0, 255);
        buttonAudioSource.Play();
    }

    public void Cheese() {
        if (changingScene) { return; }
        showCheeseLocations = !showCheeseLocations;
        if (showCheeseLocations) {
            cheeseButton.GetComponent<Image>().color = new Color32(0, 255, 0, 255);
        } else {
            cheeseButton.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        }
        buttonAudioSource.Play();
    }

    public void Controls() {
        if (changingScene) { return; }
        if (!controlsWindow.isActiveAndEnabled) {
            // Current menu
            controlsButton.GetComponent<Image>().color = new Color32(0, 255, 0, 255);
            controlsWindow.gameObject.SetActive(true);
            // Other menu
            creditsButton.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            creditsWindow.gameObject.SetActive(false);
        } else {
            controlsButton.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            controlsWindow.gameObject.SetActive(false);
        }
        buttonAudioSource.Play();
    }

    public void Credits() {
        if (changingScene) { return; }
        if (!creditsWindow.isActiveAndEnabled) {
            // Current menu
            creditsButton.GetComponent<Image>().color = new Color32(0, 255, 0, 255);
            creditsWindow.gameObject.SetActive(true);
            // Other menu
            controlsButton.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            controlsWindow.gameObject.SetActive(false);
        } else {
            creditsButton.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            creditsWindow.gameObject.SetActive(false);
        }
        buttonAudioSource.Play();
    }

    public void Play() {
        if (changingScene) { return; }
        changingScene = true;

        playButton.GetComponent<Image>().color = new Color32(0, 255, 0, 255);

        playButton.enabled = false;
        easyButton.enabled = false;
        normalButton.enabled = false;
        hardButton.enabled = false;
        insaneButton.enabled = false;
        controlsButton.enabled = false;
        creditsButton.enabled = false;
        quitButton.enabled = false;
        cheeseButton.enabled = false;
        controlsWindow.gameObject.SetActive(false);
        creditsWindow.gameObject.SetActive(false);

        playButtonAudioSource.Play();

        StartCoroutine(AudioFadeOut.FadeOut(musicAudioSource, 0.90f));

        ScreenTransitionOut();

        Timer.Register(1.00f, false, () => {
            SceneManager.LoadScene("LevelScene");
        });
    }

    public void Quit() {
        if (changingScene) { return; }
        Application.Quit();
    }

    public void ReturnToMainMenu() {
        SceneManager.LoadScene("MainMenuScene");
        mainMenuCanvas.SetActive(true);
        playButton.enabled = true;
        easyButton.enabled = true;
        normalButton.enabled = true;
        hardButton.enabled = true;
        insaneButton.enabled = true;
        controlsButton.enabled = true;
        creditsButton.enabled = true;
        quitButton.enabled = true;
        cheeseButton.enabled = true;
        playButton.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        controlsButton.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        creditsButton.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        changingScene = false;
    }

    public void ScreenTransitionOut() {
        screenTransitionCanvas.SetActive(true);
        screenTransitionImage.transform.DOKill();
        screenTransitionImage.transform.DOScale(1000f, 1f);
    }

    public void ScreenTransitionIn() {
        screenTransitionImage.transform.DOKill();
        screenTransitionImage.transform.DOScale(0f, 1f).OnComplete(() => { screenTransitionCanvas.SetActive(false); });
    }

    private void ResetPreviousColor() {
        if (gameDifficulty == GameDifficulty.Easy) {
            easyButton.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        } else if (gameDifficulty == GameDifficulty.Normal) {
            normalButton.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        } else if (gameDifficulty == GameDifficulty.Hard) {
            hardButton.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        } else if (gameDifficulty == GameDifficulty.Insane) {
            insaneButton.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        }
    }
}
