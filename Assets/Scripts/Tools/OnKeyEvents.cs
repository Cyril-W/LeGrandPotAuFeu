using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class OnKeyEvents : MonoBehaviour {
    [Header("UI")]
    [SerializeField] GameObject keyCodeUI;
    [SerializeField] TextMeshPro keyCodetext;
    [Header("Events")]
    [SerializeField] KeyCode keyCode;
    [SerializeField] UnityEvent OnKeyDown;
    [SerializeField] UnityEvent OnKeyUp;
    [SerializeField] UnityEvent OnKeyPressed;

    bool isKeyPressed = false;

    void OnValidate() {
        if (keyCodetext) keyCodetext.text = keyCode.ToString();
    }

    void OnEnable() {
        if (keyCodetext) keyCodetext.text = keyCode.ToString();
        if (keyCodeUI) keyCodeUI.SetActive(true);
    }

    void OnDisable() {
        if (keyCodeUI) keyCodeUI.SetActive(false);
    }

    void FixedUpdate() {
        var getKey = Input.GetKey(keyCode);
        if (getKey) {
            OnKeyPressed?.Invoke();
        }
        if (getKey != isKeyPressed) {
            isKeyPressed = !isKeyPressed;
            if (isKeyPressed) {
                OnKeyDown?.Invoke();
            } else {
                OnKeyUp?.Invoke();
            }
        }
    }
}
