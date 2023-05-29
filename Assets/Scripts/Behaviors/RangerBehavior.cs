using UnityEngine;

public class RangerBehavior : HeroBehavior {
    [SerializeField] SpriteRenderer spriteArrowDirection;
    [SerializeField] Transform pivotArrowDirection;
    [SerializeField] float arrowShowTime = 2f;
    [SerializeField] AnimationCurve arrowAlphaCurve;
    [SerializeField] Vector2 arrowAlphaOffOn = new Vector2(0f, 0.15f);

    float currentShowingTime = 0f;
    Color currentColor;

    void OnValidate() {
        TryFillNull();
    }

    void Start() {
        TryFillNull();
        if (spriteArrowDirection != null) { 
            currentColor = spriteArrowDirection.color;
            currentColor.a = arrowAlphaOffOn.x;
            spriteArrowDirection.color = currentColor;
        }
    }

    void TryFillNull() {
        if (spriteArrowDirection == null) {
            spriteArrowDirection = GetComponentInChildren<SpriteRenderer>();
        }
        if (pivotArrowDirection == null && spriteArrowDirection != null) {
            pivotArrowDirection = spriteArrowDirection.transform.parent;
        }
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();
        if (spriteArrowDirection == null) { return; }
        if (currentShowingTime > 0f) {
            currentShowingTime -= Time.deltaTime;
            currentColor.a = Mathf.Lerp(arrowAlphaOffOn.x, arrowAlphaOffOn.y, arrowAlphaCurve.Evaluate(1f - Mathf.Clamp01(currentShowingTime / arrowShowTime)));
            spriteArrowDirection.color = currentColor;
            if (currentShowingTime <= 0f) {
                spriteArrowDirection.enabled = false;
                SetThirdPersonControllerEnabled(true);
            }
        }
    }

    protected override bool OverrideDoSpell() {
        return Track();
    }

    void SetThirdPersonControllerEnabled(bool b) {
        if (GroupManager.Instance == null || GroupManager.Instance.GetThirdPersonController() == null) { return; }
        GroupManager.Instance.GetThirdPersonController().enabled = b;
    }

    [ContextMenu("Track")]
    bool Track() {
        if (GroupManager.Instance == null) { return false; }
        var playerPos = GroupManager.Instance.GetPlayerPosition();
        var heroPositions = GroupManager.Instance.GetUnsavedHeroPositions();
        var closestDistance = Mathf.Infinity;
        var closestPoint = Vector3.zero;
        foreach (var heroPos in heroPositions) {
            var newDistance = Vector3.Distance(playerPos, heroPos);
            if (newDistance < closestDistance) {
                closestDistance = newDistance;
                closestPoint = heroPos;
            }
        }        
        if (closestPoint.sqrMagnitude > 0f) {
            var direction = (closestPoint - playerPos).normalized;
            var angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            GroupManager.Instance.SetPlayerRotation(angle);
            SetThirdPersonControllerEnabled(false);
            if (spriteArrowDirection != null) {
                pivotArrowDirection.position = GroupManager.Instance.GetPlayerPosition();
                var rotation = pivotArrowDirection.rotation.eulerAngles;
                rotation.y = angle;
                pivotArrowDirection.rotation = Quaternion.Euler(rotation);
                spriteArrowDirection.enabled = true;
                currentShowingTime = arrowShowTime;
            }
            return true;
        } else { return false; }
    }
}
