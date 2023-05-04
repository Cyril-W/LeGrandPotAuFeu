using UnityEngine;
using UnityEngine.Events;

public class AfterDelayEvent : MonoBehaviour {
    [SerializeField] bool startOnEnable = false;
    [SerializeField] float delay = 1f;
    [SerializeField] bool resetOnDisable = false;
    [SerializeField] UnityEvent onDelayFinished;

    [SerializeField, ReadOnly] float currentDelay = 0f;

    void OnEnable() {
        if (startOnEnable) {
            StartDelay();
        }
    }

    void OnDisable() {
        if (resetOnDisable) {
            StopDelay();
        }
    }

    void FixedUpdate() {
        if (currentDelay > 0f) {
            currentDelay -= Time.deltaTime;
            if (currentDelay <= 0f) {
                onDelayFinished?.Invoke();
            }
        }
    }

    public void StartDelay() {
        currentDelay = delay;
    }

    public void PauseDelay() {
        enabled = false;
    }

    public void StopDelay() {
        currentDelay = 0f;
    }
}
