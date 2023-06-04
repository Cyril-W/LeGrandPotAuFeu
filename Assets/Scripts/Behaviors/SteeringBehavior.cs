using UnityEngine;
using UnityEngine.Events;

public class SteeringBehavior : MonoBehaviour {
    [SerializeField] float timeBetweenUpdates = 0.1f;
    [SerializeField] Rigidbody steeringRb;
    [SerializeField] float hitStrength = 1f;
    [SerializeField] Vector2 minMaxTimeBetweenHits = new Vector2(0.5f, 1f);
    [SerializeField] float minMagnitudeToHit = 0.2f;
    [SerializeField] float speed = 6f;
    [SerializeField, Range(0f, 1f)] float crouchSpeedRatio = 0.5f;
    [SerializeField] Vector2 crouchStandScale = new Vector2(0.75f, 1f);
    [SerializeField] float turnSmoothTime = 0.1f;
    [SerializeField] Transform transformTarget;
    [SerializeField] float maxTargetRange = 10f;
    [SerializeField] Vector2 minMaxDistanceToTarget = new Vector2(0.25f, 0.5f);
    [SerializeField] Vector2 minMaxDistanceToObstacles = new Vector2(0.5f, 2f);
    [SerializeField] Color directionLineColor = Color.yellow;
    [SerializeField] Color goodLineColor = Color.green;
    [SerializeField] Color badLineColor = Color.red;
    [SerializeField] Color targetColor = Color.magenta;
    [SerializeField] float targetSphereSize = 0.1f;
    [SerializeField] bool showTargetRange = false;
    [SerializeField] bool showObstacleRange = false;
    [SerializeField] bool showTarget = false;
    [SerializeField] bool showObstacles = false;
    [SerializeField] float rayOffset = 0.25f;
    [SerializeField] float timeBetweenSteps = 0.4f;
    [SerializeField] UnityEvent onFootStep;

