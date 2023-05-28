//using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class OnKeyEvents : MonoBehaviour {
    //[Header("UI")]
    //[SerializeField] GameObject keyCodeUI;
    //[SerializeField] TextMeshPro keyCodeText;
    [SerializeField] bool checkForPause = true;
    [SerializeField] int keyCodeSubstring = -1;
    [SerializeField] bool onChangeUpdateTextUI = false;
    //[SerializeField] TextMeshProUGUI keyCodeTextUGUI;
    //[Header("Events")]
    [SerializeField] KeyCode keyCode;
    [SerializeField] UnityEvent OnKeyDown;
    [SerializeField] UnityEvent OnKeyPressed;
    [SerializeField] UnityEvent OnKeyUp;

    bool isKeyPressed = false;

    void OnValidate() {
        UpdateKeyCodeTexts();
    }

    void OnEnable() {
        UpdateKeyCodeTexts();
        //if (keyCodeUI) { keyCodeUI.SetActive(true); }
        isKeyPressed = Input.GetKey(keyCode);
    }

    void OnDisable() {
        //if (keyCodeUI) { keyCodeUI.SetActive(false); }
    }

    void FixedUpdate() {
        if (checkForPause && (LevelManager.Instance == null || LevelManager.Instance.IsPaused)) { return; }
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

    void UpdateKeyCodeTexts() {
        if (!onChangeUpdateTextUI) { return; }
        var keyCodeName = keyCode.ToString();
        if (keyCodeSubstring > 0 && keyCodeName.Length >= keyCodeSubstring) { keyCodeName = keyCodeName.Substring(0, keyCodeSubstring); }
        if (WorldToUIManager.Instance != null) { WorldToUIManager.Instance.SetText(keyCodeName); }
        /*if (keyCodeText) { keyCodeText.text = keyCodeName; }
        if (keyCodeTextUGUI) { keyCodeTextUGUI.text = keyCodeName; }*/
    }

}
