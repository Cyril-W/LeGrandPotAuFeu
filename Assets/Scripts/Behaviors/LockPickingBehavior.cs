using UnityEngine;
using UnityEngine.Events;

public class LockPickingBehavior : MonoBehaviour {
    public static LockPickingBehavior Instance { get; private set; }

    [Header("Crochet")]
    [SerializeField] Transform pivotCrochet;
    [SerializeField] DoShakeRotationParameters crochetShakeParameters = new DoShakeRotationParameters(0.25f, Vector3.forward * 3f, 16);
    [SerializeField] Transform pivotShaking;
    [SerializeField] DoShakeRotationParameters crochetWrongShakeParameters = new DoShakeRotationParameters(0.5f, Vector3.forward * 5f, 16);
    [SerializeField] AnimationCurve curveWrongShakeStrength;
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
    [SerializeField] UnityEvent<bool> onPivotPressedChanged;
    [SerializeField] UnityEvent onCrochetShake;
    [SerializeField] UnityEvent onLockPickSuccess;
    [SerializeField] UnityEvent onLockPickFail;

    Vector3 initialWrongShakeStrength;
    float currentCrochetRotation = 0f, currentLockRotation = 0f, currentTimeToUnlock = 0f;
    bool hasShaken = false, isPivotKeyPressed = false, isShakingWrong = false, lastPivotPressed = false;

    void Awake() {
        if (Instance == null || Instance != this) { Instance = this; onInstanceRegistered?.Invoke(); }
    }

    void Start() {
        if (crochetWrongShakeParameters != null) { 
            initialWrongShakeStrength = crochetWrongShakeParameters.Strength;
            crochetWrongShakeParameters.OnComplete += SetFalseIsShakingWrong;
        }
    }

    void OnDestroy() {
        if (crochetWrongShakeParameters != null) {
            crochetWrongShakeParameters.OnComplete -= SetFalseIsShakingWrong;
        }
    }

    void OnEnable() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        correctCrochetRotation = Random.Range(minMaxRotationCrochet.x, minMaxRotationCrochet.y);
        UpdateCrochetState(true);
    }

    void OnDisable() {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;        
    }

    public void LockPickAbort() {
        if (jailBehavior != null) { jailBehavior.LockPickAbort(); jailBehavior = null; }
    }

    void FixedUpdate() {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Quote)) {
            Debug.LogWarning("[²] - Lock picking cheat code");
            onLockPickSuccess?.Invoke();
            if (jailBehavior != null) {
                jailBehavior.LockPicked();
                jailBehavior = null;
            }
        }
#endif
        isPivotKeyPressed = Input.GetMouseButton(0) || Input.GetKey(pivotLockKey);
        if (lastPivotPressed != isPivotKeyPressed) {
            lastPivotPressed = isPivotKeyPressed;
            onPivotPressedChanged?.Invoke(lastPivotPressed);
        }
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
            if (pivotShaking != null) { 
                crochetShakeParameters.DoShakeRotation(pivotShaking);
                onCrochetShake?.Invoke();

            }
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
            currentTimeToUnlock = 0f;
            enabled = false;
        } else if (isPivotKeyPressed && !goodRotation && !isShakingWrong) {
            isShakingWrong = true;
            crochetWrongShakeParameters.Strength = Vector3.Lerp(Vector3.zero, initialWrongShakeStrength, curveWrongShakeStrength.Evaluate(Mathf.Clamp01(currentTimeToUnlock / timeToUnlock)));
            crochetWrongShakeParameters.DoShakeRotation(pivotShaking);            
        }       
    }

    void SetFalseIsShakingWrong() {
        isShakingWrong = false;
    }

    void UpdateCrochetState(bool isIntact) {
        crochetIntact.SetActive(isIntact);
        crochetBroken.SetActive(!isIntact);
    }
}
