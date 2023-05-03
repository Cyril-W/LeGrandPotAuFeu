using UnityEngine;
using UnityEngine.Events;

public class LockPickingBehavior : MonoBehaviour {
    [Header("Crochet")]
    [SerializeField] Transform pivotCrochet;
    [SerializeField] GameObject vibrationIndicator;
    [SerializeField] GameObject crochetIntact;
    [SerializeField] GameObject crochetBroken;
    [SerializeField] float crochetTurnMouseSpeed = 10f;
    [SerializeField] float crochetTurnKeyboardSpeed = 2f;
    [SerializeField] Vector2 minMaxRotationCrochet = new Vector2(-180f, 0f);
    [SerializeField] float crochetRotationMargin = 5f;
    [SerializeField, ReadOnly] float correctCrochetRotation;
    [Header("Lock")]
    [SerializeField] KeyCode pivotLockKey = KeyCode.E;
    [SerializeField] Transform pivotLock;
    [SerializeField] float timeToUnlock = 3f;
    [SerializeField] Vector2 unlockRelockSpeed = new Vector2(1f, -2f);
    [SerializeField] AnimationCurve curveToUnlock;
    [SerializeField] Vector2 minMaxRotationLock = new Vector2(0f, -90f);
    [SerializeField] UnityEvent onLockPickSuccess;
    [SerializeField] UnityEvent onLockPickFail;

    float currentCrochetRotation = 0f, currentLockRotation = 0f, currentTimeToUnlock = 0f;

    void OnEnable() {
        Cursor.visible = false;
        correctCrochetRotation = Random.Range(minMaxRotationCrochet.x, minMaxRotationCrochet.y);
        UpdateCrochetState(true);
        vibrationIndicator.SetActive(false);
    }

    void OnDisable() {
        Cursor.visible = true;
    }

    void FixedUpdate() {
        RotateLock();     
        RotateCrochet();
        CheckLockPick();
    }

    void RotateLock() {
        currentTimeToUnlock = Mathf.Clamp(currentTimeToUnlock + Time.deltaTime * (Input.GetKey(pivotLockKey) ? unlockRelockSpeed.x : unlockRelockSpeed.y), 0, timeToUnlock);
        currentLockRotation = Mathf.LerpAngle(minMaxRotationLock.x, minMaxRotationLock.y, curveToUnlock.Evaluate(currentTimeToUnlock / timeToUnlock));
        pivotLock.rotation = Quaternion.Euler(Vector3.forward * currentLockRotation);
    }

    void RotateCrochet() {
        if (!Input.GetKey(pivotLockKey)) {
            if (Mathf.Abs(Input.GetAxis("Mouse X")) > 0f) {
                currentCrochetRotation += Input.GetAxis("Mouse X") * crochetTurnMouseSpeed * -1f;
            } else {
                currentCrochetRotation += Input.GetAxis("Horizontal") * crochetTurnKeyboardSpeed * -1f;
            }
            currentCrochetRotation = Mathf.Clamp(currentCrochetRotation, minMaxRotationCrochet.x, minMaxRotationCrochet.y);
        }
        pivotCrochet.rotation = Quaternion.Euler(Vector3.forward * currentCrochetRotation);
    }

    void CheckLockPick() {
        var goodRotation = Mathf.Abs(currentCrochetRotation - correctCrochetRotation) <= crochetRotationMargin;
        vibrationIndicator.SetActive(!Input.GetKey(pivotLockKey) && goodRotation);
        if (currentTimeToUnlock >= timeToUnlock) {
            if (goodRotation) {
                onLockPickSuccess?.Invoke();                
            } else {
                onLockPickFail?.Invoke();
                UpdateCrochetState(false);
            }
            vibrationIndicator.SetActive(false);
            enabled = false;
        }        
    }

    void UpdateCrochetState(bool isIntact) {
        crochetIntact.SetActive(isIntact);
        crochetBroken.SetActive(!isIntact);
    }
}
