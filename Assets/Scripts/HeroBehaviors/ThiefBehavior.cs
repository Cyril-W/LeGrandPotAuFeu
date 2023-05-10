using System.Linq;
using UnityEngine;

public class ThiefBehavior : HeroBehavior {
    [SerializeField] JailBehavior[] jails;
    [SerializeField] float minDistanceToJail = 4f;
    [SerializeField] SpellBehavior spellBehavior;
    [SerializeField] float coolDownOnMiss = 1f;
    [Header("Gizmos")]
    [SerializeField] Color sphereColor = Color.black;

    void OnValidate() {
        TryFillNull();
    }

    void Start() {
        TryFillNull();
    }

    void TryFillNull() {
            if (jails == null || jails.Length <= 0) { jails = FindObjectsOfType<JailBehavior>(); }
            if (spellBehavior == null) { spellBehavior = FindObjectsOfType<SpellBehavior>().Where(sB => sB.GetHero() == GetHero()).FirstOrDefault(); }
    }

    protected override void OverrideDoSpell() {        
        LockPick();
    }

    [ContextMenu("Lock Pick")]
    void LockPick() {
        if (GroupManager.Instance == null) { return; }
        var currentNearestDistance = Mathf.Infinity;
        JailBehavior currentJail = null;
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
            currentJail.LockPicked();
        } else { 
            spellBehavior.SetCurrentCooldown(coolDownOnMiss);
        }
    }

    void OnDrawGizmos() {
        if (!isActiveAndEnabled || GroupManager.Instance == null) { return; }
        Gizmos.color = sphereColor;
        Gizmos.DrawWireSphere(GroupManager.Instance.GetPlayerPosition(), minDistanceToJail);
    }
}
