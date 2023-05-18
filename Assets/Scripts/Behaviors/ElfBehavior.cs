using UnityEngine;
using Cinemachine;

public class ElfBehavior : HeroBehavior {
    [SerializeField] CinemachineVirtualCamera virtualCamera;
    [SerializeField] Vector2 minMaxLens = new Vector2(40f, 60f);
    [SerializeField] float visionChangeDuration = 1f;
    [SerializeField] float visionDuration = 5f;

    float currentVisionTime = 0f, currentVisionChangeTime = 0f;
    void OnValidate() {
        TryFillNull();
    }

    void Start() {
        TryFillNull();
    }

    void TryFillNull() {
        if (virtualCamera == null) virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
    }

    void FixedUpdate() {
        if (currentVisionChangeTime > 0f) {
            currentVisionChangeTime -= Time.deltaTime;
            virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(currentVisionTime <= 0f ? minMaxLens.x : minMaxLens.y, currentVisionTime <= 0f ? minMaxLens.y : minMaxLens.x, currentVisionChangeTime / visionChangeDuration);
        }
        if (currentVisionTime > 0f) {
            currentVisionTime -= Time.deltaTime;
            if (currentVisionTime <= 0f) {
                currentVisionChangeTime = visionChangeDuration;
            }
        }
    }

    protected override void OverrideDoSpell() {
        Vision();
    }

    [ContextMenu("Vision")]
    void Vision() {
        currentVisionTime = visionDuration;
        currentVisionChangeTime = visionChangeDuration;
    }
}
