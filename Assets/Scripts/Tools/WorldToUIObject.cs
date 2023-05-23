using UnityEngine;

public class WorldToUIObject : MonoBehaviour {
    [SerializeField, ReadOnly] bool isRegistered = false;

    void OnEnable() {
        Register();
    }

    void FixedUpdate() {
        if (!isRegistered) {
            Register();
        }
    }

    void OnDisable() {
        if (WorldToUIManager.Instance != null) { 
            WorldToUIManager.Instance.UnregisterToUI(transform);
            isRegistered = false;
        }
    }

    void Register() {
        if (WorldToUIManager.Instance != null) { 
            WorldToUIManager.Instance.RegisterToUI(transform);
            isRegistered = true;
        }
    }
}
