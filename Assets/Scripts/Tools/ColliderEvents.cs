using UnityEngine;
using UnityEngine.Events;

public class ColliderEvents : MonoBehaviour {
    [Header("Parameters")]
    [SerializeField, TagSelector] string ColliderTag;
    [SerializeField] Color BoxWireColor = Color.green;
    [SerializeField] Color BoxColor = Color.clear;
    [Header("Events")]
    [SerializeField] bool sendTriggerExitOnDisable = true;
    [SerializeField] UnityEvent<Collider> OnTriggerEnterEvent;
    [SerializeField] UnityEvent<Collider> OnTriggerStayEvent;
    [SerializeField] UnityEvent<Collider> OnTriggerExitEvent;

    [SerializeField, ReadOnly] Collider lastCollider;

    void OnDisable() {
        if (sendTriggerExitOnDisable && lastCollider != null) {
            lastCollider = null;
            OnTriggerExitEvent?.Invoke(lastCollider);
        }
    }

    bool CheckCollisionTag(Collider collider) {
        if (ColliderTag.Length <= 0) {
            return true;
        }
        return collider.gameObject.CompareTag(ColliderTag);
    }

    void OnTriggerEnter(Collider collider) {
        if (CheckCollisionTag(collider)) {
            lastCollider = collider;
            OnTriggerEnterEvent?.Invoke(collider);
        }
    }

    void OnTriggerExit(Collider collider) {
        if (CheckCollisionTag(collider)) {
            if (lastCollider == collider) {
                lastCollider = null;
            }
            OnTriggerExitEvent?.Invoke(collider);
        }
    }

    void OnTriggerStay(Collider collider) {
        if (CheckCollisionTag(collider)) {
            OnTriggerStayEvent?.Invoke(collider);
        }
    }

    void OnDrawGizmos() {
        Gizmos.matrix = this.transform.localToWorldMatrix;
        Gizmos.color = BoxWireColor;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        Gizmos.color = BoxColor;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
    }
}
