using System.Collections.Generic;
using UnityEngine;

public class GuardsManager : MonoBehaviour {
    public static GuardsManager Instance { get; private set; }

    [SerializeField] GuardBehavior[] guards;
    [SerializeField] Vector2 crouchStandVisionOffset = new Vector2(0.25f, 1f);
    [SerializeField] Vector2 offOnVisionRatio = new Vector2(0.5f, 1f);

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

    public void SetGuardsVisionRatio(bool isActive) {
        foreach (var guard in guards) {
            guard.SetGuardVisionRatio(isActive ? offOnVisionRatio.x : offOnVisionRatio.y);
        }
    }

    public void SetGuardsLayer(int layer) {
        foreach (var guard in guards) {
            guard.SetGuardLayer(layer);
        }
    }

    Vector3[] GetActiveGuardsPositions() {
        var positions = new List<Vector3>();
        foreach (var guard in guards) {
            if (guard.gameObject.activeSelf && guard.enabled) {
                positions.Add(guard.GetPosition());
            } else {
                positions.Add(new Vector3(0f, Mathf.Infinity, 0f));
            }
        }
        return positions.ToArray();
    }

    public bool CollideWithAny(Vector3 position, float distance) {
        foreach (var guardPosition in GetActiveGuardsPositions()) {
            if (Vector3.Distance(guardPosition, position) <= distance) {
                return true;
            }
        }
        return false;
    }

    public bool GetClosestGuard(Vector3 position, float maxDistance, out int lastGuardIndex, out Vector3 lastPos) {
        var positions = GetActiveGuardsPositions();
        var closestDistance = Mathf.Infinity;
        lastGuardIndex = -1;
        lastPos = position;
        for (int i = 0; i < positions.Length; i++) {
            var pos = positions[i];
            var newDistance = Vector3.Distance(position, pos);
            if (newDistance < maxDistance && newDistance < closestDistance) {
                closestDistance = newDistance;
                lastPos = pos;
                lastGuardIndex = i;
            }
        }
        return lastGuardIndex != -1;
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
