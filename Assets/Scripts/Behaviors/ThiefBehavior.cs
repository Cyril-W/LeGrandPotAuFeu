using System.Linq;
using UnityEngine;

public class ThiefBehavior : HeroBehavior {
    [SerializeField] LockedObjectBehavior[] jails;
    [SerializeField] float minDistanceToJail = 4f;
    [Header("Gizmos")]
    [SerializeField] Color sphereColor = Color.black;

    void OnValidate() {
        TryFillNull();
    }

    void Start() {
        TryFillNull(true);
    }

    void TryFillNull(bool force = false) {
        if (force || jails == null || jails.Length <= 0) { jails = FindObjectsOfType<LockedObjectBehavior>(true); }
    }

    protected override bool OverrideDoSpell() {        
        return LockPick();
    }

    [ContextMenu("Lock Pick")]
    bool LockPick() {
        if (GroupManager.Instance == null) { return false; }
        var currentNearestDistance = Mathf.Infinity;
        LockedObjectBehavior currentJail = null;
        var currentDistance = 0f;
        foreach (var jail in jails) {
            if (!jail.GetIsLocked()) { continue; }
            currentDistance = Vector3.Distance(GroupManager.Instance.GetPlayerPosition(), jail.transform.position);
            if (currentDistance <= minDistanceToJail && currentDistance < currentNearestDistance) {
                currentNearestDistance = currentDistance;
                currentJail = jail;
            }
        }
        if (currentJail != null) {
            currentJail.LockPickSuccess();
            return true;
        } else { return false; }
    }

    void OnDrawGizmos() {
        if (!isActiveAndEnabled || GroupManager.Instance == null) { return; }
        Gizmos.color = sphereColor;
        Gizmos.DrawWireSphere(GroupManager.Instance.GetPlayerPosition(), minDistanceToJail);
    }
}
