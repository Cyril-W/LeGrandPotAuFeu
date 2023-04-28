using UnityEngine;
using UnityEngine.Events;

public class EventSender : MonoBehaviour {
    [SerializeField] UnityEvent OnStartEvent;
    [SerializeField] UnityEvent OnEnableEvent;
    [SerializeField] UnityEvent OnDisableEvent;
    [Space]
    [SerializeField] UnityEvent EventToSend;

    void Start() {
        OnStartEvent?.Invoke();
    }

    void OnEnable() {
        OnEnableEvent?.Invoke();
    }

    void OnDisable() {
        OnDisableEvent?.Invoke();
    }

    [ContextMenu("Send Event")]
    public void SendEvent() {
        EventToSend?.Invoke();
    }
}
