using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class BarbarianManager : MonoBehaviour {
    public static BarbarianManager Instance { get; private set; }

    [SerializeField] float timerBeforeBarbarian;
    [SerializeField] TextMeshProUGUI textTimer;
    [SerializeField] TextMeshProUGUI textRecap;
    [Space()]
    [SerializeField] UnityEvent onTimerOver;

    List<Hero> heroesToSave = new List<Hero>();
    string recapIfTimesUp = "";
    float currentTimer;
    bool isInitialized = false;

    void Awake() {
        if (Instance == null || Instance != this) { Instance = this; }
        TryInitialize();
    }

    void OnEnable() {
        TryInitialize();
    }

    void Start() {
        TryInitialize();
        recapIfTimesUp = CreateRecap();
        UpdateRecap(recapIfTimesUp);
        currentTimer = timerBeforeBarbarian;
    }

    void OnValidate() {
        currentTimer = timerBeforeBarbarian;
    }

    void FixedUpdate() {
        if (!TryInitialize()) { return; }
        currentTimer -= Time.deltaTime;
        if (textTimer != null && currentTimer >= 0f) {
            var minutes = Mathf.Floor(currentTimer / 60).ToString("00");
            var seconds = (currentTimer % 60).ToString("00");
            textTimer.text = minutes + " : " + seconds;
        }
        if (currentTimer <= 0f) {
            textTimer.text = "XX : XX";
            UpdateRecap(recapIfTimesUp);
            onTimerOver?.Invoke();
            enabled = false;
        }
    }

    bool TryInitialize() {
        if (!isInitialized && GroupManager.Instance != null) {
            heroesToSave = GroupManager.Instance.GetHeroList();
            isInitialized = true;
        }
        return isInitialized;
    }

    void UpdateRecap(string overrideRecap = "") {
        if (textRecap == null) return;
        textRecap.text = overrideRecap.Length > 0 ? overrideRecap : CreateRecap();
    }

    string CreateRecap () {
        if (!TryInitialize()) { return "Not initialized"; }
        var recap = "Still to save: ";
        if (heroesToSave.Count == 0) {
            recap += "\n - no one";
        } else {
            foreach (var hero in heroesToSave) {
                recap += "\n - " + hero + " (200 gold)";
            }
            recap += "\n - " + Hero.Ranger + " (200 gold)";
        }
        recap += "\n-----------------------";
        recap += "\n<u>Total:</u> " + (heroesToSave.Count > 0 ? (heroesToSave.Count + 1) * 200 : 0) + " gold";
        return recap;
    }

    public void SaveHero(Hero hero) {
        if (!TryInitialize()) { return; }
        if (heroesToSave.Contains(hero)) {
            heroesToSave.Remove(hero);
            UpdateRecap();
        } else {
            Debug.LogError("Hero is not declared : " + hero);
        }
    }

    public void LoseHero(Hero hero) {
        if (!TryInitialize()) { return; }
        if (!heroesToSave.Contains(hero)) {
            heroesToSave.Add(hero);
            UpdateRecap();
        } else {
            Debug.LogError("Hero is already declared : " + hero);
        }
    }
}
