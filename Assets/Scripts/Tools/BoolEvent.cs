using UnityEngine;
using UnityEngine.Events;

public class BoolEvent : MonoBehaviour {
    [SerializeField, ReadOnly] bool lastBoolStored = false;
    [SerializeField] UnityEvent onTrueEvent;
    [SerializeField] UnityEvent onFalseEvent;

    public void SetAndSendBoolStored(bool newBool) {
        SetBoolStored(newBool);
        SendOnBoolEvent(lastBoolStored);
    }

    public void SetBoolStored(bool newBool) {
        lastBoolStored = newBool;
    }

    public void SendOnTrueEvent() {
        onTrueEvent?.Invoke();
    }

    public void SendOnFalseEvent() {
        onFalseEvent?.Invoke();
    }

    public void SendOnBoolEvent() {
        SendOnBoolEvent(lastBoolStored);
    }

    public void SendOnBoolEvent(bool b) {
        if (b) {
            SendOnTrueEvent();
        } else {
            SendOnFalseEvent();
        }
    }
}
