using UnityEngine;
using UnityEngine.Events;

public abstract class HeroBehavior : MonoBehaviour {
    [SerializeField] Hero hero;
    [SerializeField] Transform savedHeroTransform;
    [SerializeField] UnityEvent OnSpellClicked;

    bool registered = false;

    protected virtual void OnEnable() {
        if (GroupManager.Instance != null) {
            GroupManager.Instance.RegisterHero(hero, DoSpell);
            registered = true;
        } else {
            Debug.LogWarning("No GroupManager.Instance to register " + hero);
            registered = false;
        }
    }

    protected virtual void OnDisable() {
        if (GroupManager.Instance != null) {
            GroupManager.Instance.UnregisterHero(hero);
            registered = false;
        } else {
            Debug.LogWarning("No GroupManager.Instance to unregister " + hero);
        }
    }

    protected virtual void FixedUpdate() {
        if (!registered) {
            Debug.LogWarning("FixedUpdate register of " + hero);
            GroupManager.Instance.RegisterHero(hero, DoSpell);
            registered = true;
        }
    }

    bool DoSpell() {
        //if (hero == GetHero()) {
            OnSpellClicked?.Invoke();
            return OverrideDoSpell();
        //} else { return false; }
    }

    protected abstract bool OverrideDoSpell();

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
