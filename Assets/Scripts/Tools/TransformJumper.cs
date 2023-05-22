using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class TransformJumper : MonoBehaviour {
    [SerializeField] bool jumpOnStart = false;
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
    public void Jump() {
        if (tweenTransform == null || startTransform == null || endTransform == null) { return; }
        tweenTransform.DOMove(startTransform.position, startEndDuration.x).OnComplete(() => JumpNumberTime());
    }

    void JumpNumberTime() {
        tweenTransform.DOJump(endTransform.position, jumpPower, numberJumps, startEndDuration.y).OnComplete(() => onJumpFinished?.Invoke());
    }
}
