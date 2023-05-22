using UnityEngine;
using UnityEngine.Events;

public class AfterDelayEvent : MonoBehaviour {
    [SerializeField] bool delayOnStart = false;
    [SerializeField] float delay = 1f;
    [SerializeField] UnityEvent onDelayFinished;

    [SerializeField, ReadOnly] float currentDelay = 0f;

    void Start() {
        if (delayOnStart) { StartDelay(); }
    }

    void FixedUpdate() {
        if (currentDelay > 0f) {
            currentDelay -= Time.deltaTime;
            if (currentDelay <= 0f) {
                onDelayFinished?.Invoke();
                enabled = false;
            }
        }
    }

    public void StartDelay() {
        currentDelay = delay;
        enabled = true;
    }

    public void PauseDelay() {
        enabled = false;
    }

    public void ResumeDelay() {
        enabled = true;
    }

    public void StopDelay() {
        currentDelay = 0f;
        enabled = false;
    }
}
