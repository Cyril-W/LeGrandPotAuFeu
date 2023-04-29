using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class DestinyManager : MonoBehaviour {
    public static DestinyManager Instance { get; private set; }

    [Header("Destiny Time")]
    [SerializeField, ReadOnly] List<GuardBehavior> trackingGuards = new List<GuardBehavior>();
    [SerializeField] AnimationCurve timeScaleCurve;
    [SerializeField] Volume postProcessVolume;
    [SerializeField] AnimationCurve vignetteIntensityCurve;
    [SerializeField, ReadOnly] float currentTimeScale;
    [Header("Destiny Coins")]
    [SerializeField] Image[] destinyCoins;
    [SerializeField] Color destinyCoinOnColor;
    [SerializeField] Color destinyCoinOffColor;
    [SerializeField] UnityEvent OnDestinyPointOver;

    int destinyPoints;
    float currentTrackingProgress;
    Vignette vignette;

    void Start() {
        if (Instance == null || Instance != this) { Instance = this; }
        destinyPoints = destinyCoins.Length;
        postProcessVolume.profile.TryGet(out vignette);
    }

    public void DestinyTimeScale(GuardBehavior guard, float trackingProgress) {
        if (!trackingGuards.Contains(guard)) trackingGuards.Add(guard);
        trackingProgress = Mathf.Clamp01(trackingProgress);
        if (trackingProgress <= currentTrackingProgress) return;
        currentTrackingProgress = trackingProgress;
        currentTimeScale = timeScaleCurve.Evaluate(trackingProgress);
        Time.timeScale = currentTimeScale;
        vignette.intensity.value = vignetteIntensityCurve.Evaluate(trackingProgress);
    }

    public void LostTrack(GuardBehavior guard) {
        currentTrackingProgress = 0f;
        if (trackingGuards.Contains(guard)) trackingGuards.Remove(guard);
        if (trackingGuards.Count <= 0) {
            currentTimeScale = timeScaleCurve.Evaluate(0f);
            Time.timeScale = currentTimeScale;
            vignette.intensity.value = vignetteIntensityCurve.Evaluate(0f);
        }
    }

    [ContextMenu("Gain Destiny Point")]
    public void DestinyPointGain() {
        if (destinyPoints == destinyCoins.Length) return;
        destinyPoints++;
        destinyCoins[destinyPoints - 1].color = destinyCoinOnColor;
    }

    [ContextMenu("Lose Destiny Point")]
    public void DestinyPointLose(GuardBehavior guard) {
        LostTrack(guard);
        if (destinyPoints == 0) return;
        destinyCoins[destinyPoints - 1].color = destinyCoinOffColor;
        destinyPoints--;

        if (destinyPoints == 0) { OnDestinyPointOver?.Invoke(); }
    }
}