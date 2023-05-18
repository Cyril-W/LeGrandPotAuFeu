using TMPro;
using UnityEngine;

public class CanvasTooltip : MonoBehaviour {
    public static CanvasTooltip Instance { get; private set; }

    [SerializeField] Vector2 padding = new Vector2(8, 8);
    [SerializeField, ReadOnly] RectTransform canvasRectTransform;
    [SerializeField, ReadOnly] RectTransform rectTransform;
    [SerializeField, ReadOnly] RectTransform backgroundRectTransform;
    [SerializeField, ReadOnly] TextMeshProUGUI textMeshPro;

    System.Func<string> toolTipFunc;

    void Awake() {
        if (Instance == null || Instance != this) { Instance = this;}
    }

    void OnValidate() {
        TryFillNull();
    }

    void Start() {
        TryFillNull();
        //TestTooltip();
    }

    void TryFillNull() {
        if (canvasRectTransform == null) { canvasRectTransform = transform.parent.GetComponent<RectTransform>(); }
        if (rectTransform == null) { rectTransform = GetComponent<RectTransform>(); }
        if (backgroundRectTransform == null && transform.childCount > 0) { backgroundRectTransform = transform.GetChild(0).GetComponent<RectTransform>(); }
        if (textMeshPro == null) {textMeshPro = GetComponentInChildren<TextMeshProUGUI>(); }
    }

    void FixedUpdate() {
        var anchoredPosition = Input.mousePosition / canvasRectTransform.localScale.x;
        if (anchoredPosition.x + backgroundRectTransform.rect.width > canvasRectTransform.rect.width) {
            anchoredPosition.x = canvasRectTransform.rect.width - backgroundRectTransform.rect.width;
        }
        if (anchoredPosition.y + backgroundRectTransform.rect.height > canvasRectTransform.rect.height) {
            anchoredPosition.y = canvasRectTransform.rect.height - backgroundRectTransform.rect.height;
        }
        rectTransform.anchoredPosition = anchoredPosition;
        if (toolTipFunc != null) { Debug.Log("func not null"); SetText(toolTipFunc()); }
    }

    public void ShowTooltip(System.Func<string> newToolTipFunc) {
        toolTipFunc = newToolTipFunc;
        SetText(toolTipFunc());
        gameObject.SetActive(true);
    }

    public void ShowTooltip(string toolTipText) {
        toolTipFunc = null;
        SetText(toolTipText);
        gameObject.SetActive(true);
    }

    public void HideTooltip() {
        toolTipFunc = null;
        gameObject.SetActive(false);
    }

    void SetText(string tooltipText) {
        textMeshPro.SetText(tooltipText);
        textMeshPro.ForceMeshUpdate();
        var textSize = textMeshPro.GetRenderedValues(false);
        backgroundRectTransform.sizeDelta = textSize + padding;
    }

    [ContextMenu("Test Tooltip")]
    void TestTooltip() {
        var test = "This is a test...";
        var abc = "azertyuiopqsdfghjklmwxcvbnAZERTYUIOPQSDFGHJKLMWXCVBN\n\n\n\n\n\n,;:!ù^$";
        for (int i = 0; i < Random.Range(5, 100); i++) {
            test += abc[Random.Range(0, abc.Length)];
        }
        SetText(test);
    }
}
