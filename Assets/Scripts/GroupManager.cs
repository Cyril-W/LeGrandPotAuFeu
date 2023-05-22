using UnityEngine;
using System.Linq;
using System;
using UnityEngine.Events;
using System.Collections.Generic;

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

    [SerializeField] Transform rangerTransform;
    [SerializeField] float timeToCrouch = 0.5f;
    [SerializeField] Vector2 crouchStandScale = new Vector2(0.75f, 1f);
    [SerializeField] HeroModel[] heroes;
    [SerializeField] UnityEvent onHeroLost;

    public Action<Hero> OnSpellClick;

    ThirdPersonController thirdPersonController;
    float currentCrouchTime = 0;
    int numberHeroSaved = 0;

    void Awake() {
        if (Instance == null || Instance != this) { Instance = this; }
#if !UNITY_EDITOR
        foreach (var heroe in heroes) {
            heroe.Saved = false;
        }
        numberHeroSaved = 0;
#endif
    }

    void Start() {
        if (thirdPersonController == null) { thirdPersonController = GetComponent<ThirdPersonController>(); }
        UpdateHeroes();
    }

    void FixedUpdate() {
        if (DestinyManager.Instance != null && DestinyManager.Instance.AnyTrackingGuard()) { Crouch(false); return; }
        if (currentCrouchTime > 0f) {            
            currentCrouchTime -= Time.deltaTime;
            var newScale = Mathf.Lerp(crouchStandScale.x, crouchStandScale.y, Mathf.Clamp01(currentCrouchTime / timeToCrouch));
            if (rangerTransform != null) { rangerTransform.localScale = new Vector3(crouchStandScale.y, newScale, crouchStandScale.y); }
            if (currentCrouchTime <= 0f) {
                if (GuardsManager.Instance != null) { GuardsManager.Instance.SetGuardsVisionOffset(true); }
            }
        }
    }

    [ContextMenu("Update Heroes")]
    void UpdateHeroes() {
        numberHeroSaved = 0;
        foreach (var h in heroes) {
            UpdateHero(h);
            if (h.Saved) {
                if (BarbarianManager.Instance != null) { BarbarianManager.Instance.SaveHero(h.Hero); }
                numberHeroSaved++;
            }
        }
    }

    public void Crouch(bool isCrouched) {
        if (rangerTransform == null) { return; }
        if (isCrouched) {
            currentCrouchTime = timeToCrouch;
        } else {
            currentCrouchTime = 0f;
            if (GuardsManager.Instance != null) { GuardsManager.Instance.SetGuardsVisionOffset(false); }
            if (rangerTransform != null) { rangerTransform.localScale = Vector3.one * crouchStandScale.y; }
        }
        if (thirdPersonController == null) { return; }
        thirdPersonController.SetIsCrouched(isCrouched);
    }

    public List<Hero> GetHeroList() {
        var heroList = new List<Hero>();
        foreach (var hero in heroes) {
            heroList.Add(hero.Hero);
        }
        return heroList;
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

    public bool LoseRandomMember() {
        var savedHeroes = heroes.Where(h => h.Saved);
        if (savedHeroes == null || savedHeroes.Count() <= 0) { return false;}
        var lostHero = savedHeroes.ElementAt(UnityEngine.Random.Range(0, savedHeroes.Count()));
        if (lostHero != null) { 
            lostHero.Saved = false;
            UpdateHero(lostHero);
            if (CanvasTooltip.Instance != null) { CanvasTooltip.Instance.HideTooltip(); }
            onHeroLost?.Invoke();
            return true;
        } else { return false; }
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
        numberHeroSaved += hero.Saved ? 1 : -1;
        UpdateHero(hero);
    }

    public int GetNumberHeroSaved() {
        return numberHeroSaved;
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
