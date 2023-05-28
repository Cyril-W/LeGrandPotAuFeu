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
            if (guard.gameObject.activeSelf) {
                positions.Add(guard.GetGuardPosition());
            } else {
                positions.Add(new Vector3(0f, Mathf.Infinity, 0f));
            }
        }
        return positions.ToArray();
    }

    public void DisableGuard(int guardIndex) {
        if (guardIndex < 0 || guardIndex >= guards.Length) { return; }
        var guard = guards[guardIndex];
        guard.gameObject.SetActive(false);
        if (DestinyManager.Instance != null) { DestinyManager.Instance.LostTrack(guard); }
    }

    public void ImmobilizeGuard(int guardIndex) {
        if (guardIndex < 0 || guardIndex >= guards.Length) { return; }
        guards[guardIndex].ImmobilizeGuard();
    }
}
