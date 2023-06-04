using UnityEngine;
using System.Collections.Generic;

public class WitchBehavior : HeroBehavior {
    [SerializeField, Range(0, 100)] float chancePercentage = 75f;
    [SerializeField] float sheepDuration = 2f;
    [SerializeField] float teleportationDuration = 1f;
    [SerializeField] BoxCollider teleportArea;
    [SerializeField, Range(0, 100)] int numberXTeleportPoints = 20;
    [SerializeField, Range(0, 100)] int numberZTeleportPoints = 20;
    [SerializeField] float distanceToObstacles = 2f;
    [SerializeField] float distanceToPlayer = 20f;
    [SerializeField, Layer] int defaultLayer = 0;
    [SerializeField, Layer] int enemiLayer = 6;
    [Header("Gizmos")]
    [SerializeField] Color playerSphereColor = Color.red;
    [SerializeField] float teleportSphereSize = 0.1f;
    [SerializeField] Color teleportSphereColor = Color.magenta;

    List<Vector3> teleportPoints = new List<Vector3>();
    float currentTeleportTime = 0f;

    protected override void OnEnable() {
        base.OnEnable();
        GetTeleportPoints();
        SetGuardLayer(true);
    }

    protected override void OnDisable() {
        base.OnDisable();
        SetGuardLayer(false);
    }

    void SetGuardLayer(bool isActive) {
        if (GuardsManager.Instance != null) { GuardsManager.Instance.SetGuardsLayer(isActive ? enemiLayer : defaultLayer); }
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();
        if (currentTeleportTime > 0f) {
            currentTeleportTime -= Time.deltaTime;
            if (currentTeleportTime <= 0f) { SetThirdPersonControllerEnabled(true); }
        }
    }

    protected override bool OverrideDoSpell() {
        return Teleport();
    }

    [ContextMenu("Teleport")]
    bool Teleport() {
        if (GroupManager.Instance == null) { return false; }
        SetThirdPersonControllerEnabled(false);
        if (Random.Range(0f, 100f) <= chancePercentage) {            
            currentTeleportTime = teleportationDuration;
            GetTeleportPoints();
            if (teleportPoints.Count <= 0) { return false; }
            GroupManager.Instance.SetGroupPosition(teleportPoints[Random.Range(0, teleportPoints.Count)]);
            return true;
        } else {
            currentTeleportTime = sheepDuration;
            GroupManager.Instance.SetSheep(sheepDuration);
            return false;            
        }
    }

    void SetThirdPersonControllerEnabled(bool b) {
        if (GroupManager.Instance == null || GroupManager.Instance.GetThirdPersonController() == null) { return; }
        GroupManager.Instance.GetThirdPersonController().enabled = b;
    }

    void GetTeleportPoints() {
        teleportPoints.Clear();
        var boxBounds = teleportArea.bounds;
        var topLeft = new Vector3(boxBounds.center.x - boxBounds.extents.x, 0f, boxBounds.center.z + boxBounds.extents.z);
        var topRight = new Vector3(boxBounds.center.x + boxBounds.extents.x, 0f, boxBounds.center.z - boxBounds.extents.z);
        var bottomLeft = new Vector3(boxBounds.center.x - boxBounds.extents.x, 0f, boxBounds.center.z - boxBounds.extents.z);
        var x = topLeft.x;
        for (var i = 0; i <= numberXTeleportPoints; i++) {
            var z = topLeft.z;
            for (var j = 0; j <= numberZTeleportPoints; j++) {
                var point = new Vector3(x, 0f, z);
                if (GroupManager.Instance != null && Vector3.Distance(GroupManager.Instance.GetPlayerPosition(), point) > distanceToPlayer) {
                    var isToAvoid = false;
                    if (!isToAvoid && GuardsManager.Instance != null) {
                        isToAvoid = GuardsManager.Instance.CollideWithAny(point, distanceToObstacles);
                    }
                    if (!isToAvoid && SteeringManager.Instance != null) {
                        isToAvoid = SteeringManager.Instance.CollideWithAny(point, distanceToObstacles);
                    }                    
                    if (!isToAvoid) {
                        teleportPoints.Add(point);
                    }
                }
                z -= Mathf.Abs(topLeft.z - bottomLeft.z) / numberZTeleportPoints;
            }
            x += Mathf.Abs(topLeft.x - topRight.x) / numberXTeleportPoints;
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = teleportSphereColor;
        foreach (var teleportPoint in teleportPoints) {
            Gizmos.DrawSphere(teleportPoint, teleportSphereSize);
        }
        if (!isActiveAndEnabled || GroupManager.Instance == null) { return; }
        Gizmos.color = playerSphereColor;
        Gizmos.DrawWireSphere(GroupManager.Instance.GetPlayerPosition(), distanceToPlayer);
    }
}
