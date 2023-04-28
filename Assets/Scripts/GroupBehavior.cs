using UnityEngine;
using System.Linq;
using System;

[Serializable]
public class HeroModel {
    public Hero Hero;
    public GameObject Model;
    public bool Saved = false;
}

public class GroupBehavior : MonoBehaviour {
    [SerializeField] HeroModel[] heores;

    void Start() {
        foreach (var h in heores) {
            if (h.Model != null) h.Model.SetActive(h.Saved);
        }
    }

    public void SaveHero(Hero hero) {
        if (heores.Any(h => h.Hero == hero)) {
            var savedHero = heores.Where(h => h.Hero == hero).FirstOrDefault();
            if (savedHero != null && savedHero.Model != null) {
                savedHero.Saved = true;
                savedHero.Model.SetActive(savedHero.Saved);
            }
        }
    }
}
