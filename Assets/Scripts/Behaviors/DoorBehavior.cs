using UnityEngine;
using UnityEngine.Events;

public class DoorBehavior : MonoBehaviour {
    [SerializeField] float interactTime = 1f;
    [SerializeField] float offsetForward = 1.5f;
    [SerializeField] GameObject interactor;
    [SerializeField] UnityEvent onInteracted;

    float currentInteractTime;
    bool isInToOut = false;

    void FixedUpdate() {
        if (LevelManager.Instance != null && LevelManager.Instance.IsPaused) { return; }
        if (DestinyManager.Instance != null && DestinyManager.Instance.AnyTrackingGuard()) { 
            currentInteractTime = 0f;
            SetImageFill(0f);
            interactor.SetActive(false);
            return;
        }
        if (currentInteractTime > 0f) {
            currentInteractTime -= Time.deltaTime;
            if (currentInteractTime <= 0f) {
                SetImageFill(0f);
                interactor.SetActive(false);
                if (ClothTearingBehavior.Instance != null) { ClothTearingBehavior.Instance.doorBehavior = this; }
                onInteracted?.Invoke();
            } else {
                SetImageFill(1f - Mathf.Clamp01(currentInteractTime / interactTime));
            }
        }
    }

    void SetImageFill(float newImagefill) {
        if (WorldToUIManager.Instance != null) { WorldToUIManager.Instance.SetImageFill(newImagefill); }
    }

    public void SetIsInToOut(bool b) {
        isInToOut = b;
    }

    public void InteractAbort() {
        interactor.SetActive(true);
        Interact(false);
    }

    public void InteractSuccess() {
        var newPos = transform.position + transform.forward * offsetForward * (isInToOut ? -1f : 1f);
        newPos.y = 0;
        GroupManager.Instance.SetGroupPosition(newPos);
        gameObject.SetActive(false);
    }

    public void Interact(bool isInteracting) {
        if (DestinyManager.Instance == null || DestinyManager.Instance.AnyTrackingGuard()) { return; }
        currentInteractTime = isInteracting ? interactTime : 0f;
        if (WorldToUIManager.Instance != null) { WorldToUIManager.Instance.SetImageFill(0f); }
    }
}
