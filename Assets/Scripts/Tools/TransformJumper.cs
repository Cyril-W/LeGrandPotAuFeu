using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class TransformJumper : MonoBehaviour {
    [SerializeField] bool jumpOnStart = false;
    [SerializeField] bool rotateOnJump = false;
    [SerializeField] Transform tweenTransform;
    [SerializeField] Transform startTransform;
    [SerializeField] Transform endTransform;
    [SerializeField] float jumpPower = 1f;
    [SerializeField, Range(1, 100)] int numberJumps = 1;
    [SerializeField] Vector2 startEndDuration = new Vector2(0.1f, 1f);
    [SerializeField] UnityEvent onJumpFinished;

    void Start() {
        if (jumpOnStart) { Jump(); }
    }

    [ContextMenu("Reset")]
    public void Reset() {
        if (tweenTransform == null || startTransform == null) { return; }
        tweenTransform.position = startTransform.position;
    }

    [ContextMenu("Jump")]
    public void Jump() {
        if (tweenTransform == null || startTransform == null || endTransform == null) { return; }
        if (rotateOnJump) { tweenTransform.DORotate(GetRotation(), startEndDuration.x); }
        tweenTransform.DOMove(startTransform.position, startEndDuration.x).OnComplete(() => JumpNumberTime());
    }

    void JumpNumberTime() {
        tweenTransform.DOJump(endTransform.position, jumpPower, numberJumps, startEndDuration.y).OnComplete(() => onJumpFinished?.Invoke());
    }

    Vector3 GetRotation() {
        var direction = (endTransform.position - startTransform.position).normalized;
        var angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        return Vector3.up * angle;
    }
}
