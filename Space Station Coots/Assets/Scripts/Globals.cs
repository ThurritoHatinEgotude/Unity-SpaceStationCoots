using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static MainMenu;
using UnityEngine.UI;
using Cinemachine;
using TimerManager;

public class Globals : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject cootsPrefab;
    public GameObject cheesePrefab;
    public GameObject cheeseParticlePrefab;
    public GameObject player;
    public GameObject teleporter;
    public GameObject ground;
    public GameObject turboAbility;
    public GameObject pauseMenu;
    public Transform domeCollider;
    public BoxCollider domeBaseCollider;
    public AudioSource musicAudioSource;
    public TMP_Text teleporterActiveText;
    public TMP_Text cheeseCountText;
    public TMP_Text gameOverText;
    public TMP_Text gameOverTipText;
    public TMP_Text victoryText;
    public TMP_Text spaceBarText;
    public List<Image> heartImages = new();
    public Image cheeseImage;
    public Sprite heartGreySprite;
    public Sprite heartSprite;
    public Camera playerMainCam;
    public CinemachineVirtualCamera playerCinemachineCam;
    public Transform playerCamTarget;
    public Transform cheeseSpawns;
    public Transform cootsSpawns;
    public Transform playerSpawns;
    public Transform traps;
    public LayerMask cootsLayer;
    public LayerMask detectableLayer;
    public GameDifficulty gameDifficulty;
    public float difficulty_CootsMoveSpeed;
    public float difficulty_CootsLingerDuration;
    public int difficulty_CootsCount;
    public int difficulty_CheeseCount;
    public int difficulty_PlayerStartingLife;
    public bool difficulty_ShowCheeseLocations;
    public int cheeseFound = 0;
    public List<GameObject> allCootsList = new();
    public List<GameObject> allCheeseList = new();
    public List<SpikeTrap> spikeTrapList = new();

    private static int gameCentiseconds;
    private static int gameSeconds;
    private static int gameMinutes;
    private static int gameHours;

    public static Globals Instance;

    public GameState gameState;
    public enum GameState {
        Playing,
        Victory,
        Defeat,
    }

    private void Awake() {
        Instance = this;
        cheeseFound = 0;
    }

    public void StartGameTimer() {
        gameCentiseconds = 0; // 1/100th
        gameSeconds = 0;
        gameMinutes = 0;
        gameHours = 0;

        Timer.Register(0.01f, true, () => {
            gameCentiseconds++;
            if (gameCentiseconds == 100) {
                gameCentiseconds = 0;
                gameSeconds++;
                if (gameSeconds == 60) {
                    gameSeconds = 0;
                    gameMinutes++;
                    if (gameMinutes == 60) {
                        gameMinutes = 0;
                        gameHours++;
                    }
                }
            }

            if (gameState != GameState.Playing) {
                Timer.GetExpiredTimer().Cancel();
            }
        });
    }

    public string GetGameElapsedTime() {
        var gameElapsedTime = gameHours.ToString().PadLeft(2, '0') +
        "h:" + gameMinutes.ToString().PadLeft(2, '0') +
        "m:" + gameSeconds.ToString().PadLeft(2, '0') +
        "." + gameCentiseconds.ToString().PadLeft(2, '0') + "s";
        return gameElapsedTime;
    }
}
