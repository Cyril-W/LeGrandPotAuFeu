using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldToUIManager : MonoBehaviour {
    public static WorldToUIManager Instance { get; private set; }

    [SerializeField] Transform transformUI;
    [SerializeField] List<Transform> transformsToUI = new List<Transform>();
    [SerializeField] GameObject gameObjectUI;
    [SerializeField] Image imageUI;
    [SerializeField] TextMeshProUGUI textUI;

    Transform currentTransform;

    void OnValidate() {
        TryFillNull();
    }

    void Awake() {
        if (Instance == null || Instance != this) { Instance = this; }
    }

    void Start() {
        TryFillNull();
        SetImageFill(0);
        SetText("");
        PickNextTransformToUI();
    }

    void TryFillNull() {
        if (transformUI == null) { transformUI = transform; }
        if (imageUI == null) { imageUI = GetComponentInChildren<Image>(); }
        if (gameObjectUI == null && imageUI != null) { gameObjectUI = imageUI.gameObject; }
        if (textUI == null) { textUI = GetComponentInChildren<TextMeshProUGUI>(); }
    }

    void FixedUpdate() {
        if (currentTransform == null || Camera.main == null) { return; }
        transformUI.position = Camera.main.WorldToScreenPoint(currentTransform.position);
    }

    public void SetImageFill(float newFill) {
        if (imageUI == null) { return; }
        imageUI.fillAmount = Mathf.Clamp01(newFill);
    }

    public void SetText(string newText) {
        if (textUI == null) { return; }
        textUI.text = newText;
    }

    public void RegisterToUI(Transform transformToStart) {
        if (!transformsToUI.Contains(transformToStart)) { transformsToUI.Add(transformToStart); }
        PickNextTransformToUI();
    }

    public void UnregisterToUI(Transform transformToStop) {
        if (transformsToUI.Contains(transformToStop)) { transformsToUI.Remove(transformToStop); }
        if (currentTransform == transformToStop) { PickNextTransformToUI(); }
    }

    void PickNextTransformToUI() {
        if (transformsToUI.Count > 0) { 
            currentTransform = transformsToUI[transformsToUI.Count - 1];
            transformUI.position = Camera.main.WorldToScreenPoint(currentTransform.position);
            gameObjectUI.SetActive(true);
        } else { 
            currentTransform = null;
            gameObjectUI.SetActive(false);
        }
    }
}
