using UnityEngine;
using UnityEngine.Events;

public abstract class HeroBehavior : MonoBehaviour {
    [SerializeField] Hero hero;
    [SerializeField] UnityEvent OnSpellClicked;

    protected virtual void OnEnable() {
        if (GroupManager.Instance != null) {
            GroupManager.Instance.OnSpellClick += DoSpell;
        }
    }

    protected virtual void OnDisable() {
        if (GroupManager.Instance != null) {
            GroupManager.Instance.OnSpellClick -= DoSpell;
        }
    }

    void DoSpell(Hero hero) {
        if (hero == GetHero()) {
            OnSpellClicked?.Invoke();
            OverrideDoSpell();
        }
    }

    protected abstract void OverrideDoSpell();

    public Hero GetHero() {
        return hero;
    }

    public void SaveHero() {
        if (GroupManager.Instance != null) { GroupManager.Instance.SaveHero(hero); }
        if (BarbarianManager.Instance != null) { BarbarianManager.Instance.SaveHero(hero); }
    }
}
