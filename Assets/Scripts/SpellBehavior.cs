using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SpellBehavior : MonoBehaviour {
    [SerializeField] Hero hero;
    [SerializeField] Button spellButton;
    [SerializeField] Image spellCooldown;
    [SerializeField] float cooldown;
    [SerializeField] DoPunchScaleParameters punchScaleParameters = new DoPunchScaleParameters(Vector3.one * 1.1f, 0.25f);
    float currentCooldown = 0f;

    public Hero GetHero() {
        return hero;
    }

    void Start() {
        spellCooldown.fillAmount = Mathf.Clamp01(currentCooldown / cooldown);
    }

    void OnEnable() {
        punchScaleParameters.DoPunchScale(transform);
    }

    public void OnSpellClick() {
        currentCooldown = cooldown;
        spellButton.interactable = false;
        if (GroupManager.Instance != null) {
            GroupManager.Instance.OnSpellClick?.Invoke(hero);
        }
    }

    public void SetCurrentCooldown(float newCooldown) {
        currentCooldown = newCooldown;
    }

    void FixedUpdate() {
        if (currentCooldown <= 0f) return;

        currentCooldown -= Time.deltaTime;
        spellCooldown.fillAmount = Mathf.Clamp01(currentCooldown / cooldown);
        if (currentCooldown <= 0f) spellButton.interactable = true;
    }
}
