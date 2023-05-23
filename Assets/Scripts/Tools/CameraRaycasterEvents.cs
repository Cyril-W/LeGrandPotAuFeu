using UnityEngine;
using UnityEngine.Events;

public class CameraRaycasterEvents : MonoBehaviour {
    [SerializeField] bool raycastOnlyOnClick = true;
    [SerializeField] LayerMask layerToCast;
    [SerializeField, TagSelector] string tagToCheck;
    [SerializeField] float maxDistance = 1000f;
    [SerializeField] bool useCameraFrustrumAsMaxDistance = true;
    [SerializeField] bool hitTriggers = false;
    [Space]
    [SerializeField] UnityEvent<Vector3> onHitPosition;
    [SerializeField] UnityEvent onRaycastHit;
    [SerializeField] UnityEvent onRaycastHitClickInside;
    [SerializeField] UnityEvent onRaycastMissed;
    [Header("Gizmos")]
    [SerializeField] bool debugRay = false;
    [SerializeField] Color rayHitColor = Color.green;
    [SerializeField] Color rayMissColor = Color.red;
    [SerializeField] Color hitSpherecolor = Color.grey;
    [SerializeField] float hitSphereSize = 0.1f;

    Vector3 screenPosition, hitPosition;
    Ray ray;
    bool isHitting = false, lastInputMouse;

    void FixedUpdate() {
        if (Camera.main == null) { return; }
        if (raycastOnlyOnClick && !Input.GetMouseButton(0)) {
            lastInputMouse = false;
            if (isHitting) {
                isHitting = false;
                onRaycastMissed?.Invoke();
            }
            return;
        }
        if (useCameraFrustrumAsMaxDistance) { maxDistance = Camera.main.farClipPlane; }
        screenPosition = Input.mousePosition;
        ray = Camera.main.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out var hitData, maxDistance, layerToCast, hitTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore)) {
            if (string.IsNullOrEmpty(tagToCheck) || (!string.IsNullOrEmpty(hitData.collider.tag) && hitData.collider.CompareTag(tagToCheck))) {
                hitPosition = hitData.point;
                onHitPosition?.Invoke(hitPosition);
                if (!isHitting) {
                    isHitting = true;
                    onRaycastHit?.Invoke();
                }
                if (!lastInputMouse) {
                    lastInputMouse = true;
                    onRaycastHitClickInside?.Invoke(); 
                }
            }
        } else {
            if (isHitting) {
                isHitting = false;
                onRaycastMissed?.Invoke();
            }
        }
    }

    void OnDrawGizmos() {
        if (debugRay) {
            Gizmos.color = isHitting ? rayHitColor : rayMissColor;
            Gizmos.DrawRay(ray.origin, ray.direction * maxDistance);
            if (isHitting) {
                Gizmos.color = hitSpherecolor;
                Gizmos.DrawSphere(hitPosition, hitSphereSize);
            }
        }
    }
}
