using UnityEngine;
using UnityEngine.Events;

public class BoolEvent : MonoBehaviour {
    [SerializeField] UnityEvent onTrueEvent;
    [SerializeField] UnityEvent onFalseEvent;

    public void SendOnTrueEvent() {
        onTrueEvent?.Invoke();
    }

    public void SendOnFalseEvent() {
        onFalseEvent?.Invoke();
    }

    public void SendOnBoolEvent(bool b) {
        if (b) {
            SendOnTrueEvent();
        } else {
            SendOnFalseEvent();
        }
    }
}
