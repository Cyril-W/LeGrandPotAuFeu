using UnityEngine;
using DG.Tweening;
using System;

[Serializable]
public class DoPunchScaleParameters {
    public Vector3 Punch;
    public float Duration;
    public int Vibrato = 10;
    [Range(0f, 1f)] public float Elasticity = 1f;

    public Action OnComplete;

    public DoPunchScaleParameters(Vector3 punch, float duration, int vibrato = 10, float elasticity = 1f) {
        Punch = punch;
        Duration = duration;
        Vibrato = vibrato;
        Elasticity = elasticity;
    }

    public DoPunchScaleParameters(DoPunchScaleParameters other) : this(other.Punch, other.Duration, other.Vibrato, other.Elasticity) { }

    public void DoPunchScale(Transform transformToScale) {
        transformToScale.DOPunchScale(Punch, Duration, Vibrato, Elasticity).OnComplete(() => OnComplete?.Invoke());
    }
}