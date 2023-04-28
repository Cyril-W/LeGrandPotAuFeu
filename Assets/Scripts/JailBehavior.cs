using UnityEngine;
using DG.Tweening;

public class JailBehavior : MonoBehaviour {
    [SerializeField] GameObject openInteractor;
    [SerializeField] GameObject closeInteractor;
    [SerializeField] Transform jailPivot;
    [SerializeField] float rotationDuration = 1f;

    bool isOpened = false;

    void Start() {
        isOpened = false;
        UpdateInteractors();
    }

    public void OpenJail() {
        if (isOpened) return;
        isOpened = true;
        UpdateInteractors();
        jailPivot.DORotate(Vector3.up * -90f, rotationDuration);
    }

    public void CloseJail() {
        if(!isOpened) return;
        isOpened = false;
        UpdateInteractors();
        jailPivot.DORotate(Vector3.zero, rotationDuration);
    }

    void UpdateInteractors() {
        openInteractor.SetActive(!isOpened);
        closeInteractor.SetActive(isOpened);
    }
}
