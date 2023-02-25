using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    private bool gameIsPaused = false;

    public static PauseMenu Instance;

    private void Awake() {
        Instance = this;
    }

    private void Update() {
        if (Globals.Instance == null || Globals.Instance.gameState != Globals.GameState.Playing) {
            return;
        }

        // Pause/resume game
        if (Input.GetKeyDown(KeyCode.Escape)) {
            gameIsPaused = !gameIsPaused;
            if (gameIsPaused) {
                Time.timeScale = 0;
                AudioListener.volume = 0;
                Globals.Instance.pauseMenu.SetActive(true);
            } else {
                Time.timeScale = 1;
                AudioListener.volume = 1;
                Globals.Instance.pauseMenu.SetActive(false);
            }
        }
    }

    public void QuitTheGame() {
        // Exit the game entirely
        Application.Quit();
    }

    public void ReturnToMenu() {
        UnpauseGame();

        // Kill all ongoing stuff
        DOTween.KillAll();
        TimerManager.TimerManager.KillAll();
        // Load main menu scene
        if (MainMenu.Instance != null) {
            MainMenu.Instance.ReturnToMainMenu();
        } else {
            SceneManager.LoadScene("MainMenuScene");
        }
    }

    public void UnpauseGame() {
        // Unpause game
        Time.timeScale = 1;
        AudioListener.volume = 1;
        if (Globals.Instance != null) {
            Globals.Instance.pauseMenu.SetActive(false);
        }
        gameIsPaused = false;
    }
}
