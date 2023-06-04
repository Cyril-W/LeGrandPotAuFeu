using System.Collections.Generic;
using UnityEngine;

public class SteeringManager : MonoBehaviour {
    public static SteeringManager Instance { get; private set; }

    [SerializeField] SteeringBehavior[] steeringBehaviors;
    [SerializeField, Layer] int obstacleLayer = 6;
    [SerializeField] Collider[] collidersToAvoid;

    void Awake() {
        if (Instance == null || Instance != this) { Instance = this; }
    }

    void Start() {
        SetSteeringBehaviors();
        SetCollidersToAvoid();
    }

    [ContextMenu("Set Steering Behaviors")]
    void SetSteeringBehaviors() {
        steeringBehaviors = GetComponentsInChildren<SteeringBehavior>(true);
    }

    [ContextMenu("Set Colliders to avoid")]
    void SetCollidersToAvoid() {
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

    public bool CollideWithAny(Vector3 position, float distance) {
        foreach (var coll in collidersToAvoid) {
            if (coll.bounds.Contains(position) || Vector3.Distance(coll.ClosestPointOnBounds(position), position) <= distance) {
                return true;
            }
        }
        if (collidersToAvoid.Length <= 0) { Debug.LogWarning("No colliders to avoid found"); }
        return false;
    }

    Vector3[] GetActiveSteeringBehaviorsPosition() {
        var positions = new List<Vector3>();
        foreach (var sB in steeringBehaviors) {
            if (sB.gameObject.activeSelf && sB.enabled) {
                positions.Add(sB.GetPosition());
            } else {
                positions.Add(new Vector3(0f, Mathf.Infinity, 0f));
            }
        }
        return positions.ToArray();
    }

    public void ResetPositions() {
        foreach (var sB in steeringBehaviors) {
            sB.ResetPosition();
        }
    }

    public bool GetClosestCollision(SteeringBehavior sB, Vector3 position, Vector3 direction, float maxDistance, out Vector3 collision) {
        collision = position;

        /*int layerMask = 1 << obstacleLayer;
        RaycastHit hit;
        if (Physics.Raycast(position, direction, out hit, maxDistance, layerMask)) {
            return hit.point;
        }*/

        // check for other steering behaviors
        var positions = GetActiveSteeringBehaviorsPosition();
        var closestDistance = Mathf.Infinity;
        var highestDot = 0f;
        var currentId = -1;
        for (int i = 0; i < positions.Length; i++) {
            if (i == System.Array.IndexOf(steeringBehaviors, sB)) { continue; }
            var sBPos = positions[i];
            var currentDistance = Vector3.Distance(position, sBPos);
            if (currentDistance <= maxDistance) {
                var currentDirectionNormalized = (sBPos - position).normalized;
                var currentDot = Vector3.Dot(currentDirectionNormalized, direction);
                if (currentDot >= 0 && currentDot > highestDot && currentDistance < closestDistance) {
                    highestDot = currentDot;
                    closestDistance = currentDistance;
                    currentId = i;
                }
            }
        }
        if (currentId >= 0) {
            collision = positions[currentId];
            return true;
        }

        // check for obstacles
        // get closest point of bounds, direction = this point - transform

        return false;
    }
}
