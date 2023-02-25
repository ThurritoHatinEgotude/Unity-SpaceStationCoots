using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerCollision : MonoBehaviour
{
    public PlayerMovement PlayerMovement;
    public PlayerLife PlayerLife;
    public Status Status;
    public TurboAbility TurboAbility;

    private void OnTriggerStay(Collider collider) {
        if (Globals.Instance.gameState != Globals.GameState.Playing) { return; }

        // Cheese
        if (collider.CompareTag("Cheese")) {
            // Heal
            PlayerLife.RestoreLife(1);
            // Increase cheese count
            Globals.Instance.cheeseFound++;
            Globals.Instance.cheeseCountText.text = $"{Globals.Instance.cheeseFound}/{Globals.Instance.difficulty_CheeseCount}";
            // Cheese animation
            var c = Globals.Instance.cheeseImage;
            c.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.2f).OnComplete(() => { c.transform.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 0.2f); });
            if (Globals.Instance.cheeseFound == Globals.Instance.difficulty_CheeseCount) {
                Globals.Instance.teleporter.GetComponent<Teleporter>().EnableTeleporter();
            }
            // Cheese particles
            var cp = Instantiate(Globals.Instance.cheeseParticlePrefab);
            cp.transform.position = collider.transform.position;
            // Destroy cheese
            Destroy(collider.gameObject);
        }

        // Coots
        else if (collider.CompareTag("Coots")) {
            if (!PlayerLife.IsInvulnerable()) {
                collider.GetComponent<CootsBehavior>().AttackPlayer(transform);
            }
        }

        // Spike
        else if (collider.CompareTag("SpikeTrap")) {
            if (!PlayerLife.IsInvulnerable()) {
                PlayerLife.TakeDamage(collider.transform, true);
                Status.StunTimed(1f);
            }
        }

        // Teleporter
        else if (collider.CompareTag("Teleporter")) {
            if (PlayerLife.IsAlive()) {
                collider.GetComponent<Teleporter>().UseTeleporter(transform);
            }
        }
    }
}
