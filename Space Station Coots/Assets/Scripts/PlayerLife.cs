using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TimerManager;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Cinemachine;

public class PlayerLife : MonoBehaviour
{
    [SerializeField] private int lifePoints = 3;
    [SerializeField] private int lifePointsMax = 3;
    [SerializeField] private bool invulnerable = false;
    private Rigidbody rb;
    private RigidbodyConstraints rbConstraints;
    public AudioSource cheeseAudioSource;
    public AudioSource damageAudioSource;
    public AudioSource gameOverAudioSource;
    private bool returningToMenu;
    private GameObject model;
    private PhysicMaterial physicMaterial;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        rbConstraints = rb.constraints;
        model = transform.GetChild(1).gameObject;

        // Get and then null the physic material (bounciness)
        physicMaterial = GetComponent<SphereCollider>().material;
        GetComponent<SphereCollider>().material = null;
    }

    private void Update() {
        if (Globals.Instance.gameState != Globals.GameState.Defeat) { return; }

        if (Input.GetKeyDown(KeyCode.Space)) {
            ReturnToMenu();
        }
    }

    public void TakeDamage(Transform damageSource = null, bool applyKnockback = false) {
        if (invulnerable) { return; }

        damageAudioSource.Play();

        lifePoints--;

        // Make heart image greyed out
        UpdateHearts();

        if (lifePoints > 0) {
            // Become damage immune for a short moment
            Invulnerable();

            // Knockback if able
            if (damageSource != null && applyKnockback) {
                Knockback(damageSource);
            }
        } else if (lifePoints == 0) {
            // Knockback if able (crazy knockback for comedic death effect)
            if (damageSource != null && applyKnockback) {
                KnockbackDead(damageSource);
            } else {
                GetComponent<Status>().Stun();
            }
            Defeat();
        }
    }

    private void Knockback(Transform knockbackSource) {
        // Stun the player
        GetComponent<Status>().Stun();

        // Knockback
        Vector3 knockbackDirection = transform.position - knockbackSource.position;
        knockbackDirection.y = .9f; // Adjust the height of the knockback
        knockbackDirection.Normalize();
        rb.useGravity = true;
        rb.constraints &= ~RigidbodyConstraints.FreezePositionY;
        rb.AddForce(knockbackDirection * 3.0f, ForceMode.Impulse);
        Timer.Register(0.50f, false, () => {
            GetComponent<Status>().Unstun();
            rb.useGravity = false;
            rb.constraints = rbConstraints;
            transform.position = new Vector3(transform.position.x, 0.025f, transform.position.z);
        });
    }

    private void KnockbackDead(Transform knockbackSource) {
        // Stun the player
        GetComponent<Status>().Stun();

        // Knockback (extra powerful)
        Vector3 knockbackDirection = transform.position - knockbackSource.position;
        knockbackDirection.y = .6f; // Adjust the height of the knockback
        knockbackDirection.Normalize();
        rb.useGravity = true;
        rb.mass = 0.2f;
        rb.angularDrag = 0.1f;
        rb.drag = 0;
        rb.constraints = RigidbodyConstraints.None;
        rb.AddForce(knockbackDirection * 2.0f, ForceMode.Impulse);
        rb.AddTorque(new Vector3(Random.Range(30f, 60f), Random.Range(30f, 60f), Random.Range(30f, 60f)));
    }

    public void SetStartingLife(int playerStartingLife) {
        lifePoints = playerStartingLife;
        lifePointsMax = playerStartingLife;

        for (int i = 0; i < Globals.Instance.heartImages.Count; i++) {
            var h = Globals.Instance.heartImages[i];
            if (i < playerStartingLife) {
                h.gameObject.SetActive(true);
            } else {
                h.gameObject.SetActive(false);
            }
        }
    }

    public void RestoreLife(int amount) {
        cheeseAudioSource.Play();

        if (lifePoints < lifePointsMax) {
            // Currently this only works for healing a single heart
            var hp = Globals.Instance.heartImages[lifePoints];
            hp.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.2f).OnComplete(() => { hp.transform.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 0.2f); });
        }

        lifePoints += amount;
        if (lifePoints > lifePointsMax) {
            lifePoints = lifePointsMax;
        }

        // Make heart image normal again
        UpdateHearts();
    }

    public void UpdateHearts() {
        if (lifePoints == 3) {
            for (int i = 0; i < 3; i++) {
                var h = Globals.Instance.heartImages[i];
                h.sprite = Globals.Instance.heartSprite;
            }
        } else if (lifePoints == 2) {
            Globals.Instance.heartImages[0].sprite = Globals.Instance.heartSprite;
            Globals.Instance.heartImages[1].sprite = Globals.Instance.heartSprite;
            Globals.Instance.heartImages[2].sprite = Globals.Instance.heartGreySprite;
        } else if (lifePoints == 1) {
            Globals.Instance.heartImages[0].sprite = Globals.Instance.heartSprite;
            Globals.Instance.heartImages[1].sprite = Globals.Instance.heartGreySprite;
            Globals.Instance.heartImages[2].sprite = Globals.Instance.heartGreySprite;
        } else if (lifePoints == 0) {
            for (int i = 0; i < 3; i++) {
                var h = Globals.Instance.heartImages[i];
                h.sprite = Globals.Instance.heartGreySprite;
            }
        }
    }

    private void Invulnerable() {
        invulnerable = true;

        // Make the player's sphere collider ignore all of the coots box colliders
        var playerCollider = GetComponent<SphereCollider>();
        foreach (GameObject coots in Globals.Instance.allCootsList) {
            Collider[] colliders = coots.GetComponents<BoxCollider>();
            for (int i = 0; i < colliders.Length; i++) {
                Physics.IgnoreCollision(playerCollider, colliders[i], true);
            }
        }

        // Lasts 2.8 seconds (0.2 * 14)
        int counter = 0;
        Timer.Register(0.2f, true, () => {
            counter++;
            if (counter % 2 == 0) {
                model.SetActive(false);
            } else {
                model.SetActive(true);
            }
            // End the invulnerability
            if (counter == 14) {
                model.SetActive(true);
                invulnerable = false;

                // Reset the colliders
                foreach (GameObject coots in Globals.Instance.allCootsList) {
                    Collider[] colliders = coots.GetComponents<BoxCollider>();
                    for (int i = 0; i < colliders.Length; i++) {
                        Physics.IgnoreCollision(playerCollider, colliders[i], false);
                    }
                }

                Timer.GetExpiredTimer().Cancel();
            }
        });
    }

    public bool IsInvulnerable() {
        return invulnerable;
    }

    public bool IsAlive() {
        return lifePoints > 0;
    }

    private void Defeat() {
        if (Globals.Instance.gameState != Globals.GameState.Playing) { return; }

        // Kill any tweens
        DOTween.KillAll();

        // Sound
        foreach (var trap in Globals.Instance.spikeTrapList) {
            trap.GetComponent<AudioSource>().mute = true;
        }
        StartCoroutine(AudioFadeOut.FadeOut(Globals.Instance.musicAudioSource, 0.25f));
        gameOverAudioSource.Play();

        Globals.Instance.gameState = Globals.GameState.Defeat;
        Globals.Instance.spaceBarText.enabled = true;
        Globals.Instance.gameOverText.enabled = true;
        Globals.Instance.gameOverText.GetComponent<Animator>().enabled = true;
        Globals.Instance.gameOverTipText.enabled = true;

        // Enable collision for the rest of the dome
        foreach (Transform t in Globals.Instance.domeCollider) {
            t.GetComponent<BoxCollider>().enabled = true;
        }
        Globals.Instance.domeBaseCollider.enabled = true;

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

        // Reapply the bounciness physic material
        GetComponent<SphereCollider>().material = physicMaterial;

        var cam = Globals.Instance.playerCinemachineCam;
        cam.LookAt = null;
        cam.Follow = null;
        cam.transform.SetParent(Globals.Instance.ground.transform);
        cam.transform.position = new Vector3(transform.position.x, 2f, transform.position.z);

        // Make camera update every frame instead of every 0.02s
        var brain = Globals.Instance.playerMainCam.GetComponent<CinemachineBrain>();
        brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.LateUpdate;
        brain.m_BlendUpdateMethod = CinemachineBrain.BrainUpdateMethod.LateUpdate;

        Timer.Register(0.01f, true, () => {
            Vector3 direction = transform.position - cam.transform.position;
            Quaternion rotation = Quaternion.LookRotation(direction);
            cam.transform.rotation = Quaternion.RotateTowards(cam.transform.rotation, rotation, 1000f);
            cam.transform.position = Vector3.MoveTowards(cam.transform.position, transform.position, 0.01f * Mathf.Clamp(rb.velocity.magnitude, 0.15f, 5f));
        });

        // Return to main menu after 8s
        Timer.Register(7f, false, () => {
            ReturnToMenu();
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
}
