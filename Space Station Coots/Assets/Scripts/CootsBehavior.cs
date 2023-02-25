using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TimerManager;

public class CootsBehavior : MonoBehaviour 
{
    public int numRaycasts = 100;
    public float coneAngle = 45f;
    public float coneDistance = 6f;
    public Transform conePosition;
    public LayerMask detectableLayer;
    public CurrentBehavior currentBehavior;
    public List<Sprite> spriteFaces = new();
    public SpriteRenderer spriteRenderer;
    public GameObject redFaceLight;
    public GameObject redEyeLeft;
    public GameObject redEyeRight;
    public GameObject redLightOn;
    public GameObject redLightOff;
    public GameObject blueLightOn;
    public GameObject blueLightOff;

    public AudioSource sirensAudioSource;
    public AudioSource attackAudioSource;

    private List<Transform> targets = new();
    private Transform target;
    private float attackRotationSpeed = 45f;
    private RaycastHit lineOfSightHit;
    private Timer alarmTimer;
    private bool spawnProtection = true;

    private CootsMovement Pathfinding;

    public enum CurrentBehavior {
        Searching,
        Chasing,
        Attacking,
        Lingering,
    }

    public enum Faces {
        Normal,
        Angry,
        Pissed,
    }

    private void Awake() {
        currentBehavior = CurrentBehavior.Searching;
        Pathfinding = GetComponent<CootsMovement>();
    }

    private void Start() {
        Timer.Register(1f, false, () => {
            spawnProtection = false;
        });
    }

    private void FixedUpdate() {
        if (Globals.Instance.gameState != Globals.GameState.Playing || spawnProtection) { return; }

        if (target != null) {

            if (redFaceLight.activeSelf && target) {
                Vector3 direction = target.position - redFaceLight.transform.position;
                Quaternion rotation = Quaternion.LookRotation(direction);
                redFaceLight.transform.rotation = Quaternion.RotateTowards(redFaceLight.transform.rotation, rotation, 180f * Time.fixedDeltaTime);
            }

            // Rotate towards a target it just attacked
            if (currentBehavior == CurrentBehavior.Attacking) {
                Vector3 direction = target.position - transform.position;
                Quaternion rotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, attackRotationSpeed * Time.fixedDeltaTime);
                return;
            }

            // Linger after a lost target for 2 seconds
            if (currentBehavior == CurrentBehavior.Lingering && target) {
                Pathfinding.SetTargetAsDestination(target);
                return;
            }

        }

