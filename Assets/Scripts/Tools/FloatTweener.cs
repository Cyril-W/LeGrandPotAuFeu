using UnityEngine;
using UnityEngine.Events;

public class FloatTweener : MonoBehaviour {
    [SerializeField] bool tweenOnStart = false;
    [SerializeField] bool sendEventOnStop = false;
    [SerializeField] Vector2 startEndFloat;
    [SerializeField] float duration = 1f;
    [SerializeField] AnimationCurve floatCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] UnityEvent<float> onCurrentFloat;
    [SerializeField] UnityEvent onFloatEnd;
    [SerializeField, ReadOnly] float currentFloat = 0f;

    float currentDuration = 0f;
    bool isReverse = false;

    void Start() {
        if (tweenOnStart) { StartTweenFloat(); }
    }

    void FixedUpdate() {
        if (currentDuration > 0f) {
            currentDuration -= Time.deltaTime;
            if (currentDuration < 0f) {
                enabled = false;
                currentFloat = startEndFloat.y;
                onFloatEnd?.Invoke();
                return;
            }
            var ratio = Mathf.Clamp01(currentDuration / duration);
            currentFloat = Mathf.Lerp(startEndFloat.x, startEndFloat.y, floatCurve.Evaluate(!isReverse ? 1f - ratio : ratio));
            onCurrentFloat?.Invoke(currentFloat);
        }
    }

    public void StartTweenFloat() {
        currentFloat = startEndFloat.x;
        currentDuration = duration;
        isReverse = false;
        enabled = true;
    }

    public void StartReverseTweenFloat() {
        currentFloat = startEndFloat.y;
        currentDuration = duration;
        isReverse = true;
        enabled = true;
    }

    public void PauseTweenFloat() {
        enabled = false;
    }

    public void ResumeTweenFloat() {
        enabled = true;
    }

    public void StopTweenFloat() {
        currentDuration = 0f;
        currentFloat = startEndFloat.x;
        if (sendEventOnStop) { onCurrentFloat?.Invoke(currentFloat); }
    }
}
