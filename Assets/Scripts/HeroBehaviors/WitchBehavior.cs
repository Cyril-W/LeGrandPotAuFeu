using UnityEngine;
using System.Collections.Generic;

public class WitchBehavior : HeroBehavior {
    [SerializeField] float teleportationDuration = 1f;
    [SerializeField] Collider[] collidersToAvoid;
    [SerializeField, Layer] int obstacleLayer = 3;
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

    void Awake() {
        TryFillVoid(false);
    }

    protected override void OnEnable() {
        base.OnEnable();
        TryFillVoid(false);
        GetTeleportPoints();
        SetGuardLayer(true);
    }

    protected override void OnDisable() {
        base.OnDisable();
        SetGuardLayer(false);
    }

    void TryFillVoid(bool force = true) {
        if (force || collidersToAvoid.Length <= 0) {
            var allCollidersToAvoid = new List<Collider>();
            var boxColliders = FindObjectsOfType<BoxCollider>();
            foreach (var coll in boxColliders) {
                if (coll.enabled && coll.gameObject.layer == obstacleLayer) allCollidersToAvoid.Add(coll);
            }
            var sphereColliders = FindObjectsOfType<SphereCollider>();
            foreach (var coll in sphereColliders) {
                if (coll.enabled && coll.gameObject.layer == obstacleLayer) allCollidersToAvoid.Add(coll);
            }
            collidersToAvoid = allCollidersToAvoid.ToArray();
        }
    }

    [ContextMenu("Try Fill arrays")]
    void ContextTryFillVoid() {
        TryFillVoid();
    }

    void SetGuardLayer(bool isActive) {
        if (GuardsManager.Instance != null) { GuardsManager.Instance.SetGuardsLayer(isActive ? enemiLayer : defaultLayer); }
    }

    void FixedUpdate() {
        if (currentTeleportTime > 0f) {
            currentTeleportTime -= Time.deltaTime;
            if (currentTeleportTime <= 0f) { SetThirdPersonControllerEnabled(true); }
        }
    }

    protected override void OverrideDoSpell() {
        Teleport();
    }

    [ContextMenu("Teleport")]
    void Teleport() {
        currentTeleportTime = teleportationDuration;
        SetThirdPersonControllerEnabled(false);
        GetTeleportPoints();
        if (teleportPoints.Count <= 0 || GroupManager.Instance == null) return;
        GroupManager.Instance.MovePlayerPosition(teleportPoints[Random.Range(0, teleportPoints.Count)] + Vector3.up);
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
                    foreach (var coll in collidersToAvoid) {
                        if (coll.bounds.Contains(point) || Vector3.Distance(coll.ClosestPointOnBounds(point), point) <= distanceToObstacles) {
                            isToAvoid = true;
                            break;
                        }
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

    void OnDrawGizmos() {
        Gizmos.color = teleportSphereColor;
        foreach (var teleportPoint in teleportPoints) {
            Gizmos.DrawSphere(teleportPoint, teleportSphereSize);
        }
        if (!isActiveAndEnabled || GroupManager.Instance == null) { return; }
        Gizmos.color = playerSphereColor;
        Gizmos.DrawWireSphere(GroupManager.Instance.GetPlayerPosition(), distanceToPlayer);
    }
}
