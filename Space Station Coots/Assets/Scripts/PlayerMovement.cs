using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TimerManager;
using DG.Tweening;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    public float forwardAccel = 1f, reverseAccel = 1f, maxForwardSpeed = 2f, maxReverseSpeed = 2f, turnStrength = 180f, maxWheelTurn = 25f;
    public Transform leftFrontWheel, rightFrontWheel, leftBackWheel, rightBackWheel;
    public AudioSource motorAudioSource;

    public ParticleSystem teleportingParticles;
    public ParticleSystem turboSmokeParticles;
    public ParticleSystem turboBoostParticles;

    private float speedInput, turnInput, axisVertical, turboMultiplier = 1.0f;
    private bool isDisabled;
    private bool isMoving;
    private List<Transform> wheels = new();

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    private void Start() {
        wheels.Add(leftFrontWheel);
        wheels.Add(rightFrontWheel);
        wheels.Add(leftBackWheel);
        wheels.Add(rightBackWheel);

        teleportingParticles.Play();
        Globals.Instance.teleporter.GetComponent<Teleporter>().teleportUseAudioSource.Play();

        // Motor audio is always playing but the volume changes
        motorAudioSource.volume = 0;
        motorAudioSource.Play();
    }

    private void Update() {
        if (isDisabled) { return; }

        turnInput = Input.GetAxis("Horizontal");
        axisVertical = Input.GetAxis("Vertical");

        speedInput = 0f;
        if (axisVertical > 0) {
            speedInput = axisVertical * forwardAccel * 100f;
            PlayMotorAudio();
            PlayTurboParticles();
            isMoving = true; // Important that this happens after playing
        } else if (axisVertical < 0) {
            speedInput = axisVertical * reverseAccel * 100f;
            PlayMotorAudio();
            PlayTurboParticles();
            isMoving = true; // Important that this happens after playing
        } else {
            StopMotorAudio();
            StopTurboParticles();
            isMoving = false; // Important that this happens after stopping
        }

        if (turnInput != 0) {
            // This allows turning even while idle
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrength * Time.deltaTime, 0f));

            // This prevents turning while idle
            //transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrength * Time.deltaTime * axisVertical, 0f));

            // Rotate front wheels towards turn direction
            leftFrontWheel.localRotation = Quaternion.Euler(leftFrontWheel.localRotation.eulerAngles.x, (turnInput * maxWheelTurn) - 180, leftFrontWheel.localRotation.eulerAngles.z);
            rightFrontWheel.localRotation = Quaternion.Euler(rightFrontWheel.localRotation.eulerAngles.x, (turnInput * maxWheelTurn), rightFrontWheel.localRotation.eulerAngles.z);
        }
    }

    private void FixedUpdate() {
        if (isDisabled) { return; }

        if (speedInput > 0) {

            // Apply moving force
            rb.AddForce(transform.forward * turboMultiplier * speedInput);

            // Prevent it from going over the max speed limit
            if (rb.velocity.magnitude > maxForwardSpeed * turboMultiplier) { rb.velocity = rb.velocity.normalized * maxForwardSpeed * turboMultiplier; }

            foreach (var wheel in wheels) {
                wheel.Rotate(Vector3.right, axisVertical * 100f);
            }

        } else if (speedInput < 0) {

            // Apply moving force
            rb.AddForce(transform.forward * turboMultiplier * speedInput);

            // Prevent it from going over the max speed limit
            if (rb.velocity.magnitude > maxReverseSpeed * turboMultiplier) { rb.velocity = rb.velocity.normalized * maxReverseSpeed * turboMultiplier; }

            foreach (var wheel in wheels) {
                wheel.Rotate(Vector3.right, axisVertical * 100f);
            }
        }
    }

    public void EnableMovement() {
        isDisabled = false;
    }

    public void DisableMovement() {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        isDisabled = true;
        StopTurboParticles();
        // I'm manually reducing the volume here
        motorAudioSource.volume = 0f;
        isMoving = false;
    }

    private void PlayTurboParticles() {
        if (turboMultiplier > 1.1f) {
            turboSmokeParticles.Play();
            turboBoostParticles.Play();
        }
    }

    private void StopTurboParticles() {
        if (turboMultiplier > 1.1f) {
            turboSmokeParticles.Stop();
            turboBoostParticles.Stop();
        }
    }

    private void PlayMotorAudio() {
        if (!isMoving) {
            float motorTargetVolume;
            if (turboMultiplier < 1.1f) {
                motorTargetVolume = 0.4f;
            } else {
                motorTargetVolume = 0.8f;
            }
            // Increase volume
            motorAudioSource.DOKill();
            motorAudioSource.DOFade(motorTargetVolume, 0.20f).OnComplete(() => { motorAudioSource.volume = motorTargetVolume; } );
        }
    }

    private void StopMotorAudio() {
        if (isMoving) {
            // Reduce volume
            motorAudioSource.DOKill();
            motorAudioSource.DOFade(0f, 0.20f).OnComplete(() => { motorAudioSource.volume = 0f; }); ;
        }
    }

    public void TurboBoostOn() {
        turboMultiplier = 1.8f;

        // Increase volume
        motorAudioSource.DOKill();
        motorAudioSource.DOFade(0.8f, 0.20f).OnComplete(() => { motorAudioSource.volume = 0.8f; }); ;
    }

    public void TurboBoostOff() {
        StopTurboParticles();
        turboMultiplier = 1.0f;

        // Reduce volume
        motorAudioSource.DOKill();
        if (isMoving) {
            motorAudioSource.DOFade(0.4f, 0.20f).OnComplete(() => { motorAudioSource.volume = 0.4f; }); ;
        } else {
            motorAudioSource.DOFade(0f, 0.20f).OnComplete(() => { motorAudioSource.volume = 0f; }); ;
        }
    }
}
