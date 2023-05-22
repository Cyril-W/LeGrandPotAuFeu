using System.Collections.Generic;
using UnityEngine;

public class GuardsManager : MonoBehaviour {
    public static GuardsManager Instance { get; private set; }

    [SerializeField] GuardBehavior[] guards;
    [SerializeField] Vector2 crouchStandVisionOffset = new Vector2(0.25f, 1f);

    void Awake() {
        if (Instance == null || Instance != this) { Instance = this; }
    }

    void OnValidate() {
        //TryFillVoid();
    }

    void Start() {
        TryFillVoid();
        SetGuardsVisionOffset(false);
    }

    [ContextMenu("Refresh Guards")]
    void TryFillVoid() {
        guards = GetComponentsInChildren<GuardBehavior>();
    }

    public void SetGuardsEnable(bool isEnabled) {
        foreach (var guard in guards) {
            guard.enabled = isEnabled;
        }
    }

    public void SetGuardsVisionOffset(bool isCrouching) {
        foreach (var guard in guards) {
            guard.SetGuardOffsetVision(isCrouching ? crouchStandVisionOffset.x : crouchStandVisionOffset.y);
        }
    }

    public void SetGuardsLayer(int layer) {
        foreach (var guard in guards) {
            guard.SetGuardLayer(layer);
        }
    }

    public Vector3[] GetGuardsPositions() {
        var positions = new List<Vector3>();
        foreach (var guard in guards) {
            positions.Add(guard.GetGuardPosition());
        }
        return positions.ToArray();
    }
}
