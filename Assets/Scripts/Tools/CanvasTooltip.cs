using TMPro;
using UnityEngine;

public class CanvasTooltip : MonoBehaviour {
    public static CanvasTooltip Instance { get; private set; }

    [SerializeField] Vector2 padding = new Vector2(8, 8);
    [SerializeField] float timeBeforeLongTooltip = 2f;
    [SerializeField, ReadOnly] RectTransform canvasRectTransform;
    [SerializeField, ReadOnly] RectTransform rectTransform;
    [SerializeField, ReadOnly] RectTransform backgroundRectTransform;
    [SerializeField, ReadOnly] TextMeshProUGUI textMeshPro;

    System.Func<string> toolTipFunc;
    string shortTooltip, longTooltip;
    float currentTime = 0;

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
        if (anchoredPosition.x - backgroundRectTransform.rect.width < 0) {
            anchoredPosition.x = backgroundRectTransform.rect.width;
        }
        if (anchoredPosition.y + backgroundRectTransform.rect.height > canvasRectTransform.rect.height) {
            anchoredPosition.y = canvasRectTransform.rect.height - backgroundRectTransform.rect.height;
        }
        rectTransform.anchoredPosition = anchoredPosition;
        if (currentTime > 0f) {
            currentTime -= Time.deltaTime;
            if (currentTime < 0f &&toolTipFunc == null) { 
                SetText(longTooltip);
            }
        } else if (toolTipFunc != null) { 
            SetText(toolTipFunc(), true);
        }
    }

    public void ShowTooltip(string newShortTooltop, System.Func<string> newToolTipFunc) {
        toolTipFunc = newToolTipFunc;
        SetTooltip(newShortTooltop);
    }

    public void ShowTooltip(string newShortTooltip, string newLongTooltip) {        
        toolTipFunc = null;
        SetTooltip(newShortTooltip, newLongTooltip);
    }

    void SetTooltip(string newShortTooltip, string newLongTooltip = "") {
        gameObject.SetActive(true);
        currentTime = timeBeforeLongTooltip;
        shortTooltip = ReplaceChars(newShortTooltip);
        longTooltip = ReplaceChars(newLongTooltip);
        SetText(shortTooltip);
    }

    public void HideTooltip() {
        gameObject.SetActive(false);
        toolTipFunc = null;
        currentTime = 0f;
    }

    void SetText(string tooltipText, bool needReplace = false) {
        if (needReplace) {
            tooltipText = ReplaceChars(tooltipText);
        }
        textMeshPro.SetText(tooltipText);
        textMeshPro.ForceMeshUpdate();
        var textSize = textMeshPro.GetRenderedValues();
        backgroundRectTransform.sizeDelta = textSize + padding;
        textMeshPro.rectTransform.sizeDelta = textSize + padding;
    }

    string ReplaceChars(string s) {
        s = s.Replace('é', 'e');
        s = s.Replace('è', 'e');
        s = s.Replace('ê', 'e');
        s = s.Replace('à', 'a');
        s = s.Replace('ç', 'c');
        return s;
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
