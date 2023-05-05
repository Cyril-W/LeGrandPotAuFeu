using UnityEngine;
using System.Linq;
using System;

public enum Hero {
    Ranger,
    Witch,
    Elf,
    Barbarian,
    Ogre,
    Thief,
    Minstrel,
    Dwarf
}

public class GroupManager : MonoBehaviour {
    [System.Serializable]
    class HeroModel {
        [HideInInspector] public string HeroName;
        public Hero Hero;
        public GameObject Model;
        public SpellBehavior SpellBehavior;
        public HeroBehavior HeroBehavior;
        public bool Saved = false;
    }

    public static GroupManager Instance { get; private set; }

    [SerializeField] HeroModel[] heroes;

    public Action<Hero> OnSpellClick;

    ThirdPersonController thirdPersonController;

    void Awake() {
        if (Instance == null || Instance != this) { Instance = this; }
 #if !UNITY_EDITOR
        foreach (var heroe in heroes) {
            heroe.Saved = false;
        }
#endif
    }

    void Start() {
        if (thirdPersonController == null) { thirdPersonController = GetComponent<ThirdPersonController>(); }
        UpdateHeroes();
    }

    [ContextMenu("Update Heroes")]
    void UpdateHeroes() {
        foreach (var h in heroes) {
            UpdateHero(h);
            if (BarbarianManager.Instance != null && h.Saved) { BarbarianManager.Instance.SaveHero(h.Hero); }
        }
    }

    public Vector3 GetPlayerPosition() {
        return transform.position;
    }

    public void MovePlayerPosition(Vector3 newPosition) {
        transform.position = newPosition;
    }

    public ThirdPersonController GetThirdPersonController() {
        return thirdPersonController;
    }

    public void SaveHero(Hero hero) {
        if (heroes.Any(h => h.Hero == hero)) {
            var savedHero = heroes.Where(h => h.Hero == hero).FirstOrDefault();
            if (savedHero != null) { UpdateHero(savedHero, true); }
        }
    }

    void UpdateHero(HeroModel hero) {
        if (hero.Model != null) { hero.Model.SetActive(hero.Saved); }
        if (hero.SpellBehavior != null) { hero.SpellBehavior.gameObject.SetActive(hero.Saved); }
        if (hero.HeroBehavior != null) {
            hero.HeroBehavior.enabled = hero.Saved;
            hero.HeroBehavior.transform.GetChild(0).gameObject.SetActive(!hero.Saved);
        }
    }

    void UpdateHero(HeroModel hero, bool saved) {
        hero.Saved = saved;
        UpdateHero(hero);
    }

    [ContextMenu("Fill Hero Models")]
    void FillHeroeModels() {
        var spellBehaviors = FindObjectsOfType<SpellBehavior>();
        var heroBehaviors = FindObjectsOfType<HeroBehavior>();
        foreach (var h in heroes) {
            h.HeroName = h.Hero.ToString();
            if (h.Model == null) {
                var child = transform.GetChild(1).Find(h.Hero.ToString());
                if (child != null) h.Model = child.gameObject;
            }
            if (h.SpellBehavior == null) { h.SpellBehavior = spellBehaviors.FirstOrDefault(sB => sB.GetHero() == h.Hero); }
            if (h.HeroBehavior == null) { h.HeroBehavior = heroBehaviors.FirstOrDefault(hB => hB.GetHero() == h.Hero); }
        }
    }
}
