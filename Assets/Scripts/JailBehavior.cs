using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class JailBehavior : MonoBehaviour {
    [SerializeField] bool isLocked;
    [SerializeField] float unlockTime = 1f;
    [SerializeField] GameObject unlockPanel;
    [SerializeField] Transform unlockProgressBarPivot;
    [SerializeField] GameObject lockInteractor;
    [SerializeField] GameObject openInteractor;
    [SerializeField] GameObject closeInteractor;
    [SerializeField] Transform jailPivot;
    [SerializeField] float rotationDuration = 1f;

    bool isOpened = false;
    float rotationY, currentUnlockTime = 0f;
    Vector3 progressLocalScale;

    void Start() {
        isOpened = false;
        rotationY = jailPivot.rotation.eulerAngles.y;
        unlockPanel.SetActive(false);
        UpdateInteractors();
    }

    void FixedUpdate() {
        progressLocalScale = unlockProgressBarPivot.localScale;
        progressLocalScale.x = 1f - Mathf.Clamp01(currentUnlockTime / unlockTime);
        unlockProgressBarPivot.localScale = progressLocalScale;
        if (currentUnlockTime > 0f) {
            currentUnlockTime -= Time.deltaTime;
            if (currentUnlockTime <= 0f) { 
                isLocked = false;
                unlockPanel.SetActive(false);
                OpenJail(); 
            }
        }
    }

    public void Unlock(bool isUnlocking) {
        unlockPanel.SetActive(isUnlocking);
        currentUnlockTime = isUnlocking ? unlockTime : 0f;
    }

    public void OpenJail() {
        if (isOpened) return;
        isOpened = true;
        UpdateInteractors();
        jailPivot.DORotate(Vector3.up * (rotationY - 90f), rotationDuration);
    }

    public void CloseJail() {
        if(!isOpened) return;
        isOpened = false;
        UpdateInteractors();
        jailPivot.DORotate(Vector3.up * rotationY, rotationDuration);
    }

    void UpdateInteractors() {
        lockInteractor.SetActive(isLocked);
        openInteractor.SetActive(!isLocked && !isOpened);
        closeInteractor.SetActive(!isLocked && isOpened);
    }
}
