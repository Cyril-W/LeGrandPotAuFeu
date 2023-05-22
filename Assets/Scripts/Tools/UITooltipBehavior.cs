using UnityEngine;
using UnityEngine.EventSystems;

public class UITooltipBehavior : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] string shortTooltipText;
    [SerializeField, TextArea] string longTooltipText;

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
        if (CanvasTooltip.Instance != null) { CanvasTooltip.Instance.ShowTooltip(shortTooltipText, longTooltipText); }
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
        if (CanvasTooltip.Instance != null) { CanvasTooltip.Instance.HideTooltip(); }
    }
}
