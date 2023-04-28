using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

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

public class BarbarianManager : MonoBehaviour {
    [SerializeField] float timerBeforeBarbarian;
    [SerializeField] TextMeshProUGUI textTimer;
    [SerializeField] List<Hero> HeroesToSave = new List<Hero>();
    [SerializeField] TextMeshProUGUI textRecap;

    [SerializeField] UnityEvent OnTimerOver;

    float currentTimer;

    void Start() {
        currentTimer = timerBeforeBarbarian;
        UpdateRecap();
    }

    void FixedUpdate() {
        currentTimer -= Time.deltaTime;
        if (textTimer != null && currentTimer >= 0f) {
            var minutes = Mathf.Floor(currentTimer / 60).ToString("00");
            var seconds = (currentTimer % 60).ToString("00");
            textTimer.text = minutes + " : " + seconds;
        }
        if (currentTimer <= 0f) {
            textTimer.text = "XX : XX";
            OnTimerOver?.Invoke();
            enabled = false;
        }
    }

    void UpdateRecap() {
        if (textRecap == null) return;

        var recap = "Still to save: ";
        foreach (var hero in HeroesToSave) {
            recap += "\n - " + hero;
        }
        if (HeroesToSave.Count == 0) {
            recap += "\n - no one";
        }
        textRecap.text = recap;
    }

    public void SaveHero(Hero hero) {
        if (HeroesToSave.Contains(hero)) {
            HeroesToSave.Remove(hero);
            UpdateRecap();
        } else {
            Debug.LogError("Hero is not declared : " + hero);
        }
    }

    public void LoseHero(Hero hero) {
        if (!HeroesToSave.Contains(hero)) {
            HeroesToSave.Add(hero);
            UpdateRecap();
        } else {
            Debug.LogError("Hero is already declared : " + hero);
        }
    }
}
