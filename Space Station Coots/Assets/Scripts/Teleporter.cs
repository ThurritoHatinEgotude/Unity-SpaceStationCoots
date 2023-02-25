using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TimerManager;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Cinemachine;

public class Teleporter : MonoBehaviour
{
    public GameObject orbParticles;
    public GameObject baseGO;
    public GameObject baseGlowingGO;
    public SphereCollider sphereCollider;
    public AudioSource teleportUseAudioSource;
    public AudioSource teleportActiveAudioSource;
    public AudioSource teleportEnableAudioSource;
    public AudioSource teleportVictoryAudioSource;
    private bool returningToMenu;

    private void Update() {
        if (Globals.Instance.gameState != Globals.GameState.Victory) { return; }

        if (Input.GetKeyDown(KeyCode.Space)) {
            ReturnToMenu();
        }
    }

    public void UseTeleporter(Transform player) {
        if (Globals.Instance.gameState != Globals.GameState.Playing) { return; }
        if (Globals.Instance.cheeseFound == Globals.Instance.difficulty_CheeseCount) {
            Victory();
        }
    }

    private void Victory() {
        // Kill any tweens
        DOTween.KillAll();

        // Sound
        foreach (var trap in Globals.Instance.spikeTrapList) {
            trap.GetComponent<AudioSource>().mute = true;
        }
        StartCoroutine(AudioFadeOut.FadeOut(teleportActiveAudioSource, 0.5f));
        StartCoroutine(AudioFadeOut.FadeOut(Globals.Instance.musicAudioSource, 0.5f));
        teleportUseAudioSource.Play();

        Timer.Register(1f, false, () => {
            teleportVictoryAudioSource.Play();
        });

        Globals.Instance.gameState = Globals.GameState.Victory;
        Globals.Instance.spaceBarText.enabled = true;

        // Text
        if (Globals.Instance.gameDifficulty == MainMenu.GameDifficulty.Easy) {
            Globals.Instance.teleporterActiveText.text = "Easy Completed!"; 
            Globals.Instance.victoryText.text =
                "You've escaped the evil robotic clutches of Coots!\n\nTime taken:\n" + Globals.Instance.GetGameElapsedTime();
        }

        if (Globals.Instance.gameDifficulty == MainMenu.GameDifficulty.Normal) {
            Globals.Instance.teleporterActiveText.text = "Normal Completed!";
            Globals.Instance.victoryText.text =
                "You've escaped the evil robotic clutches of Coots!\n\nTime taken:\n" + Globals.Instance.GetGameElapsedTime();
        }

        if (Globals.Instance.gameDifficulty == MainMenu.GameDifficulty.Hard) {
            Globals.Instance.teleporterActiveText.text = "Hard Completed!";
            Globals.Instance.victoryText.text =
                "You've escaped the evil robotic clutches of Coots!\n\nTime taken:\n" + Globals.Instance.GetGameElapsedTime();
        }

        if (Globals.Instance.gameDifficulty == MainMenu.GameDifficulty.Insane) {
            Globals.Instance.teleporterActiveText.text = "Insane Completed!";
            Globals.Instance.victoryText.text =
                "You've escaped the evil robotic clutches of Coots! May the blessing of nine lives be bestowed upon thee, mighty gamer.\n\nTime taken:\n" + Globals.Instance.GetGameElapsedTime();
        }

        SpikeTrap.HideSpikes();

        foreach (var coots in Globals.Instance.allCootsList) {
            if (coots != null) {
                coots.SetActive(false);
            }
        }

        foreach (var cheese in Globals.Instance.allCheeseList) {
            if (cheese != null) {
                cheese.SetActive(false);
            }
        }

        // Loop teleporting particles
        var main = Globals.Instance.player.GetComponent<PlayerMovement>().teleportingParticles.main;
        main.loop = true;
        Globals.Instance.player.GetComponent<PlayerMovement>().teleportingParticles.Play();

        // Disable and adjust the player's position
        int counter = 0;
        var p = Globals.Instance.player.transform;
        p.GetComponent<PlayerMovement>().DisableMovement();
        p.position = new Vector3(transform.position.x, p.position.y, transform.position.z);

        // Make camera update every frame instead of every 0.02s
        var brain = Globals.Instance.playerMainCam.GetComponent<CinemachineBrain>();
        brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.LateUpdate;
        brain.m_BlendUpdateMethod = CinemachineBrain.BrainUpdateMethod.LateUpdate;

        Timer.Register(0.01f, true, () => {
            p.Rotate(Vector3.up, 1f);
            p.Translate(new Vector3(0, 0.003f, 0));

            counter++;

            // After 20 seconds, fade out music
            if (counter == 2000) {
                StartCoroutine(AudioFadeOut.FadeOut(teleportVictoryAudioSource, 0.80f));
            }

            // After 21 seconds, return to main menu
            if (counter == 2100) {
                ReturnToMenu();
            }
        });
    }

    private void ReturnToMenu() {
        if (returningToMenu) { return; }
        returningToMenu = true;

        // Play transition animation
        if (MainMenu.Instance != null) {
            MainMenu.Instance.ScreenTransitionOut();
        }

        // Return to main menu in 1 second
        Timer.Register(1f, false, () => {
            // Cancel all timers
            TimerManager.TimerManager.KillAll();

            if (MainMenu.Instance != null) {
                MainMenu.Instance.ReturnToMainMenu();
                MainMenu.Instance.ScreenTransitionIn();
            } else {
                SceneManager.LoadScene("MainMenuScene");
            }
        });
    }

    public void EnableTeleporter() {
        teleportActiveAudioSource.Play();
        teleportEnableAudioSource.Play();

        baseGO.SetActive(false);
        baseGlowingGO.SetActive(true);
        orbParticles.SetActive(true);
        sphereCollider.enabled = true;
        Globals.Instance.teleporterActiveText.enabled = true;
        Globals.Instance.teleporterActiveText.GetComponent<Animator>().enabled = true;
    }
}
