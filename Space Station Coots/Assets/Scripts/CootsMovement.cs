using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TimerManager;

public class CootsMovement : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    private Vector3 destination;
    private Vector3 previousDestination;
    private Bounds mapBounds;
    private int ignoreStoppingDistance;

    public AudioSource walkingAudioSource;

    private void Start() {
        navMeshAgent = GetComponent<NavMeshAgent>();
        MeshRenderer renderer = Globals.Instance.ground.GetComponent<MeshRenderer>();
        mapBounds = renderer.bounds;

        navMeshAgent.speed = Globals.Instance.difficulty_CootsMoveSpeed;

        SetRandomDestination();
    }

    private void FixedUpdate() {
        if (!navMeshAgent.isActiveAndEnabled) { return; }

        if (ignoreStoppingDistance <= 0) {
            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance) {
                SetRandomDestination();
            }
        } else {
            ignoreStoppingDistance--;
        }

        if (!navMeshAgent.isStopped && !walkingAudioSource.isPlaying) {
            walkingAudioSource.Play();
        } else if (navMeshAgent.isStopped) {
            walkingAudioSource.Stop();
        }
    }

    public void SetRandomDestination() {
        if (!navMeshAgent.isActiveAndEnabled) { return; }

        navMeshAgent.stoppingDistance = 1f;

        int attempts = 10;
        while (Vector3.Distance(destination, previousDestination) < 5f) {
            float x = Random.Range(mapBounds.min.x, mapBounds.max.x);
            float z = Random.Range(mapBounds.min.z, mapBounds.max.z);
            destination = new Vector3(x, 0, z); ;
            navMeshAgent.SetDestination(destination);
            attempts--;
            if (attempts == 0) { break; }
        }

        previousDestination = destination;
        ignoreStoppingDistance = 25;
    }

    public void SetTargetAsDestination(Transform target) {
        if (!navMeshAgent.isActiveAndEnabled) { return; }

        navMeshAgent.SetDestination(target.position);
        navMeshAgent.stoppingDistance = 0.01f;
    }

    public void DisableMovement() {
        navMeshAgent.enabled = false;
    }

    public void EnableMovement() {
        navMeshAgent.enabled = true;
    }

    public void DisablePathfindingTimed(float duration) {
        navMeshAgent.enabled = false;
        Timer.Register(duration, false, () => {
            navMeshAgent.enabled = true;
        });
    }
}
