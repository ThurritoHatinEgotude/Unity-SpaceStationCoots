using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TimerManager;
using DG.Tweening;
using System.Linq;

public class SpikeTrap : MonoBehaviour
{
    private AudioSource spikeAudioSource;

    public static void InitializeTraps() {
        foreach (Transform trap in Globals.Instance.traps) {
            Globals.Instance.spikeTrapList.Add(trap.GetComponentInChildren<SpikeTrap>());
        }

        foreach (var trap in Globals.Instance.spikeTrapList) {
            trap.spikeAudioSource = trap.GetComponent<AudioSource>();
        }

        Timer.Register(10f, true, () => {
            // Raise spike traps
            foreach (var spikeTrap in Globals.Instance.spikeTrapList) {
                spikeTrap.RaiseTrap();
            }
        });
    }

    public static void HideSpikes() {
        foreach (var trap in Globals.Instance.spikeTrapList) {
            trap.gameObject.SetActive(false);
        }
    }

    private void RaiseTrap() {
        if (!isActiveAndEnabled) { return; }

        spikeAudioSource.Play();

        transform.DOKill();
        transform.DOMove(new Vector3(transform.position.x, 0.6f, transform.position.z), 0.2f);

        // Lower trap
        Timer.Register(0.60f, false, () => {
            LowerTrap();
        });
    }

    private void LowerTrap() {
        if (!isActiveAndEnabled) { return; }

        transform.DOKill();
        transform.DOMove(new Vector3(transform.position.x, 0f, transform.position.z), 0.4f);
    }
}
