using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class BarbarianManager : MonoBehaviour {
    public static BarbarianManager Instance { get; private set; }

    [SerializeField] float timerBeforeBarbarian;
    [SerializeField] TextMeshProUGUI textTimer;
    [SerializeField] List<Hero> HeroesToSave = new List<Hero>();
    [SerializeField] TextMeshProUGUI textRecap;

    [SerializeField] UnityEvent OnTimerOver;

    string recapIfTimesUp = "";
    float currentTimer;

    void Awake() {
        if (Instance == null || Instance != this) { Instance = this; }
        recapIfTimesUp = CreateRecap();
    }

    void Start() {
        currentTimer = timerBeforeBarbarian;
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
            UpdateRecap(recapIfTimesUp);
            OnTimerOver?.Invoke();
            enabled = false;
        }
    }

    void UpdateRecap(string overrideRecap = "") {
        if (textRecap == null) return;
        textRecap.text = overrideRecap.Length > 0 ? overrideRecap : CreateRecap();
    }

    string CreateRecap () {
        var recap = "Still to save: ";
        foreach (var hero in HeroesToSave) {
            recap += "\n - " + hero + " (200 gold)";
        }
        if (HeroesToSave.Count == 0) {
            recap += "\n - no one";
        }
        recap += "\n-----------------------";
        recap += "\n<u>Total:</u> " + HeroesToSave.Count * 200 + " gold";
        return recap;
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
