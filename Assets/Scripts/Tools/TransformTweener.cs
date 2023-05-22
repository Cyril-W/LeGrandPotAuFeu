using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class TransformTweener : MonoBehaviour {
    [SerializeField] bool tweenOnStart = false;
    [SerializeField] Transform tweenTransform;
    [SerializeField] Transform startTransform;
    [SerializeField] Transform endTransform;
    [SerializeField] bool tweenPosition = true;
    [SerializeField] Vector2 startEndDurationPosition = new Vector2(0.1f, 1f);
    [SerializeField] UnityEvent onPositionFinished;
    [SerializeField] bool tweenRotation = false;
    [SerializeField] Vector2 startEndDurationRotation = new Vector2(0.1f, 1f);
    [SerializeField] UnityEvent onRotationFinished;
    [SerializeField] bool tweenScale = false;
    [SerializeField] Vector2 startEndDurationScale = new Vector2(0.1f, 1f);
    [SerializeField] UnityEvent onScaleFinished;

    private void Start() {
        if (tweenOnStart) { Tween(); }
    }
    public void Tween() {
        if (tweenTransform == null || startTransform == null || endTransform == null) { return; }
        if (tweenPosition) {
            tweenTransform.DOMove(startTransform.position, startEndDurationPosition.x).OnComplete(() => TweenPosition());
        }
        if (tweenRotation) {
            tweenTransform.DORotateQuaternion(startTransform.rotation, startEndDurationRotation.x).OnComplete(() => TweenRotation());
        }
        if (tweenScale) {
            tweenTransform.DOScale(startTransform.localScale, startEndDurationScale.x).OnComplete(() => TweenScale());
        }
    }

    void TweenPosition() {
        tweenTransform.DOMove(endTransform.position, startEndDurationPosition.y).OnComplete(() => onPositionFinished?.Invoke());
    }
    void TweenRotation() {
        tweenTransform.DORotateQuaternion(endTransform.rotation, startEndDurationRotation.y).OnComplete(() => onRotationFinished?.Invoke());
    }
    void TweenScale() {
        tweenTransform.DOScale(endTransform.localScale, startEndDurationScale.y).OnComplete(() => onScaleFinished?.Invoke());
    }
}