    Vector3[] directions = new Vector3[8] { 
        new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 0), new Vector3(1, 0, -1), 
        new Vector3(0, 0, -1), new Vector3(-1, 0, -1), new Vector3(-1, 0, 0), new Vector3(-1, 0, 1)
    };
    Vector3[] closestDirectionalCollision = new Vector3[8];
    Vector3 currentDirection, directionToTargetNormalized, directionToObstacleNormalized, lastTarget;
    Ray ray;
    bool[] displayClosestDirectionalCollision = new bool[8];
    float[] interests = new float[8];
    float[] dangers = new float[8];
    float[] results = new float[8];
    float currentTimeBetweenUpdate = 0f, currentTimeBetweenSteps = 0f, currentTimeBetweenHits = 0f;
    float turnSmoothVelocity, targetAngle, angle, collisionDistance, collisionDistanceRatio, targetDistanceRatio, targetDistance, lastRaycastMaxDistance;
    bool isCrouched = false, hasHit = true, lastRaycast = false;

    void OnValidate() {
        TryFillNull();
    }

    void Start() {
        currentTimeBetweenUpdate = timeBetweenUpdates;
        TryFillNull();
    }

    void OnEnable() {
        TryFillNull();
        ResetPosition();
    }

    public void ResetPosition() {
        if (steeringRb != null) {
            steeringRb.MovePosition(transformTarget.position);
        }
        CheckTarget();
    }

    void CheckTarget() {
        if (transformTarget == null || steeringRb == null) { return; }
        if (Vector3.Distance(steeringRb.position, transformTarget.position) > maxTargetRange) { return; }
        if (SteeringManager.Instance != null) {
            ray = new Ray(steeringRb.position + Vector3.up * 0.5f, transformTarget.position - steeringRb.position);
            lastRaycastMaxDistance = Vector3.Distance(transformTarget.position, steeringRb.position);
            lastRaycast = SteeringManager.Instance.RaycastHitAnyObstacle(ray, lastRaycastMaxDistance);
            if (lastRaycast) { return; }
        }
        lastTarget = transformTarget.position;
        
    }

    void TryFillNull() {
        directions = new Vector3[8] {
            new Vector3(0, 0, 1), new Vector3(1, 0, 1).normalized, new Vector3(1, 0, 0), new Vector3(1, 0, -1).normalized,
            new Vector3(0, 0, -1), new Vector3(-1, 0, -1).normalized, new Vector3(-1, 0, 0), new Vector3(-1, 0, 1).normalized
        };
        if (steeringRb == null) { steeringRb = GetComponent<Rigidbody>(); }
    }

    void FixedUpdate() {
        if (currentTimeBetweenUpdate > 0f) {
            currentTimeBetweenUpdate -= Time.deltaTime;
            if (currentTimeBetweenUpdate <= 0f) {
                CheckTarget();
                CheckDirections();               
                currentTimeBetweenUpdate = timeBetweenUpdates;
            }
        }
        Move();
        if (currentDirection.magnitude >= minMagnitudeToHit) {
            hasHit = false;
            if (currentTimeBetweenSteps > 0) {
                currentTimeBetweenSteps -= Time.deltaTime;
                if (currentTimeBetweenSteps <= 0) {
                    currentTimeBetweenSteps = timeBetweenSteps / (isCrouched ? crouchSpeedRatio : 1f);
                    onFootStep?.Invoke();
                }
            }
        } else if (!hasHit) {
            if (steeringRb != null && GroupManager.Instance != null) { GroupManager.Instance.HitGroup(steeringRb.position); }                
            hasHit = true;
        } 
        if (currentTimeBetweenHits > 0f) { currentTimeBetweenHits -= Time.deltaTime; }        
    }

    public Vector3 GetPosition() {
        return steeringRb != null ? steeringRb.position : transform.position;
    }

    void CheckDirections() {
        if (steeringRb == null) { return; }
        directionToTargetNormalized = (lastTarget - steeringRb.position).normalized;
        currentDirection = Vector3.zero;
        targetDistance = Vector3.Distance(lastTarget, steeringRb.position);
        targetDistanceRatio = targetDistance >= minMaxDistanceToTarget.y ? 1 : Mathf.Clamp01((targetDistance - minMaxDistanceToTarget.x) / (minMaxDistanceToTarget.y - minMaxDistanceToTarget.x)); // 0 = between 0 and min, 0-1 = between min and max, 1 = beyond max
        Vector3 closestCollision;
        for (int i = 0; i < directions.Length; i++) {
            interests[i] = Mathf.Clamp01(Vector3.Dot(directionToTargetNormalized, directions[i]) * targetDistanceRatio); // 0 = side or back or too close, 1 = right in front
            if (SteeringManager.Instance != null) {                
                if (SteeringManager.Instance.GetClosestCollision(this, steeringRb.position, directions[i], minMaxDistanceToObstacles.y, out closestCollision)) {
                    closestDirectionalCollision[i] = closestCollision;
                    displayClosestDirectionalCollision[i] = true;
                    collisionDistance = Vector3.Distance(closestCollision, steeringRb.position);
                    collisionDistanceRatio = collisionDistance <= minMaxDistanceToObstacles.x ? 1 : Mathf.Clamp01((minMaxDistanceToObstacles.y - collisionDistance) / minMaxDistanceToObstacles.y); // 1 = too close, 0 = too far away, 1-0 = between min and max
                    directionToObstacleNormalized = (closestCollision - steeringRb.position).normalized;
                    //var shapedDot = 1f - Mathf.Abs(Vector3.Dot(directionToObstacleNormalized, directions[i]) - 0.65f); // Removes the front and favors the side
                    var shapedDot = Vector3.Dot(directionToObstacleNormalized, directions[i]);
                    dangers[i] = Mathf.Clamp01(shapedDot * collisionDistanceRatio); // 0 = too far or side or back, 1 = in front or too close
                } else {
                    dangers[i] = 0f;
                    displayClosestDirectionalCollision[i] = false;
                }
            } else {
                dangers[i] = 0f;
            }
            results[i] = interests[i] - dangers[i];
            //currentDirection += Mathf.Clamp01(results[i]) * directions[i];
            currentDirection += results[i] * directions[i];
        }
        currentDirection /= directions.Length;
        //currentDirection.Normalize();
    }

    void Move() {
        if (steeringRb == null /*|| currentDirection.magnitude < 0.05f*/) { return; }
        steeringRb.MovePosition(steeringRb.position + currentDirection * speed * (isCrouched ? crouchSpeedRatio : 1f) * Time.deltaTime);
        if (currentDirection.magnitude < 0.05f) { return; }
        targetAngle = Mathf.Atan2(currentDirection.x, currentDirection.z) * Mathf.Rad2Deg;
        angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        steeringRb.MoveRotation(Quaternion.Euler(0f, angle, 0f));
    }

    public void SetIsCrouched(bool newIsCrouched) {
        isCrouched = newIsCrouched;
        transform.localScale = new Vector3(crouchStandScale.y, isCrouched ? crouchStandScale.x : crouchStandScale.y, crouchStandScale.y);
    }

    public void GetHit(Vector3 position) {
        if (steeringRb == null || Vector3.Distance(steeringRb.position, position) <= 0.1f || currentTimeBetweenHits > 0) { return; }
        currentTimeBetweenHits = Random.Range(minMaxTimeBetweenHits.x, minMaxTimeBetweenHits.y);
        var direction = (steeringRb.position - position).normalized;
        steeringRb.AddForce(direction * hitStrength, ForceMode.Impulse);
    }

    void OnDrawGizmos() {
        if (steeringRb == null) { return; }
        //Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = directionLineColor;
        //Gizmos.DrawLine(Vector3.zero, currentDirection);
        Gizmos.DrawRay(steeringRb.position, (currentDirection.normalized * rayOffset) + currentDirection);
        Gizmos.color = Color.grey;
        Gizmos.DrawWireSphere(steeringRb.position, rayOffset);
        Gizmos.DrawWireSphere(steeringRb.position, rayOffset * 2);
        for (int i = 0; i < directions.Length; i++) {
            Gizmos.color = results[i] >= 0 ? goodLineColor :  badLineColor;
            //Gizmos.DrawLine(Vector3.zero, directions[i] + Mathf.Clamp01(results[i]) * directions[i]);
            Gizmos.DrawRay(steeringRb.position, (rayOffset + rayOffset * results[i]) * directions[i]);
        }
    }

    void OnDrawGizmosSelected() {
        if (steeringRb == null || transformTarget == null) { return; }
        if (showTargetRange) {
            Gizmos.color = targetColor;
            Gizmos.DrawWireSphere(steeringRb.position, maxTargetRange);
            Gizmos.DrawSphere(lastTarget, targetSphereSize);
            if (lastRaycast) {
                Gizmos.color = badLineColor;
            }            
            Gizmos.DrawRay(ray.origin, ray.direction * lastRaycastMaxDistance);
        }
        if (showTarget) {
            Gizmos.color = goodLineColor;
            Gizmos.DrawSphere(transformTarget.position, minMaxDistanceToTarget.x);
            Gizmos.DrawWireSphere(transformTarget.position, minMaxDistanceToTarget.y);
        }
        if (showObstacleRange) {
            Gizmos.color = directionLineColor;
            Gizmos.DrawWireSphere(steeringRb.position, minMaxDistanceToObstacles.y);
        }
        if (showObstacles) {
            Gizmos.color = badLineColor;
            for (int i = 0; i < closestDirectionalCollision.Length; i++) {
                if (displayClosestDirectionalCollision[i]) {
                    Gizmos.DrawSphere(closestDirectionalCollision[i], minMaxDistanceToObstacles.x);
                    Gizmos.DrawWireSphere(closestDirectionalCollision[i], minMaxDistanceToObstacles.y);
                }
            }
        }
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(steeringRb.position + directions[0] * rayOffset * 2, directions[0] * 0.25f);
    }
}
