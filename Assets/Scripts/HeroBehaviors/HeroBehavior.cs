using UnityEngine;

public abstract class HeroBehavior : MonoBehaviour {
    [SerializeField] Hero hero;

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

    protected abstract void DoSpell(Hero hero);

    public Hero GetHero() {
        return hero;
    }

    public void SaveHero() {
        if (GroupManager.Instance != null) { GroupManager.Instance.SaveHero(hero); }
        if (BarbarianManager.Instance != null) { BarbarianManager.Instance.SaveHero(hero); }
    }
}
