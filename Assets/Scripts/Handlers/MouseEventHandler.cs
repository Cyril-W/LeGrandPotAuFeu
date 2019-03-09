using UnityEngine;
using UnityEngine.Events;

public class MouseEventHandler : MonoBehaviour {
    [SerializeField] UnityEvent onMouseUp;
    [SerializeField] UnityEvent onMouseDown;
    [SerializeField] UnityEvent onMouseEnter;
    [SerializeField] UnityEvent onMouseExit;

    private void OnMouseUp() {
        onMouseUp.Invoke();
    }

    private void OnMouseDown() {
        onMouseDown.Invoke();
    }

    private void OnMouseEnter() {
        onMouseEnter.Invoke();
    }

    private void OnMouseExit() {
        onMouseExit.Invoke();
    }
}
