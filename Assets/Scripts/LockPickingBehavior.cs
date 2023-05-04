using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class LockPickingBehavior : MonoBehaviour {
    public static LockPickingBehavior Instance { get; private set; }

    [Header("Crochet")]
    [SerializeField] Transform pivotCrochet;
    [SerializeField] DoShakeRotationParameters crochetShakeParameters = new DoShakeRotationParameters(0.25f, Vector3.forward * 3f, 16);
    [SerializeField] Transform pivotShaking;
    [SerializeField] DoShakeRotationParameters vibrationShakeParameters = new DoShakeRotationParameters(0.5f, Vector3.forward * 10f, 10, 135f);
    [SerializeField] GameObject vibrationIndicator;
    [SerializeField] GameObject crochetIntact;
    [SerializeField] GameObject crochetBroken;
    [SerializeField] float crochetTurnMouseSpeed = 10f;
    [SerializeField] float crochetTurnKeyboardSpeed = 1.5f;
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
    [Header("Events")]
    [SerializeField] UnityEvent onInstanceRegistered;
    [ReadOnly] public JailBehavior jailBehavior;
    [SerializeField] UnityEvent onLockPickSuccess;
    [SerializeField] UnityEvent onLockPickFail;

    Transform vibrationTransform;
    float currentCrochetRotation = 0f, currentLockRotation = 0f, currentTimeToUnlock = 0f;
    bool hasShaken = false, isPivotKeyPressed = false;

    void Start() {
        if (Instance == null || Instance != this) { Instance = this; onInstanceRegistered?.Invoke(); }
        if (vibrationIndicator != null && vibrationTransform == null) { vibrationTransform = vibrationIndicator.transform; }
    }

    void OnEnable() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        correctCrochetRotation = Random.Range(minMaxRotationCrochet.x, minMaxRotationCrochet.y);
        UpdateCrochetState(true);
        vibrationIndicator.SetActive(false);
    }

    void OnDisable() {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void FixedUpdate() {
        isPivotKeyPressed = Input.GetMouseButton(0) || Input.GetKey(pivotLockKey);
        RotateLock();     
        RotateCrochet();
        CheckLockPick();
    }

    void RotateLock() {
        currentTimeToUnlock = Mathf.Clamp(currentTimeToUnlock + Time.deltaTime * (isPivotKeyPressed ? unlockRelockSpeed.x : unlockRelockSpeed.y), 0, timeToUnlock);
        currentLockRotation = Mathf.LerpAngle(minMaxRotationLock.x, minMaxRotationLock.y, curveToUnlock.Evaluate(currentTimeToUnlock / timeToUnlock));
        pivotLock.rotation = Quaternion.Euler(Vector3.forward * currentLockRotation);
    }

    void RotateCrochet() {
        if (!isPivotKeyPressed) {
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
        if (!hasShaken && goodRotation) {
            hasShaken = true;
            if (vibrationIndicator != null) {
                vibrationIndicator.SetActive(!isPivotKeyPressed);
            }
            if (vibrationTransform != null) {
                vibrationTransform.DOScale(1.25f, .5f).OnComplete(() => vibrationTransform.localScale = Vector3.one);
                vibrationShakeParameters.DoShakeRotation(vibrationTransform);
            }
            if (pivotShaking != null) { crochetShakeParameters.DoShakeRotation(pivotShaking); }
        } else if (!goodRotation && hasShaken) {
            hasShaken = false;
        }     
        if (currentTimeToUnlock >= timeToUnlock) {
            if (goodRotation) {
                onLockPickSuccess?.Invoke();
                if (jailBehavior != null) { 
                    jailBehavior.LockPicked();
                    jailBehavior = null;
                }
            } else {
                onLockPickFail?.Invoke();
                UpdateCrochetState(false);
            }
            vibrationIndicator.SetActive(false);
            currentTimeToUnlock = 0f;
            enabled = false;
        }        
    }

    void UpdateCrochetState(bool isIntact) {
        crochetIntact.SetActive(isIntact);
        crochetBroken.SetActive(!isIntact);
    }
}