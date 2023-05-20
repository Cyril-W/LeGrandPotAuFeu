using UnityEngine;
using UnityEngine.Events;

public class ObjectVisibleToCameraEvents : MonoBehaviour {
    [SerializeField] Collider objectToCheck;
    [SerializeField] LayerMask layerToCast;
    [SerializeField] float maxDistance = 1000f;
    [SerializeField] bool useCameraFrustrumAsMaxDistance = true;
    [SerializeField] bool hitTriggers = false;
    [SerializeField] bool debugProjection = false;
    [Space]
    [SerializeField] UnityEvent onObjectVisible;
    [SerializeField] UnityEvent onObjectNotVisible;
    [Header("Gizmos")]
    [SerializeField] float sphereSize = 0.5f;
    [SerializeField] Color projectedSphereColor = Color.blue;
    [SerializeField] Color unProjectedSphereColor = Color.red;
    [SerializeField] Color projectionLineColor = Color.magenta;

    Vector3[] projectedPositions, unProjectedPositions;
    bool[] raycastResults;
    Ray ray;
    bool isVisible = false;
    int indexClosestToCamera = -1;

    const int POSITION_LENGTH = 9;

    void OnValidate() {
        if (!Application.isPlaying) { return; } 
        if (debugProjection && (unProjectedPositions == null || unProjectedPositions.Length < POSITION_LENGTH)) {
            unProjectedPositions = new Vector3[POSITION_LENGTH];
        }
    }

    void Start() {
        projectedPositions = new Vector3[POSITION_LENGTH];
        raycastResults = new bool[POSITION_LENGTH];
        if (debugProjection) { unProjectedPositions = new Vector3[POSITION_LENGTH]; };
    }

    void FixedUpdate() {
        if (CheckNullCondition()) { return; }
        if (useCameraFrustrumAsMaxDistance) { maxDistance = Camera.main.farClipPlane; }
        GetAllColliderPoints();
        for (int i = 0; i < POSITION_LENGTH; i++) {
            var objPos = projectedPositions[i];
            ray = new Ray(Camera.main.transform.position, objPos - Camera.main.transform.position);
            raycastResults[i] = Physics.Raycast(ray, out var hitData, maxDistance, layerToCast, hitTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore);
        }
        if (!System.Array.Exists(raycastResults, b => b == true)) {
            if (!isVisible) {
                isVisible = true;
                onObjectVisible?.Invoke();
            }
        } else {
            if (isVisible) {
                isVisible = false;
                onObjectNotVisible?.Invoke();
            }
        }
    }

    void GetAllColliderPoints() {
        if (CheckNullCondition()) { return; }
        var center = Vector3.zero;
        var size = Vector3.one;
        var extents = size * 0.5f;
        projectedPositions[0] = center;
        projectedPositions[1] = center + new Vector3(extents.x, extents.y, extents.z);
        projectedPositions[2] = center + new Vector3(extents.x, extents.y, -extents.z);
        projectedPositions[3] = center + new Vector3(extents.x, -extents.y, extents.z);
        projectedPositions[4] = center + new Vector3(extents.x, -extents.y, -extents.z);
        projectedPositions[5] = center + new Vector3(-extents.x, extents.y, extents.z);
        projectedPositions[6] = center + new Vector3(-extents.x, extents.y, -extents.z);
        projectedPositions[7] = center + new Vector3(-extents.x, -extents.y, extents.z);
        projectedPositions[8] = center + new Vector3(-extents.x, -extents.y, -extents.z);
        indexClosestToCamera = -1;
        var closestDistance = Mathf.Infinity;
        for (int i = 0; i < POSITION_LENGTH; i++) {
            projectedPositions[i] = objectToCheck.transform.TransformPoint(projectedPositions[i]);
            var distance = Vector3.Distance(projectedPositions[i], Camera.main.transform.position);
            if (distance < closestDistance) { 
                indexClosestToCamera = i;
                closestDistance = distance;
            }            
        }
        if (indexClosestToCamera < 0) { return; }
        for (int i = 0; i < POSITION_LENGTH; i++) {
            if (debugProjection && unProjectedPositions != null && unProjectedPositions.Length == POSITION_LENGTH) { unProjectedPositions[i] = projectedPositions[i]; }
            projectedPositions[i] = ProjectPointOnPlane(Camera.main.transform.forward, projectedPositions[indexClosestToCamera], projectedPositions[i]);
        }

    }

    Vector3 ProjectPointOnPlane(Vector3 planeNormal, Vector3 planePoint, Vector3 point) {
         var distance = -Vector3.Dot(planeNormal.normalized, point - planePoint);
         return point + planeNormal * distance;
    }

    bool CheckNullCondition() {
        if (projectedPositions == null || projectedPositions.Length < POSITION_LENGTH) { return true; }
        if (raycastResults == null || raycastResults.Length < POSITION_LENGTH) { return true; }
        if (debugProjection && (unProjectedPositions == null || unProjectedPositions.Length < POSITION_LENGTH)) { return true; }
        if (Camera.main == null) { return true; }
        return false;
    }

    void OnDrawGizmos() {        
        if (CheckNullCondition()) { return; }
        for (int i = 0; i < POSITION_LENGTH; i++) {
            Gizmos.color = projectedSphereColor;
            Gizmos.DrawSphere(projectedPositions[i], sphereSize);
            Gizmos.color = raycastResults[i] ? Color.red : Color.green;
            var ray = new Ray(Camera.main.transform.position, projectedPositions[i] - Camera.main.transform.position);
            Gizmos.DrawRay(ray.origin, ray.direction * maxDistance);
            if (debugProjection) {
                Gizmos.color = projectionLineColor;
                Gizmos.DrawLine(projectedPositions[i], unProjectedPositions[i]);
                Gizmos.color = unProjectedSphereColor;
                Gizmos.DrawSphere(unProjectedPositions[i], sphereSize);
            }
        }
    }
}
