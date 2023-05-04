using UnityEngine;
using DG.Tweening;
using System;

[Serializable]
public class DoShakeRotationParameters {
    public float ShakeDuration;
    public Vector3 ShakeStrength;
    public int ShakeVibrato = 10;
    [Range(0f, 180f)] public float Randomness = 90f;
    public bool FadeOut = true;
    public ShakeRandomnessMode ShakeMode = ShakeRandomnessMode.Harmonic;

    public DoShakeRotationParameters(float shakeDuration, Vector3 shakeStrength, int shakeVibrato = 10, float randomness = 90f, bool fadeOut = true, ShakeRandomnessMode shakeMode = ShakeRandomnessMode.Harmonic) {
        ShakeDuration = shakeDuration;
        ShakeStrength = shakeStrength;
        ShakeVibrato = shakeVibrato;
        Randomness = randomness;
        FadeOut = fadeOut;
        ShakeMode = shakeMode;
    }

    public void DoShakeRotation(Transform transformToRotate) {
        transformToRotate.DOShakeRotation(ShakeDuration, ShakeStrength, ShakeVibrato, Randomness, FadeOut, ShakeMode);
    }
}