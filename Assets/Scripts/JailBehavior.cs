using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class JailBehavior : MonoBehaviour {
    [SerializeField] bool isLocked;
    [SerializeField] float unlockTime = 1f;
    [SerializeField] GameObject unlockPanel;
    [SerializeField] Transform unlockProgressBarPivot;
    [SerializeField] GameObject lockInteractor;
    [SerializeField] GameObject openInteractor;
    [SerializeField] GameObject closeInteractor;
    [SerializeField] Transform jailPivot;
    [SerializeField] float rotationDuration = 1f;
    [SerializeField] Vector2 minMaxClosedAngle = new Vector2(10f, 15f);
    [SerializeField] Vector2 minMaxOpenAngle = new Vector2(70f, 120f);
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
    float rotationY, currentUnlockTime = 0f, randomClosedRotation, randomOpenRotation, currentTimeSinceAction = 0f;
    Vector3 progressLocalScale;

    void Awake() {
        isOpened = false;
        rotationY = jailPivot.rotation.eulerAngles.y;    
        unlockPanel.SetActive(false);
        UpdateInteractors();
    }

    void OnEnable() {
        if (!isLocked) {
            UpdateRandomRotation(true);
            jailPivot.DORotate(Vector3.up * (rotationY - randomClosedRotation), 0f);
        } else {
            jailPivot.DORotate(Vector3.up * rotationY, 0f);
        }
    }

    void FixedUpdate() {
        progressLocalScale = unlockProgressBarPivot.localScale;
        progressLocalScale.x = 1f - Mathf.Clamp01(currentUnlockTime / unlockTime);
        unlockProgressBarPivot.localScale = progressLocalScale;
        if (currentUnlockTime > 0f) {
            currentUnlockTime -= Time.deltaTime;
            if (currentUnlockTime <= 0f) {
                lockInteractor.SetActive(false);
                LockPickingBehavior.Instance.jailBehavior = this;
                onLockPickStart?.Invoke();                
            }
        }
        if (currentTimeSinceAction > 0f) {
            currentTimeSinceAction -= Time.deltaTime;
        }
    }

    public void LockPickAbort() {
        lockInteractor.SetActive(true);
        Unlock(false);
    }

    public void LockPicked() {
        isLocked = false;
        unlockPanel.SetActive(false);
        OpenJail();
    }

    public bool GetIsLocked() {
        return isLocked;
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
        UpdateRandomRotation(false);
        jailPivot.DOKill();
        jailPivot.DORotate(Vector3.up * (rotationY - randomOpenRotation), rotationDuration);
    }

    public void CloseJail() {
        if(!isOpened || currentTimeSinceAction > 0f) return;
        currentTimeSinceAction = timeBetweenAction;
        isOpened = false;
        onJailIsOpen?.Invoke(isOpened);
        UpdateInteractors();
        UpdateRandomRotation(true);
        jailPivot.DOKill();
        jailPivot.DORotate(Vector3.up * (rotationY - randomClosedRotation), rotationDuration);
    }

    void UpdateInteractors() {
        lockInteractor.SetActive(isLocked);
        openInteractor.SetActive(!isLocked && !isOpened);
        closeInteractor.SetActive(!isLocked && isOpened);
    }

    void UpdateRandomRotation(bool isClosed) {
        if (isClosed) {
            randomClosedRotation = Random.Range(minMaxClosedAngle.x, minMaxClosedAngle.y);
        } else {
            randomOpenRotation = Random.Range(minMaxOpenAngle.x, minMaxOpenAngle.y);
        }
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
