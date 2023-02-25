using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimerManager 
{
    internal class TimerManager : MonoBehaviour 
    {
        internal static TimerManager Instance;
        private List<Timer> timerList = new();

        private void Awake() {
            if (Instance == null) {
                Instance = this;
            }
        }

        private void Update() {
            for (int i = 0; i < timerList.Count; i++) {
                timerList[i].UpdateTimer();
            }
        }

        public void AddTimer(Timer _timer) {
            timerList.Add(_timer);
        }

        public void RemoveTimer(Timer _timer) {
            timerList.Remove(_timer);
        }

        public static void KillAll() {
            if (Instance.timerList.Count == 0) { return; }
            while (Instance.timerList.Count > 0) {
                Instance.timerList[0].Cancel();
            }
            Instance.timerList.Clear();
        }
    }

    internal class Timer 
    {
        public float timeElapsed { get; private set; }
        public float timeNeeded { get; private set; }
        public bool isRepeating { get; private set; }
        public bool isPaused { get; private set; }
        public Action callbackAction { get; private set; }
        private static Timer lastExpiredTimer;

        public void UpdateTimer() {
            if (isPaused) { return; }

            timeElapsed += Time.deltaTime;
            if (timeElapsed >= timeNeeded) {
                lastExpiredTimer = this;
                if (isRepeating) {
                    callbackAction.Invoke();
                    timeElapsed = 0;
                } else {
                    callbackAction.Invoke();
                    Cancel();
                }
            }
        }

        public static Timer Register(float duration, bool isRepeating, Action callbackAction) {
            Timer timer = new(duration, isRepeating, callbackAction);
            return timer;
        }
        private Timer(float duration, bool isRepeating, Action callbackAction) {
            if (duration < 0) {
                duration = 0;
            }
            this.timeNeeded = duration;
            this.isRepeating = isRepeating;
            this.isPaused = false;
            this.callbackAction = callbackAction;
            TimerManager.Instance.AddTimer(this);
        }

        public static Timer GetExpiredTimer() {
            return lastExpiredTimer;
        }

        public void Pause() {
            this.isPaused = true;
        }

        public void Resume() {
            this.isPaused = false;
        }

        public void Cancel() {
            TimerManager.Instance.RemoveTimer(this);
            callbackAction = null;
        }

        public void Reset(float newDuration = -1) {
            if (newDuration >= 0) {
                this.timeNeeded = newDuration;
            }
            this.timeElapsed = 0;
        }

        public void SetAction(Action newAction) {
            this.callbackAction = newAction;
        }
    }
}