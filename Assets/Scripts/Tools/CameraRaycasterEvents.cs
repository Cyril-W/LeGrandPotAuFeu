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

    Vector3 screenPosition;
    Ray ray;
    bool isHitting = false;

    void FixedUpdate() {
        if (Camera.main == null) { return; }
        if (raycastOnlyOnClick && !Input.GetMouseButton(0)) {
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
                onHitPosition?.Invoke(hitData.point);
                if (!isHitting) {
                    isHitting = true;
                    onRaycastHit?.Invoke();
                }
                if (Input.GetMouseButtonDown(0)) {
                    Debug.Log("Clicked inside");
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
}
