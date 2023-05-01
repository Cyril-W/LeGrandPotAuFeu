using UnityEngine;
using UnityEngine.UI;

public class SpellBehavior : MonoBehaviour {
    [SerializeField] Hero hero;
    [SerializeField] Button spellButton;
    [SerializeField] Image spellCooldown;
    [SerializeField] float cooldown;

    float currentCooldown = 0f;

    public Hero GetHero() {
        return hero;
    }

    void Start() {
        spellCooldown.fillAmount = Mathf.Clamp01(currentCooldown / cooldown);
    }

    public void OnSpellClick() {
        currentCooldown = cooldown;
        spellButton.interactable = false;
    }

    void FixedUpdate() {
        if (currentCooldown <= 0f) return;

        currentCooldown -= Time.deltaTime;
        spellCooldown.fillAmount = Mathf.Clamp01(currentCooldown / cooldown);
        if (currentCooldown <= 0f) spellButton.interactable = true;
    }
}
