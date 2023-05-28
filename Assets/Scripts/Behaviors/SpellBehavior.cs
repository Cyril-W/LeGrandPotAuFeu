using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SpellBehavior : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] Hero hero;
    [SerializeField] string heroName;
    [SerializeField] string spellName;
    [SerializeField, TextArea] string spellDescription;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] Vector2 minMaxAlphaCanvasGroup = new Vector2(0.25f, 1f);
    [SerializeField] GameObject chainsGameObject;
    [SerializeField] Button spellButton;
    [SerializeField] Image spellCooldown;
    [SerializeField] float cooldown;
    [SerializeField] Transform transformToPunch;
    [SerializeField] float afterDelayPunch = 0.5f;
    [SerializeField] DoPunchScaleParameters punchScaleParameters = new DoPunchScaleParameters(Vector3.one * 1.1f, 0.25f);

    float currentCooldown = 0f, currentPunchDelay = 0f;
    bool currentIsSaved = false;

    public Hero GetHero() {
        return hero;
    }

    void Start() {
        spellCooldown.fillAmount = Mathf.Clamp01(currentCooldown / cooldown);
    }

    void OnEnable() {
        if (currentIsSaved) { currentPunchDelay = afterDelayPunch; }
    }

    public void SetSaved(bool isSaved) {
        if (chainsGameObject == null || canvasGroup == null) { return; }
        chainsGameObject.SetActive(!isSaved);
        canvasGroup.alpha = !isSaved ? minMaxAlphaCanvasGroup.x : minMaxAlphaCanvasGroup.y;
        canvasGroup.interactable = isSaved;
        currentIsSaved = isSaved;
    }

    public void OnSpellClick() {
        if (!currentIsSaved) { return; }
        currentCooldown = cooldown;
        spellButton.interactable = false;
        if (GroupManager.Instance != null) { GroupManager.Instance.OnSpellClick?.Invoke(hero); }
    }

    public void SetCurrentCooldown(float newCooldown) {
        currentCooldown = newCooldown;
    }

    void FixedUpdate() {
        if (currentPunchDelay > 0f) {
            currentPunchDelay -= Time.deltaTime;
            if (currentPunchDelay <= 0f && transformToPunch != null) { punchScaleParameters.DoPunchScale(transformToPunch); }
        }
        if (currentCooldown <= 0f) { return; }
        currentCooldown -= Time.deltaTime;
        spellCooldown.fillAmount = Mathf.Clamp01(currentCooldown / cooldown);
        if (currentCooldown <= 0f) spellButton.interactable = true;
    }

    string GetToolTip() {
        var cooldownValue = cooldown - currentCooldown;
        var s = "<u>" + heroName + "</u> : " + spellName + " <i>(" + cooldownValue.ToString("0") + " / " + cooldown.ToString("0") + ")</i>\n" + spellDescription;
        return s;
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
        if (CanvasTooltip.Instance != null) { CanvasTooltip.Instance.ShowTooltip("<u>" + heroName + "</u>: " + spellName, GetToolTip); }
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
        if (CanvasTooltip.Instance != null) { CanvasTooltip.Instance.HideTooltip(); }
    }
}
