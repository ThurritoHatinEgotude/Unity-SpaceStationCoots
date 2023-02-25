using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TimerManager;
using DG.Tweening;

public class TurboAbility : MonoBehaviour
{
    private float duration = 3f;
    private float cooldown = 11f;
    private bool abilityIsReady = true;
    private RectTransform icon;
    private RectTransform cooldownIcon;
    private Vector3 cooldownPosition;

    private PlayerMovement playerMovement;
    private Status status;

    public AudioSource turboAudioSource;
    public AudioSource turboCooldownAudioSource;

    private void Awake() {
        var turboUI = Globals.Instance.turboAbility;
        icon = turboUI.GetComponent<RectTransform>();
        cooldownIcon = turboUI.transform.GetChild(1).GetComponent<RectTransform>();
        cooldownPosition = cooldownIcon.localPosition;
        cooldownIcon.gameObject.SetActive(false);
        playerMovement = GetComponent<PlayerMovement>();
        status = GetComponent<Status>();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.E)) {
            ActivateAbility();
        }
    }

    private void ActivateAbility() {
        if (abilityIsReady && !status.IsStunned() && Globals.Instance.gameState == Globals.GameState.Playing) {
            // Apply the actual effects of the ability
            playerMovement.TurboBoostOn();

            abilityIsReady = false;
            cooldownIcon.gameObject.SetActive(true);
            cooldownIcon.DOKill();
            icon.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.2f).OnComplete(() => { icon.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 0.2f); });
            cooldownIcon.DOLocalMoveX(175f, cooldown + 2.4f, true); // The visual is slightly off so we need add some time to it

            turboAudioSource.Play();

            // Reset the effects of the ability
            Timer.Register(duration, false, () => {
                playerMovement.TurboBoostOff();
            });

            Timer.Register(cooldown, false, () => {
                CooldownFinished();
            });
        }
    }

    private void CooldownFinished() {
        abilityIsReady = true;
        cooldownIcon.DOKill();
        cooldownIcon.gameObject.SetActive(false);
        cooldownIcon.localPosition = cooldownPosition;
        icon.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.2f).OnComplete(() => { icon.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 0.2f); });
        turboCooldownAudioSource.Play();
    }
}
