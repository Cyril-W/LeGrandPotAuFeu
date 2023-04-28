using UnityEngine;
using UnityEngine.Events;

public class HeroBehavior : MonoBehaviour {
    [SerializeField] Hero hero;
    [SerializeField] UnityEvent<Hero> onHeroSaved;

    public void SaveHero() {
        onHeroSaved?.Invoke(hero);
    }
}
