using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TimerManager;

public class Status : MonoBehaviour
{
    private int stunCounter;

    public void Stun() {
        stunCounter++;
        if (stunCounter == 1) {
            if (TryGetComponent(out PlayerMovement pm)) {
                pm.DisableMovement();
            } else if (TryGetComponent(out CootsMovement cm)) {
                cm.DisableMovement();
            }
        };
    }

    public void StunTimed(float duration) {
        stunCounter++;
        if (stunCounter == 1) {
            if (TryGetComponent(out PlayerMovement pm)) {
                pm.DisableMovement();
            } else if (TryGetComponent(out CootsMovement cm)) {
                cm.DisableMovement();
            }
        };

        Timer.Register(duration, false, () => {
            Unstun();
        });
    }

    public void Unstun() {
        stunCounter--;
        if (stunCounter < 0) {
            stunCounter = 0;
        }
        if (stunCounter == 0) {
            if (TryGetComponent(out PlayerMovement pm)) {
                pm.EnableMovement();
            } else if (TryGetComponent(out CootsMovement cm)) {
                cm.EnableMovement();
            }
        };
    }

    public bool IsStunned() {
        return stunCounter > 0;
    }
}
