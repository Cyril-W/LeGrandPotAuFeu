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

    float currentTimer;

    void Awake() {
        if (Instance == null || Instance != this) { Instance = this; }
    }

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
            recap += "\n - " + hero + " (200 gold)";
        }
        if (HeroesToSave.Count == 0) {
            recap += "\n - no one";
        }
        recap += "\n-----------------------";
        recap += "\n<u>Total:</u> " + HeroesToSave.Count * 200 + " gold";
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
