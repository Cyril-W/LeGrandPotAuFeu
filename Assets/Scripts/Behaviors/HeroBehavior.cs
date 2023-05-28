using UnityEngine;
using UnityEngine.Events;

public abstract class HeroBehavior : MonoBehaviour {
    [SerializeField] Hero hero;
    [SerializeField] Transform savedHeroTransform;
    [SerializeField] UnityEvent OnSpellClicked;

    bool registered = false;

    protected virtual void OnEnable() {
        if (GroupManager.Instance != null) {
            GroupManager.Instance.OnSpellClick += DoSpell;
            registered = true;
        } else {
            Debug.LogWarning("No GroupManager.Instance to register " + hero);
            registered = false;
        }
    }

    protected virtual void OnDisable() {
        if (GroupManager.Instance != null) {
            GroupManager.Instance.OnSpellClick -= DoSpell;
            registered = false;
        } else {
            Debug.LogWarning("No GroupManager.Instance to unregister " + hero);
        }
    }

    protected virtual void FixedUpdate() {
        if (!registered) {
            Debug.LogWarning("FixedUpdate register of " + hero);
            GroupManager.Instance.OnSpellClick += DoSpell;
            registered = true;
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

    public Vector3 GetSavedHeroPosition() {
        if (savedHeroTransform == null) { return Vector3.up * Mathf.Infinity; }
        else { return savedHeroTransform.position; }
    }

    public void SaveHero() {
        if (GroupManager.Instance != null) { GroupManager.Instance.SaveHero(hero); }
        if (BarbarianManager.Instance != null) { BarbarianManager.Instance.SaveHero(hero); }
    }
}
