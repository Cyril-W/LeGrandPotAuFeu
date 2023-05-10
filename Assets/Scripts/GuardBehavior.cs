using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GuardBehavior : MonoBehaviour {
    class ViewCastInfo {
        public bool Hit;
        public Vector3 Point;
        public float Distance;
        public float Angle;

        public ViewCastInfo(bool hit, Vector3 point, float distance, float angle) {
            Hit = hit;
            Point = point;
            Distance = distance;
            Angle = angle;
        }

        public ViewCastInfo() : this(false, Vector3.zero, 0f, 0f) { }
    }

    class EdgeInfo {
        public Vector3 PointA;
        public Vector3 PointB;

        public EdgeInfo(Vector3 pointA, Vector3 pointB) {
            PointA = pointA;
            PointB = pointB;
        }
    }

    [Header("Vision")]
    [Header("Parameters")]
    [SerializeField] bool canSee = true;
    [SerializeField] float visionAngle = 45f;
    [SerializeField] float visionLength = 1f;
    [SerializeField] float visionProximity = 0.2f;
    [SerializeField] LayerMask viewMask;
    [SerializeField] float detectionTime = 2f;
    [SerializeField] int edgeResolveIterations = 6;
    [SerializeField] float edgeDistanceThreshold = .5f;
    [SerializeField, Range(0f, 1f)] float meshResolution = .5f;
    [SerializeField] MeshFilter viewMeshFilter;
    //[SerializeField] Gradient visionGradient;
    [SerializeField] AudioSource audioSource;
    [SerializeField] Vector2 minMaxVolume = new Vector2(0f, 0.5f);
    [SerializeField] AnimationCurve volumeCurve;
    [Header("Movement")]
    [SerializeField] bool canMove = true;
    [SerializeField] float speed = 5f;
    [SerializeField, Range(0f, 1f)] float spottingSpeedRatio = 0.75f;
    [SerializeField] float timeAfterSpot = 2f;
    [SerializeField] float distanceToWaypoint = 0.1f;
    [SerializeField] bool moveUp = true;
    [SerializeField] Vector2 minMaxWaitTime = new Vector2(0.3f, 0.6f);
    [SerializeField] float turnSmoothTime = 0.1f;
    [SerializeField] Transform guardTransform;
    [SerializeField] bool updateWaypoints = false;
    [SerializeField] Transform waypointsHolder;
    [Header("Gizmos")]
    [SerializeField] float gizmosSphereSize = 0.3f;
    [SerializeField] bool showVisionGizmos = false;
    [SerializeField, Range(0, 100)] int rayNumber = 4;
    [SerializeField] Color targetLineColor = Color.cyan;
    [SerializeField] Color viewPointsSphereColor = Color.blue;
    [SerializeField] bool showMovementGizmos = false;
    [SerializeField] Color activeWaypointSphereColor = Color.red;
    [SerializeField] Color inactiveWaypointSphereColor = Color.gray;
    [SerializeField] Color waypointLineColor = Color.yellow;
    [SerializeField] UnityEvent<bool> OnPlayerSpotted;

    List<Vector3> viewPoints = new List<Vector3>();
    Vector3[] waypoints;
    Vector3 direction, target;
    Transform player;
    Rigidbody guardRigidbody;
    //MeshRenderer viewMeshRenderer;
    Mesh viewMesh;
    //Material viewMaterial;
    int currentWaypoint = 0;
    float currentWaitTime = 0f, turnSmoothVelocity, targetAngle, angle, currentDetectionTime, currentTimeAfterSpot = 0f, lastPuzzledRotation;
    bool playerSpotted = false, previousCanMove;

    void OnValidate() {
        previousCanMove = canMove;
    }

    void Start() {
        waypoints = new Vector3[waypointsHolder.childCount];
        PopulateWaypoints();
        currentWaitTime = Random.Range(minMaxWaitTime.x, minMaxWaitTime.y);
        currentDetectionTime = detectionTime;
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (guardRigidbody == null) guardRigidbody = guardTransform.GetComponent<Rigidbody>();
        if (viewMeshFilter == null) viewMeshFilter = GetComponent<MeshFilter>();
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        if (viewMeshFilter != null) viewMeshFilter.mesh = viewMesh;
        //viewMeshRenderer = viewMeshFilter.GetComponent<MeshRenderer>();
        //if (viewMeshRenderer != null) viewMaterial = viewMeshRenderer.material;
        previousCanMove = canMove;
        audioSource.enabled = false;
    }

    void FixedUpdate() {
        if (updateWaypoints) {
            PopulateWaypoints();
        }

        if (canSee) {
            VisionCheck();
        }
        /*if (viewMeshRenderer != null) {
            viewMeshRenderer.enabled = canSee;
        }*/

        if (currentTimeAfterSpot > 0f) {
            GuardPuzzled();
        }

        if (canMove) {
            MoveGuard();
        }
    }

    void LateUpdate() {
        DrawFieldOfView();
    }

    void PopulateWaypoints() {
        if (waypointsHolder == null || waypointsHolder.childCount <= 0) return;

        if (waypointsHolder.childCount == waypoints.Length) {
            waypoints = new Vector3[waypointsHolder.childCount];
        }

        for (int i = 0; i < waypoints.Length; i++) {
            waypoints[i] = waypointsHolder.GetChild(i).position;
        }
    }

    void VisionCheck() {
        var newPlayerSpotted = SpotPlayer();
        if (newPlayerSpotted) {            
            currentDetectionTime -= Time.deltaTime;
            audioSource.volume = Mathf.Lerp(minMaxVolume.x, minMaxVolume.y, volumeCurve.Evaluate(Mathf.Clamp01(1f - (currentDetectionTime / detectionTime))));
            currentTimeAfterSpot = 0f;
            canMove = previousCanMove;
            if (DestinyManager.Instance != null) DestinyManager.Instance.DestinyTimeScale(this, 1 - (currentDetectionTime / detectionTime));
            if (currentDetectionTime <= 0f) {
                if (DestinyManager.Instance != null) {
                    DestinyManager.Instance.DestinyPointLose(this);
                }
                gameObject.SetActive(false);
            }
        }
        //if (viewMaterial != null) viewMaterial.color = visionGradient.Evaluate(1 - Mathf.Clamp01(currentDetectionTime / detectionTime));
        if (newPlayerSpotted != playerSpotted) {
            OnPlayerSpotted?.Invoke(newPlayerSpotted);
            audioSource.enabled = newPlayerSpotted;
            if (!newPlayerSpotted) {
                currentDetectionTime = detectionTime;
                currentTimeAfterSpot = timeAfterSpot;
                lastPuzzledRotation = guardTransform.localRotation.eulerAngles.y;
                canMove = false;
                if (DestinyManager.Instance != null) DestinyManager.Instance.LostTrack(this);
            }
        }
        playerSpotted = newPlayerSpotted;
    }

    bool SpotPlayer() {
        if (player == null) return false;
        if (Vector3.Distance(guardTransform.position, player.position) <= visionLength) {
            if (Vector3.Distance(guardTransform.position, player.position) <= visionProximity || Vector3.Angle(guardTransform.forward, (player.position - guardTransform.position).normalized) <= (visionAngle / 2f)) {
                if (!Physics.Linecast(guardTransform.position, player.position, viewMask)) {
                    return true;
                }
            }
        }
        return false;
    }

    void MoveGuard() {
        if (guardTransform == null || guardRigidbody == null) return;
        target = playerSpotted ? player.position : waypoints[currentWaypoint];
        if (!playerSpotted || Vector3.Distance(guardTransform.position, player.position) > visionProximity) {
            guardRigidbody.MovePosition(Vector3.MoveTowards(guardTransform.position, target, speed * Time.deltaTime * (playerSpotted ? spottingSpeedRatio : 1f)));
        }
        direction = (target - guardTransform.position).normalized;
        if (direction.magnitude >= 0.1f) {
            targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            angle = Mathf.SmoothDampAngle(guardTransform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            guardTransform.rotation = Quaternion.Euler(Vector3.up * angle);
        }
        if (Vector3.Distance(guardTransform.position, target) <= distanceToWaypoint) {
            if (currentWaitTime > 0f) {
                currentWaitTime -= Time.deltaTime;
            } else {
                currentWaypoint += moveUp ? 1 : -1;
                currentWaypoint %= waypoints.Length;
                if (currentWaypoint == -1) currentWaypoint = waypoints.Length - 1;
                currentWaitTime = Random.Range(minMaxWaitTime.x, minMaxWaitTime.y);
            }
        }
    }

    void GuardPuzzled() {
        currentTimeAfterSpot -= Time.deltaTime;
        if (currentTimeAfterSpot <= 0f) { canMove = previousCanMove; return; }
        var timeAfterSpotRatio = Mathf.Clamp01(currentTimeAfterSpot / timeAfterSpot);
        if (timeAfterSpotRatio <= 0.33f) { 
            targetAngle = lastPuzzledRotation - 45f;
        } else if (0.33f < timeAfterSpotRatio && timeAfterSpotRatio <= 0.66f) {
            targetAngle = lastPuzzledRotation + 45f;
        } else {
            targetAngle = lastPuzzledRotation;
        }
        angle = Mathf.SmoothDampAngle(guardTransform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        guardTransform.rotation = Quaternion.Euler(Vector3.up * angle);
    }

    void DrawFieldOfView() {
        var stepCount = Mathf.RoundToInt(visionAngle * meshResolution);
        var stepAngleSize = visionAngle / stepCount;
        viewPoints.Clear();
        var oldViewCast = new ViewCastInfo();
        for (int i = 0; i <= stepCount; i++) {
            var angle = (stepAngleSize * i) - (visionAngle / 2f);
            var newViewCast = ViewCast(angle);
            if (i > 0) {
                var edgeDistanceThresholdExceeded = Mathf.Abs(oldViewCast.Distance - newViewCast.Distance) > edgeDistanceThreshold;
                if (oldViewCast.Hit != newViewCast.Hit || (oldViewCast.Hit && newViewCast.Hit && edgeDistanceThresholdExceeded)) {
                    var edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.PointA != Vector3.zero) {
                        viewPoints.Add(edge.PointA);
                    }
                    if (edge.PointB != Vector3.zero) {
                        viewPoints.Add(edge.PointB);
                    }
                }
            }
            viewPoints.Add(newViewCast.Point);
            oldViewCast = newViewCast;
        }
        var vertexCount = viewPoints.Count + 1;
        var vertices = new Vector3[vertexCount];
        var triangles = new int[(vertexCount - 2) * 3];
        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++) {
            vertices[i + 1] = guardTransform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2) {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }
        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast) {
        var minAngle = minViewCast.Angle;
        var maxAngle = maxViewCast.Angle;
        var minPoint = Vector3.zero;
        var maxPoint = Vector3.zero;
        for (int i = 0; i < edgeResolveIterations; i++) {
            var angle = (minAngle + maxAngle) / 2;
            var newViewCast = ViewCast(angle);
            var edgeDistanceThresholdExceeded = Mathf.Abs(minViewCast.Distance - newViewCast.Distance) > edgeDistanceThreshold;
            if (newViewCast.Hit == minViewCast.Hit && !edgeDistanceThresholdExceeded) {
                minAngle = angle;
                minPoint = newViewCast.Point;
            } else {
                maxAngle = angle;
                maxPoint = newViewCast.Point;
            }
        }
        return new EdgeInfo(minPoint, maxPoint);
    }

    ViewCastInfo ViewCast(float angle) {
        RaycastHit hit;
        var guardPosition = guardTransform.position/* + Vector3.up * 1.5f*/;
        var targetPos = guardPosition + GetCartesianFromPolar(visionLength, angle);
        if (Physics.Linecast(guardPosition, targetPos, out hit, viewMask)) {
            return new ViewCastInfo(true, hit.point, hit.distance, angle);
        } else {
            return new ViewCastInfo(false,  targetPos, visionLength, angle);
        }
    }

    Vector3 GetCartesianFromPolar(float length, float angle) {
        angle = (angle + 90 - guardTransform.rotation.eulerAngles.y) * Mathf.Deg2Rad;
        return new Vector3(length * Mathf.Cos(angle), 0f, length * Mathf.Sin(angle));
    }

    void OnDrawGizmos() {   
        if (showVisionGizmos && canSee && guardTransform != null) {
            Gizmos.color = viewPointsSphereColor;
            foreach (var viewPoint in viewPoints) {
                Gizmos.DrawSphere(viewPoint, gizmosSphereSize);
            }
            var guardPosition = guardTransform.position + Vector3.up * 1.5f;
            Gizmos.color = targetLineColor;
            if (rayNumber == 0 || rayNumber == 1) Gizmos.DrawLine(guardPosition, guardPosition + GetCartesianFromPolar(visionLength, 0f));
            else {
                for (int i = 0; i < rayNumber; i++) {
                    var angle = (-visionAngle / 2f) + i * (visionAngle / (rayNumber - 1));
                    Gizmos.DrawLine(guardPosition, guardPosition + GetCartesianFromPolar(visionLength, angle));
                }
            }
        }

        if (showMovementGizmos && canMove && waypointsHolder != null && waypointsHolder.childCount > 0) {
            var startPosition = waypointsHolder.GetChild(0).position;
            var previousPosition = startPosition;
            var i = 0;
            foreach (Transform waypoint in waypointsHolder) {
                Gizmos.color = i == currentWaypoint ? activeWaypointSphereColor : inactiveWaypointSphereColor;
                Gizmos.DrawSphere(waypoint.position, gizmosSphereSize);
                Gizmos.color = waypointLineColor;
                Gizmos.DrawLine(previousPosition, waypoint.position);
                previousPosition = waypoint.position;
                i++;
            }
            Gizmos.DrawLine(previousPosition, startPosition);
        }
    }
}
