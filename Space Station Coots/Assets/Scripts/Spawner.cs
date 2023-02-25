using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public SpawnType spawnType;
    public List<GameObject> neighbors = new();
    public bool isDisabled;

    public enum SpawnType {
        Cheese,
        Coots,
        Player,
    }

    private void Awake() {
        isDisabled = false;
    }

    public void DisableNeighbors() {
        foreach (var spawner in neighbors) {
            spawner.GetComponent<Spawner>().isDisabled = true;
        }
    }

    public void FindNeighbors() {
        // Check for collisions
        float radius = 0.5f;
        var hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var hit in hits) {
            if (hit.transform.TryGetComponent(out Spawner s) && hit.transform != transform) {
                if (!neighbors.Contains(s.gameObject)) {
                    neighbors.Add(s.gameObject);
                }
            }
        }
    }
}
