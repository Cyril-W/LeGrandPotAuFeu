using UnityEngine;
using UnityEngine.Events;

public class SteeringBehavior : MonoBehaviour {
    [SerializeField] float timeBetweenUpdates = 0.1f;
    [SerializeField] Rigidbody steeringRb;
    [SerializeField] float hitStrength = 1f;
    [SerializeField] Vector2 minMaxTimeBetweenHits = new Vector2(1.5f, 2.5f);
    [SerializeField] float speed = 6f;
    [SerializeField, Range(0f, 1f)] float crouchSpeedRatio = 0.5f;
    [SerializeField] Vector2 crouchStandScale = new Vector2(0.75f, 1f);
    [SerializeField] float turnSmoothTime = 0.1f;
    [SerializeField] Transform transformTarget;
    [SerializeField] float minDistanceToTarget = 0.5f;
    [SerializeField] float detectionRadius = 1f;
    [SerializeField] Color directionLineColor = Color.yellow;
    [SerializeField] Color goodLineColor = Color.green;
    [SerializeField] Color badLineColor = Color.red;
    [SerializeField] float timeBetweenSteps = 0.4f;
    [SerializeField] UnityEvent onFootStep;

    Vector3[] directions = new Vector3[8] { 
        new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 0), new Vector3(1, 0, -1), 
        new Vector3(0, 0, -1), new Vector3(-1, 0, -1), new Vector3(-1, 0, 0), new Vector3(-1, 0, 1)
    };
    Vector3 currentDirection;
    float[] interests = new float[8];
    float[] dangers = new float[8];
    float[] results = new float[8];
    float currentTimeBetweenUpdate = 0f, currentTimeBetweenSteps = 0f, currentTimeBetweenHits = 0f, turnSmoothVelocity, targetAngle, angle, distanceRatio;
    bool isCrouched = false, hasHit = true;

    void OnValidate() {
        TryFillNull();
    }

    void Start() {
        currentTimeBetweenUpdate = timeBetweenUpdates;
        TryFillNull();
    }

    void OnEnable() {
        TryFillNull();
        if (steeringRb != null) {
            steeringRb.MovePosition(transformTarget.position);
        }
    }

    void TryFillNull() {
        directions = new Vector3[8] {
            new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 0), new Vector3(1, 0, -1),
            new Vector3(0, 0, -1), new Vector3(-1, 0, -1), new Vector3(-1, 0, 0), new Vector3(-1, 0, 1)
        };
        if (steeringRb == null) { steeringRb = GetComponent<Rigidbody>(); }
    }

    void FixedUpdate() {
        if (currentTimeBetweenUpdate > 0f) {
            currentTimeBetweenUpdate -= Time.deltaTime;
            if (currentTimeBetweenUpdate <= 0f) {
                CheckDirections();               
                currentTimeBetweenUpdate = timeBetweenUpdates;
            }
        }
        Move();
        if (currentDirection.magnitude >= 0.1f) {
            hasHit = false;
            currentTimeBetweenHits = 0f;
            if (currentTimeBetweenSteps > 0) {
                currentTimeBetweenSteps -= Time.deltaTime;
                if (currentTimeBetweenSteps <= 0) {
                    currentTimeBetweenSteps = timeBetweenSteps / (isCrouched ? crouchSpeedRatio : 1f);
                    onFootStep?.Invoke();
                }
            }
        } else {
            if (!hasHit && currentTimeBetweenHits <= 0) {
                if (steeringRb != null && GroupManager.Instance != null) { GroupManager.Instance.HitGroup(steeringRb.position); }
                currentTimeBetweenHits = Random.Range(minMaxTimeBetweenHits.x, minMaxTimeBetweenHits.y);
                hasHit = true;
            } else {
                currentTimeBetweenHits -= Time.deltaTime;
            }
        }
    }

    void CheckDirections() {
        var directionToTargetNormalized = (transformTarget.position - transform.position).normalized;
        currentDirection = Vector3.zero;
        distanceRatio = Mathf.Clamp01(Vector3.Distance(transformTarget.position, transform.position) / minDistanceToTarget);
        for (int i = 0; i < directions.Length; i++) {
            interests[i] = Mathf.Lerp(0, Vector3.Dot(directionToTargetNormalized, directions[i]), distanceRatio);
            //dangers[i] = ...
            results[i] = Mathf.Clamp01(interests[i] - dangers[i]);
            currentDirection += results[i] * directions[i];
        }
        currentDirection /= directions.Length;
    }

    void Move() {
        if (steeringRb == null || currentDirection.magnitude < 0.1f) { return; }
        targetAngle = Mathf.Atan2(currentDirection.x, currentDirection.z) * Mathf.Rad2Deg;
        angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        steeringRb.MoveRotation(Quaternion.Euler(0f, angle, 0f));
        steeringRb.MovePosition(steeringRb.position + currentDirection * speed * (isCrouched ? crouchSpeedRatio : 1f) * Time.deltaTime);
    }

    public void SetIsCrouched(bool newIsCrouched) {
        isCrouched = newIsCrouched;
        transform.localScale = new Vector3(crouchStandScale.y, isCrouched ? crouchStandScale.x : crouchStandScale.y, crouchStandScale.y);
    }

    public void GetHit(Vector3 position) {
        if (steeringRb == null || Vector3.Distance(steeringRb.position, position) <= 0.1f) { return; }
        var direction = (steeringRb.position - position).normalized;
        steeringRb.AddForce(direction * hitStrength, ForceMode.Impulse);
    }

    void OnDrawGizmos() {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = directionLineColor;
        Gizmos.DrawLine(Vector3.zero, currentDirection);
        for (int i = 0; i < directions.Length; i++) {
            Gizmos.color = Color.Lerp(badLineColor, goodLineColor, Mathf.Clamp01(results[i]));
            Gizmos.DrawLine(Vector3.zero, directions[i].normalized * detectionRadius);
        }
    }
}
