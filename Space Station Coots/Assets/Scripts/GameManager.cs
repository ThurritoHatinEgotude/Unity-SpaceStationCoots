using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TimerManager;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private void Start() {
        StartTheGame();
    }

    public void StartTheGame() {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("LevelScene"));

        // Debug only:
        if (MainMenu.Instance == null) {
            //Globals.Instance.gameObject.AddComponent<TimerManager.TimerManager>();

            SpawnGameObjects(3, 6, 1f, 1.5f, 3);
            Globals.Instance.gameDifficulty = MainMenu.GameDifficulty.Easy;
            return;
        }

        MainMenu.Instance.mainMenuCanvas.SetActive(false);
        MainMenu.Instance.ScreenTransitionIn();

        // ( Cheese Count, Coots Count, Coots Move Speed, Coots Linger Duration, Player Starting Life )
        if (MainMenu.Instance.gameDifficulty == MainMenu.GameDifficulty.Easy) {
            SpawnGameObjects(3, 6, 1f, 1.5f, 3);

        } else if (MainMenu.Instance.gameDifficulty == MainMenu.GameDifficulty.Normal) {
            SpawnGameObjects(6, 8, 1.25f, 2f, 3);

        } else if (MainMenu.Instance.gameDifficulty == MainMenu.GameDifficulty.Hard) {
            SpawnGameObjects(8, 12, 1.5f, 2.5f, 2);

        } else if (MainMenu.Instance.gameDifficulty == MainMenu.GameDifficulty.Insane) {
            SpawnGameObjects(12, 16, 1.75f, 2.5f, 1);
        }

        // Carry over difficulty settings from main menu
        Globals.Instance.difficulty_ShowCheeseLocations = MainMenu.Instance.showCheeseLocations;
        Globals.Instance.gameDifficulty = MainMenu.Instance.gameDifficulty;

        PauseMenu.Instance.UnpauseGame(); // In case the game was paused from a previous session
    }

    private void SpawnGameObjects(int cheeseCount, int cootsCount, float cootsMoveSpeed, float cootsLingerDuration, int playerStartingLife) {
        Globals.Instance.difficulty_CheeseCount = cheeseCount;
        Globals.Instance.difficulty_CootsCount = cootsCount;
        Globals.Instance.difficulty_CootsMoveSpeed = cootsMoveSpeed;
        Globals.Instance.difficulty_CootsLingerDuration = cootsLingerDuration;
        Globals.Instance.difficulty_PlayerStartingLife = playerStartingLife;

        // Initialize cheese text
        Globals.Instance.cheeseFound = 0;
        Globals.Instance.cheeseCountText.text = $"{Globals.Instance.cheeseFound}/{Globals.Instance.difficulty_CheeseCount}";

        // Add adjacent neighbors that weren't already in the list
        foreach (Transform t in Globals.Instance.playerSpawns) {
            t.GetComponent<Spawner>().FindNeighbors();
        }

        foreach (Transform t in Globals.Instance.cheeseSpawns) {
            t.GetComponent<Spawner>().FindNeighbors();
        }

        foreach (Transform t in Globals.Instance.cootsSpawns) {
            t.GetComponent<Spawner>().FindNeighbors();
        }

        // Hide them so we don't see them when the player spawns in
        foreach (Transform t in Globals.Instance.playerSpawns) {
            t.gameObject.SetActive(false);
        }

        foreach (Transform t in Globals.Instance.cheeseSpawns) {
            t.gameObject.SetActive(false);
        }

        foreach (Transform t in Globals.Instance.cootsSpawns) {
            t.gameObject.SetActive(false);
        }

        // Instantiate the player first, then the cheese and coots
        SpawnPlayer();
        Timer.Register(1.00f, false, () => {
            SpawnCheese();
            SpawnCoots();

            // Turn off all of the unused spawns
            Globals.Instance.cheeseSpawns.gameObject.SetActive(false);
            Globals.Instance.cootsSpawns.gameObject.SetActive(false);
            Globals.Instance.playerSpawns.gameObject.SetActive(false);

            // Initialize spike traps
            SpikeTrap.InitializeTraps();

            // Enable player (game has offically started)
            Globals.Instance.player.GetComponent<Status>().Unstun();

            Globals.Instance.StartGameTimer();
        });
    }

    private void SpawnPlayer() {
        // Player
        var playerList = new List<Transform>();
        foreach (Transform t in Globals.Instance.playerSpawns) {
            playerList.Add(t);
        }
        var playerSpawner = playerList[Random.Range(0, playerList.Count)];
        var player = Instantiate(Globals.Instance.playerPrefab, playerSpawner.position, Quaternion.identity);
        Globals.Instance.player = player;
        player.transform.position = new Vector3(playerSpawner.position.x, 0.025f, playerSpawner.position.z);
        player.transform.rotation = playerSpawner.rotation;
        player.GetComponent<PlayerLife>().SetStartingLife(Globals.Instance.difficulty_PlayerStartingLife);
        Globals.Instance.playerMainCam = player.GetComponentInChildren<Camera>();
        Globals.Instance.playerCinemachineCam = player.GetComponentInChildren<CinemachineVirtualCamera>();
        Globals.Instance.playerCamTarget = player.transform.GetChild(0);
        playerSpawner.GetComponent<Spawner>().DisableNeighbors();

        // Disable player temporarily
        player.GetComponent<Status>().Stun();
    }
    
    private void SpawnCheese() {
        // Cheese
        var cheeseList = new List<Transform>();
        foreach (Transform t in Globals.Instance.cheeseSpawns) {
            cheeseList.Add(t);
        }

        int total = 0;
        while (total < Globals.Instance.difficulty_CheeseCount) {
            if (cheeseList.Count == 0) { break; }
            var cheeseSpawner = cheeseList[Random.Range(0, cheeseList.Count)];

            // We found a dud, remove it and try again
            if (cheeseSpawner.GetComponent<Spawner>().isDisabled) {
                cheeseList.Remove(cheeseSpawner);
                continue;
            }

            // Create the cheese
            var cheese = Instantiate(Globals.Instance.cheesePrefab, cheeseSpawner.position, Quaternion.identity);
            Globals.Instance.allCheeseList.Add(cheese);

            // Show cheese indicators
            if (Globals.Instance.difficulty_ShowCheeseLocations) {
                cheese.transform.GetChild(0).gameObject.SetActive(true);
            }

            cheeseList.Remove(cheeseSpawner);
            cheeseSpawner.GetComponent<Spawner>().DisableNeighbors();
            total++;
        }
    }

    private void SpawnCoots() {
        // Coots
        var cootsList = new List<Transform>();
        foreach (Transform t in Globals.Instance.cootsSpawns) {
            cootsList.Add(t);
        }

        int total = 0;
        while (total < Globals.Instance.difficulty_CootsCount) {
            if (cootsList.Count == 0) { break; }
            var cootsSpawner = cootsList[Random.Range(0, cootsList.Count)];

            // We found a dud, remove it and try again
            if (cootsSpawner.GetComponent<Spawner>().isDisabled) {
                cootsList.Remove(cootsSpawner);
                continue;
            }

            // Create coots
            var coots = Instantiate(Globals.Instance.cootsPrefab, cootsSpawner.position, Quaternion.identity);
            Globals.Instance.allCootsList.Add(coots);

            cootsList.Remove(cootsSpawner);
            cootsSpawner.GetComponent<Spawner>().DisableNeighbors();
            total++;
        }
    }
}

public static class AudioFadeOut {

    public static IEnumerator FadeOut(AudioSource audioSource, float FadeTime) {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0) {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;

            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }

}