        // Proceed with normal searching
        CheckLineOfSight();
    }

    private void CheckLineOfSight() {
        RaycastForTargets();

        // Get the final closest target to chase after
        if (targets.Count > 0) {
            // Found a player
            if (currentBehavior == CurrentBehavior.Searching) {
                SetFace(Faces.Angry);
            }
            currentBehavior = CurrentBehavior.Chasing;
            target = GetClosestTarget();
            Pathfinding.SetTargetAsDestination(target);

            targets.Clear();
        } else {
            // Lost target
            if (currentBehavior == CurrentBehavior.Chasing && target != null) {
                currentBehavior = CurrentBehavior.Lingering;
                var lingerTimer = Timer.Register(Globals.Instance.difficulty_CootsLingerDuration, false, () => {
                    RaycastForTargets();
                    if (targets.Count == 0) {
                        // Found nothing
                        target = null;
                        currentBehavior = CurrentBehavior.Searching;
                        Pathfinding.SetRandomDestination();
                        SetFace(Faces.Normal);
                    } else {
                        // Found a player
                        target = GetClosestTarget();
                        currentBehavior = CurrentBehavior.Chasing;
                        Pathfinding.SetTargetAsDestination(target);
                        SetFace(Faces.Angry);

                        targets.Clear();
                    }
                });
            } else if (currentBehavior == CurrentBehavior.Searching) {
                // No target
                target = null;
            } else if (currentBehavior == CurrentBehavior.Chasing && target == null) {
                // Reset (may fix bugs)
                currentBehavior = CurrentBehavior.Searching;
                Pathfinding.SetRandomDestination();
                SetFace(Faces.Normal);
            }
        }
    }

    private void RaycastForTargets() {
        targets.Clear();

        // Calculate the half angle of the cone in radians
        float halfAngle = coneAngle * 0.5f * Mathf.Deg2Rad;

        // Start the loop at the middle of the cone and iterate in both directions
        int middleIndex = numRaycasts / 2;

        for (int i = -middleIndex; i <= middleIndex; i++) {

            // Calculate the direction of the raycast
            float angle = i * (coneAngle / (numRaycasts - 1)) - halfAngle;
            Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;

            // Does the ray intersect any objects
            if (Physics.Raycast(conePosition.position, direction, out lineOfSightHit, coneDistance, detectableLayer)) {
                if (lineOfSightHit.collider.CompareTag("Player")) {
                    if (!targets.Contains(lineOfSightHit.transform)) {
                        targets.Add(lineOfSightHit.transform);
                    }
                }
            }
        }
    }

    private void DamagePlayer(Transform player) {
        if (player.GetComponent<PlayerLife>().IsInvulnerable()) { return; }
        target = player;

        attackAudioSource.Play();

        SetFace(Faces.Pissed);
        target.GetComponent<PlayerLife>().TakeDamage(transform, true);
        currentBehavior = CurrentBehavior.Attacking;

        Pathfinding.DisableMovement();
        Timer.Register(2f, false, () => {
            Pathfinding.EnableMovement();
            SetFace(Faces.Angry);
            currentBehavior = CurrentBehavior.Chasing;
        });
    }

    public void AttackPlayer(Transform player) {
        if (currentBehavior == CurrentBehavior.Attacking) { return; }

        if (player.CompareTag("Player")) {
            if (player != target) {
                attackRotationSpeed = 180f;
            } else {
                attackRotationSpeed = 45f;
            }
            DamagePlayer(player);
        }
    }

    private Transform GetClosestTarget() {
        // Store the closest game object's transform
        Transform closestTransform = null;

        // Store the minimum distance
        float minDistance = float.MaxValue;

        foreach (Transform target in targets) {
            // Calculate the distance between the game object's position and this transform
            float distance = Vector3.Distance(target.position, transform.position);

            // Check if the distance is smaller than the minimum distance
            if (distance < minDistance) {
                // Update the minimum distance and the closest game object
                minDistance = distance;
                closestTransform = target;
            }
        }

        // Return the closest game object's transform
        return closestTransform;
    }

    private void SetFace(Faces newFace) {
        if (newFace == Faces.Normal) {
            spriteRenderer.sprite = spriteFaces[0];
            StopAlarm();
        } else if (newFace == Faces.Angry) {
            spriteRenderer.sprite = spriteFaces[1];
            StartAlarm();
        } else if (newFace == Faces.Pissed) {
            spriteRenderer.sprite = spriteFaces[2];
            StartAlarm();
        }
    }

    private void StartAlarm() {
        if (!isActiveAndEnabled) { return; }

        if (alarmTimer != null) {
            return;
        }

        if (!sirensAudioSource.isPlaying) {
            sirensAudioSource.Play();
        }

        redFaceLight.SetActive(true);
        redEyeLeft.SetActive(true);
        redEyeRight.SetActive(true);

        bool isRed = false;
        alarmTimer = Timer.Register(0.45f, true, () => {
            isRed = !isRed;
            if (isRed) {
                blueLightOff.SetActive(true);
                blueLightOn.SetActive(false);
                redLightOff.SetActive(false);
                redLightOn.SetActive(true);
            } else {
                redLightOff.SetActive(true);
                redLightOn.SetActive(false);
                blueLightOff.SetActive(false);
                blueLightOn.SetActive(true);
            }
        });
    }

    private void StopAlarm() {
        if (sirensAudioSource.isPlaying) {
            sirensAudioSource.Stop();
        }

        redFaceLight.SetActive(false);
        redEyeLeft.SetActive(false);
        redEyeRight.SetActive(false);

        if (alarmTimer != null) {
            alarmTimer.Cancel();
            redLightOn.SetActive(false);
            blueLightOn.SetActive(false);
            redLightOff.SetActive(true);
            blueLightOff.SetActive(true);
            alarmTimer = null;
        }
    }
}