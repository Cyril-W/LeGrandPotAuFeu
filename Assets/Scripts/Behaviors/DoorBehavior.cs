using UnityEngine;
using UnityEngine.Events;

public class DoorBehavior : MonoBehaviour {
    [SerializeField] float interactTime = 1f;
    [SerializeField] float offsetForward = 1.5f;
    [SerializeField] GameObject interactor;
    [SerializeField] UnityEvent onInteracted;

    float currentInteractTime;

    void FixedUpdate() {
        if (currentInteractTime > 0f) {
            currentInteractTime -= Time.deltaTime;
            if (currentInteractTime <= 0f) {
                if (WorldToUIManager.Instance != null) { WorldToUIManager.Instance.SetImageFill(0f); }
                interactor.SetActive(false);
                ClothTearingBehavior.Instance.doorBehavior = this;
                onInteracted?.Invoke();
            } else {
                if (WorldToUIManager.Instance != null) { WorldToUIManager.Instance.SetImageFill(1f - Mathf.Clamp01(currentInteractTime / interactTime)); }
            }
        }
    }

    public void InteractAbort() {
        interactor.SetActive(true);
        Interact(false);
    }

    public void InteractSuccess() {
        var newPos = transform.position + transform.forward * offsetForward;
        newPos.y = 0;
        GroupManager.Instance.MovePlayerPosition(newPos);
        gameObject.SetActive(false);
    }

    public void Interact(bool isInteracting) {
        if (DestinyManager.Instance.AnyTrackingGuard()) { return; }
        currentInteractTime = isInteracting ? interactTime : 0f;
        if (WorldToUIManager.Instance != null) { WorldToUIManager.Instance.SetImageFill(0f); }
    }
}
