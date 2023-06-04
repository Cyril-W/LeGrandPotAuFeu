using UnityEngine;
using System.Linq;
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
        public GameObject Sheep;
        public SpellBehavior SpellBehavior;
        public HeroBehavior HeroBehavior;
        public SteeringBehavior SteeringBehavior;
        public bool Saved = false;
    }

    public static GroupManager Instance { get; private set; }

    [SerializeField] Transform rangerTransform;
    [SerializeField] float timeToCrouch = 0.5f;
    [SerializeField] Vector2 crouchStandScale = new Vector2(0.75f, 1f);
    [SerializeField] Transform rangeIndicator;
    [SerializeField] HeroModel[] heroes;
    [SerializeField] UnityEvent onHeroLost;

    Dictionary<Hero, System.Func<bool>> onSpellClick = new Dictionary<Hero, System.Func<bool>>();

    ThirdPersonController thirdPersonController;
    float currentCrouchTime = 0, currentSheepDuration = 0f;
    int numberHeroSaved = 0;

    void Awake() {
        if (Instance == null || Instance != this) { Instance = this; }
#if !UNITY_EDITOR
        foreach (var hero in heroes) {
            hero.Saved = hero.Hero == Hero.Ranger/*false*/;
        }
        numberHeroSaved = 1;
#endif
    }

    void Start() {
        if (thirdPersonController == null) { thirdPersonController = GetComponent<ThirdPersonController>(); }
        onSpellClick.Clear();
        UpdateHeroes();
    }

    void FixedUpdate() {
        if (currentSheepDuration > 0f) {
            currentSheepDuration -= Time.deltaTime;
            if (currentSheepDuration <= 0f) {
                UpdateSheepsAndModels(false);
            }
        }
        if (currentCrouchTime > 0f && ((DestinyManager.Instance != null && DestinyManager.Instance.AnyTrackingGuard()) || (LevelManager.Instance != null && LevelManager.Instance.IsPaused))) { Crouch(false); return; }
        if (currentCrouchTime > 0f) {            
            currentCrouchTime -= Time.deltaTime;
            var newScale = Mathf.Lerp(crouchStandScale.x, crouchStandScale.y, Mathf.Clamp01(currentCrouchTime / timeToCrouch));
            if (rangerTransform != null) { rangerTransform.localScale = new Vector3(crouchStandScale.y, newScale, crouchStandScale.y); }
            if (currentCrouchTime <= 0f) {
                if (GuardsManager.Instance != null) { GuardsManager.Instance.SetGuardsVisionOffset(true); }
                foreach (var h in heroes) {
                    if (h.SteeringBehavior != null) { h.SteeringBehavior.SetIsCrouched(true); }
                }
            }
        }
    }

    public void RegisterHero(Hero hero, System.Func<bool> heroSpell) {
        if (onSpellClick.ContainsKey(hero)) { return; }
        onSpellClick.Add(hero, heroSpell);
    }

    public void UnregisterHero(Hero hero) {
        if (!onSpellClick.ContainsKey(hero)) { return; }
        onSpellClick.Remove(hero);
    }

    public bool CallHeroSpell(Hero hero) {
        if (!onSpellClick.ContainsKey(hero)) {
            Debug.LogError("No HeroSpell function registered for " + hero);
            return false;
        }
        return onSpellClick[hero]();
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

    public void SetSheep(float duration) {
        currentSheepDuration = duration;
        UpdateSheepsAndModels(true);
    }

    void UpdateSheepsAndModels(bool isSheep) {
        foreach (var h in heroes) {
            if (h.SteeringBehavior) {
                h.SteeringBehavior.gameObject.SetActive(h.Saved);
            }
            if (h.Saved && h.Model && h.Sheep) { 
                h.Model.SetActive(!isSheep);
                h.Sheep.SetActive(isSheep);
            }
        }
    }

    public void Crouch(bool isCrouched) {
        if (rangerTransform == null || LevelManager.Instance == null || LevelManager.Instance.IsPaused) { return; }
        if (isCrouched) {
            currentCrouchTime = timeToCrouch;
        } else {
            currentCrouchTime = 0f;
            if (GuardsManager.Instance != null) { GuardsManager.Instance.SetGuardsVisionOffset(false); }
            if (rangerTransform != null) { rangerTransform.localScale = Vector3.one * crouchStandScale.y; }
        }
        if (thirdPersonController == null) { return; }
        thirdPersonController.SetIsCrouched(isCrouched);
        if (!isCrouched) {
            foreach (var h in heroes) {
                if (h.SteeringBehavior != null) { h.SteeringBehavior.SetIsCrouched(isCrouched); }
            }
        }
    }

    public void SetRangeIndicator(float rangeScale) {
        if (rangeIndicator == null) { return; }
        rangeIndicator.localScale = Vector3.one * rangeScale * 2f;
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

    public void SetGroupPosition(Vector3 newPosition) {
        transform.position = newPosition;
        if (SteeringManager.Instance != null) { SteeringManager.Instance.ResetPositions(); }
    }

    public Vector3[] GetUnsavedHeroPositions() {
        var heroPos = new List<Vector3>();
        foreach (var hero in heroes) {
            if (hero.Hero != Hero.Ranger && !hero.Saved) { heroPos.Add(hero.HeroBehavior.GetSavedHeroPosition()); }
        }
        return heroPos.ToArray();
    }

    public void SetPlayerRotation(float yRotation) {
        var newRotation = transform.rotation.eulerAngles;
        newRotation.y = yRotation;
        transform.rotation = Quaternion.Euler(newRotation);
    }

    public ThirdPersonController GetThirdPersonController() {
        return thirdPersonController;
    }

    public void HitGroup(Vector3 position) {
        foreach (var hero in heroes) {
            if (hero.SteeringBehavior != null) {
                hero.SteeringBehavior.GetHit(position);
            }
        }
        if (thirdPersonController != null) { thirdPersonController.GetHit(position); }
    }

    public void SaveHero(Hero hero) {
        if (heroes.Any(h => h.Hero == hero)) {
            var savedHero = heroes.Where(h => h.Hero == hero).FirstOrDefault();
            if (savedHero != null) { UpdateHero(savedHero, true); }
        }
    }

    public bool LoseMember(Hero hero) {
        if (hero == Hero.Ranger) { return false; }
        var savedHeroes = heroes.Where(h => h.Hero == hero && h.Saved);
        if (savedHeroes == null || savedHeroes.Count() <= 0) { return false; }
        return LoseHero(savedHeroes.ElementAt(0));
    }

    public bool LoseRandomMember() {
        var savedHeroes = heroes.Where(h => h.Hero != Hero.Ranger && h.Saved);
        if (savedHeroes == null || savedHeroes.Count() <= 0) { return false;}
        return LoseHero(savedHeroes.ElementAt(Random.Range(0, savedHeroes.Count())));        
    }

    bool LoseHero(HeroModel heroModel) {
        if (heroModel != null) {
            heroModel.Saved = false;
            UpdateHero(heroModel);
            if (CanvasTooltip.Instance != null) { CanvasTooltip.Instance.HideTooltip(); }
            onHeroLost?.Invoke();
            return true;
        } else { return false; }
    }

    void UpdateHero(HeroModel hero) {
        if (hero.SteeringBehavior) {
            hero.SteeringBehavior.gameObject.SetActive(hero.Saved);
        }
        if (hero.Saved && hero.Model && hero.Sheep) {
            hero.Model.SetActive(true);
            hero.Sheep.SetActive(false);
        }
        if (hero.SpellBehavior != null) { hero.SpellBehavior.SetSaved(hero.Saved); }
        if (hero.HeroBehavior != null) {
            hero.HeroBehavior.enabled = hero.Saved;
            if (hero.Hero != Hero.Ranger) { hero.HeroBehavior.transform.GetChild(0).gameObject.SetActive(!hero.Saved); }
        }
    }

    void UpdateHero(HeroModel hero, bool saved) {
        if (hero.Hero == Hero.Ranger && !saved) { return; }
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
