using UnityEngine;
using DG.Tweening;
using System;

[Serializable]
public class DoShakeRotationParameters {
    public float Duration;
    public Vector3 Strength;
    public int Vibrato = 10;
    [Range(0f, 180f)] public float Randomness = 90f;
    public bool FadeOut = true;
    public ShakeRandomnessMode ShakeMode = ShakeRandomnessMode.Harmonic;

    public Action OnComplete;

    public DoShakeRotationParameters(float duration, Vector3 strength, int vibrato = 10, float randomness = 90f, bool fadeOut = true, ShakeRandomnessMode shakeMode = ShakeRandomnessMode.Harmonic) {
        Duration = duration;
        Strength = strength;
        Vibrato = vibrato;
        Randomness = randomness;
        FadeOut = fadeOut;
        ShakeMode = shakeMode;
    }

    public DoShakeRotationParameters(DoShakeRotationParameters other) : this(other.Duration, other.Strength, other.Vibrato, other.Randomness, other.FadeOut, other.ShakeMode) { }

    public void DoShakeRotation(Transform transformToRotate) {
        transformToRotate.DOShakeRotation(Duration, Strength, Vibrato, Randomness, FadeOut, ShakeMode).OnComplete(() => OnComplete?.Invoke());
    }
}