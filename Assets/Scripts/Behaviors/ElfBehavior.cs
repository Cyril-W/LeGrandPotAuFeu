using UnityEngine;
using Cinemachine;
using System.Linq;

public class ElfBehavior : HeroBehavior {
    [SerializeField] CinemachineVirtualCamera virtualCamera;
    [SerializeField] Vector2 minMaxLens = new Vector2(40f, 50f);
    [SerializeField] float visionChangeDuration = 0.5f;
    [SerializeField] float maxRange = 10f;
    [SerializeField, Range(0, 100)] float chancePercentage = 50f;
    [SerializeField] Vector2 minMaxXRandom = new Vector2(-3f, 3f);
    [SerializeField] Vector2 minMaxYRandom = new Vector2(-3f, 3f);
    [SerializeField] float minValidRandomDistance = 0.5f;
    [SerializeField] Transform arrowTarget;
    [SerializeField] TransformJumper arrowJumper;
    [SerializeField] SpellBehavior spellBehavior;
    [SerializeField] float coolDownOnMiss = 1f;
    [Header("Gizmos")]
    [SerializeField] Color sphereRightColor = Color.green;
    [SerializeField] Color sphereMiddleColor = Color.yellow;
    [SerializeField] Color sphereWrongColor = Color.red;
    [SerializeField] Color spherePositionColor = Color.magenta;

    float currentVisionChangeTime = 0f;
    bool lastPosFound = false, lastArrowHit = false;
    int lastGuardIndex = 0;
    Vector3 lastPos;

    void OnValidate() {
        TryFillNull();
    }

    void Start() {
        TryFillNull();
    }

    void TryFillNull() {
        if (virtualCamera == null) virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        if (spellBehavior == null) { spellBehavior = FindObjectsOfType<SpellBehavior>().Where(sB => sB.GetHero() == GetHero()).FirstOrDefault(); }
    }

    protected override void OnEnable() {
        base.OnEnable();
        TryFillNull();
        SetVision(true);
    }

    protected override void OnDisable() {
        base.OnDisable();
        SetVision(false);
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();
        if (currentVisionChangeTime > 0f) {
            currentVisionChangeTime -= Time.deltaTime;
            virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(minMaxLens.x, minMaxLens.y, 1f - Mathf.Clamp01(currentVisionChangeTime / visionChangeDuration));
        }
    }

    protected override void OverrideDoSpell() {
        Arrow();
    }

    void SetVision(bool isActivated) {
        if (isActivated) {
            currentVisionChangeTime = visionChangeDuration;
        } else {
            currentVisionChangeTime = 0f;
            virtualCamera.m_Lens.FieldOfView = minMaxLens.x;
        }
    }

    [ContextMenu("Arrow")]
    void Arrow() {
        if (GuardsManager.Instance == null || GroupManager.Instance == null) { return; }
        var playerPos = GroupManager.Instance.GetPlayerPosition();
        var positions = GuardsManager.Instance.GetGuardsPositions();
        var closestDistance = Mathf.Infinity;
        lastPos = playerPos;
        for (int i = 0; i < positions.Length; i++) {
            var pos = positions[i];
            var newDistance = Vector3.Distance(playerPos, pos);
            if (newDistance < closestDistance) {
                closestDistance = newDistance;
                lastPos = pos;
                lastGuardIndex = i;
            }
        }        
        if (closestDistance > 0f && closestDistance < maxRange) {
            lastPosFound = true;
            if (Random.Range(0f, 100f) <= chancePercentage) {
                lastArrowHit = true;
                arrowTarget.position = lastPos + Vector3.up;
                GuardsManager.Instance.ImmobilizeGuard(lastGuardIndex);
            } else {
                lastArrowHit = false;
                Vector3 newPos;
                int maxAttemps = 50;
                do {
                    newPos = lastPos + new Vector3(Random.Range(minMaxXRandom.x, minMaxXRandom.y), 0f, Random.Range(minMaxYRandom.x, minMaxYRandom.y));
                    maxAttemps--;
                } while (maxAttemps >= 0 && Vector3.Distance(newPos, lastPos) <= minValidRandomDistance);
                arrowTarget.position = newPos;
            }
            arrowJumper.Reset();
            arrowJumper.gameObject.SetActive(true);
            arrowJumper.Jump();
        } else {
            lastPosFound = false;
            spellBehavior.SetCurrentCooldown(coolDownOnMiss);
        }
    }

    public void TryKillLastGuard() {
        if (!lastPosFound || !lastArrowHit || GuardsManager.Instance == null) { return; }
        GuardsManager.Instance.DisableGuard(lastGuardIndex);
        arrowJumper.gameObject.SetActive(false);
    }

    void OnDrawGizmos() {
        if (GroupManager.Instance == null) { return; }
        Gizmos.color = lastPosFound ? (lastArrowHit ? sphereRightColor : sphereMiddleColor) : sphereWrongColor;
        Gizmos.DrawWireSphere(GroupManager.Instance.GetPlayerPosition(), maxRange);
        Gizmos.color = spherePositionColor;
        Gizmos.DrawSphere(lastPos, minValidRandomDistance);
    }
}
