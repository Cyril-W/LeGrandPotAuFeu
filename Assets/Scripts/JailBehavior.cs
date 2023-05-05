using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class JailBehavior : MonoBehaviour {
    public bool needToLockPick = true;
    [SerializeField] bool isLocked;
    [SerializeField] float unlockTime = 1f;
    [SerializeField] GameObject unlockPanel;
    [SerializeField] Transform unlockProgressBarPivot;
    [SerializeField] GameObject lockInteractor;
    [SerializeField] GameObject openInteractor;
    [SerializeField] GameObject closeInteractor;
    [SerializeField] Transform jailPivot;
    [SerializeField] float rotationDuration = 1f;
    [SerializeField] Vector2 minMaxOpenAngle = new Vector2(80f, 110f);
    [SerializeField] float timeBetweenAction = 0.5f;
    [Header("Gizmos")]
    [SerializeField] float lineLength = 1f;
    [SerializeField] float ArrowLength = 0.5f;
    [SerializeField] float ArrowSize = 0.5f;
    [SerializeField] int arrowNumber = 4;
    [SerializeField] Color arrowColor = Color.red;
    [Header("Events")]
    [SerializeField] UnityEvent onLockPickStart;
    [SerializeField] UnityEvent<bool> onJailIsOpen;

    bool isOpened = false;
    float rotationY, currentUnlockTime = 0f, chosenOpenRotation, currentTimeSinceAction = 0f;
    Vector3 progressLocalScale;

    void Start() {
        isOpened = false;
        rotationY = jailPivot.rotation.eulerAngles.y;
        unlockPanel.SetActive(false);
        UpdateInteractors();
    }

    void FixedUpdate() {
        progressLocalScale = unlockProgressBarPivot.localScale;
        progressLocalScale.x = 1f - Mathf.Clamp01(currentUnlockTime / unlockTime);
        unlockProgressBarPivot.localScale = progressLocalScale;
        if (currentUnlockTime > 0f) {
            currentUnlockTime -= Time.deltaTime;
            if (currentUnlockTime <= 0f) {
                if (needToLockPick) {
                    LockPickingBehavior.Instance.jailBehavior = this;
                    onLockPickStart?.Invoke();
                } else {
                    LockPicked();
                }
            }
        }
        if (currentTimeSinceAction > 0f) {
            currentTimeSinceAction -= Time.deltaTime;
        }
    }

    public void LockPicked() {
        isLocked = false;
        unlockPanel.SetActive(false);
        OpenJail();
    }

    public void Unlock(bool isUnlocking) {
        if (DestinyManager.Instance.AnyTrackingGuard()) { return; }
        unlockPanel.SetActive(isUnlocking);
        currentUnlockTime = isUnlocking ? unlockTime : 0f;
    }

    public void OpenJail() {
        if (isOpened || currentTimeSinceAction > 0f) return;
        currentTimeSinceAction = timeBetweenAction;
        isOpened = true;
        onJailIsOpen?.Invoke(isOpened);
        UpdateInteractors();
        chosenOpenRotation = Random.Range(minMaxOpenAngle.x, minMaxOpenAngle.y);
        jailPivot.DOKill();
        jailPivot.DORotate(Vector3.up * (rotationY - chosenOpenRotation), rotationDuration);
    }

    public void CloseJail() {
        if(!isOpened || currentTimeSinceAction > 0f) return;
        currentTimeSinceAction = timeBetweenAction;
        isOpened = false;
        onJailIsOpen?.Invoke(isOpened);
        UpdateInteractors();
        jailPivot.DOKill();
        jailPivot.DORotate(Vector3.up * rotationY, rotationDuration);
    }

    void UpdateInteractors() {
        lockInteractor.SetActive(isLocked);
        openInteractor.SetActive(!isLocked && !isOpened);
        closeInteractor.SetActive(!isLocked && isOpened);
    }

    void OnDrawGizmos() {
        Gizmos.color = arrowColor;
        Gizmos.matrix = transform.localToWorldMatrix;
        var start = new Vector3(0, 1f, 1f);
        var endPos = start + Vector3.forward * lineLength;
        Gizmos.DrawLine(start, endPos);
        var size = new Vector3(1f, 1f, 0f) * ArrowSize;
        for (int i = 0; i < arrowNumber; i++) {
            var ratio = Mathf.Clamp01(i / (float)arrowNumber);
            Gizmos.DrawWireCube(endPos + Vector3.forward * ArrowLength * ratio, size * (1f - ratio));
        }
    }
}
