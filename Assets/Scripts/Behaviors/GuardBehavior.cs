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
    [SerializeField, Range(0.1f, 360f)] float visionAngle = 45f;
    [SerializeField, Range(0.1f, 100f)] float visionLength = 1f;
    [SerializeField] float visionProximity = 0.3f;
    [SerializeField] float visionOffset = 1f;
    [SerializeField] LayerMask viewMask;
    [SerializeField] float detectionTime = 2f;
    [SerializeField] float detectionDistance = 0.5f;
    [SerializeField] int edgeResolveIterations = 6;
    [SerializeField] float edgeDistanceThreshold = .5f;
    [SerializeField, Range(0f, 1f)] float meshResolution = .5f;
    [SerializeField] MeshFilter coneViewMeshFilter;
    [SerializeField] MeshRenderer circleViewMeshRenderer;
    //[SerializeField] Gradient visionGradient;
    [SerializeField] AudioSource audioSource;
    [SerializeField] Vector2 minMaxVolume = new Vector2(0f, 0.5f);
    [SerializeField] AnimationCurve volumeCurve;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] float lineWidth = 0.5f;
    [Header("Movement")]
    [SerializeField] bool canMove = true;
    [SerializeField] float speed = 5f;
    [SerializeField, Range(0f, 2f)] float spottingSpeedRatio = 0.75f;
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
    [Header("Other")]
    [SerializeField, ReadOnly] Transform player;
    [SerializeField, ReadOnly] Rigidbody guardRigidbody;
    [SerializeField, ReadOnly] MeshRenderer coneViewMeshRenderer;

    List<Vector3> viewPoints = new List<Vector3>();
    Vector3[] waypoints;
    Vector3 direction, target, offsetGuardPosition, offsetPlayerPosition; 
    Mesh viewMesh;
    Material coneViewMaterial, circleViewMaterial;
    int currentWaypoint = 0;
    float currentWaitTime = 0f, turnSmoothVelocity, targetAngle, angle, currentDetectionTime, currentTimeAfterSpot = 0f, lastPuzzledRotation, currentDistance;
    bool playerSpotted = false, previousCanMove;

    static readonly string MATERIAL_FLOAT_FILL = "_Fill";
    static readonly string MATERIAL_FLOAT_VISIONLENGTH = "_VisionLength";

    void OnValidate() {
        TryFillNull();        
        previousCanMove = canMove;
        if (lineRenderer != null) {
            lineRenderer.startWidth = lineRenderer.endWidth = lineWidth;
        }
    }

    void OnDestroy() {
        Debug.LogWarning("Destroying instanciated view material and mesh");
        if (coneViewMaterial != null) { Destroy(coneViewMaterial); }
        if (circleViewMaterial != null) { Destroy(circleViewMaterial); }
        if (viewMesh != null) { Destroy(viewMesh); }
    }

    void Start() {
        TryFillNull();
        waypoints = new Vector3[waypointsHolder.childCount];
        PopulateWaypoints();
        currentWaitTime = Random.Range(minMaxWaitTime.x, minMaxWaitTime.y);
        currentDetectionTime = detectionTime;     
        if (coneViewMeshFilter != null) {
            viewMesh = new Mesh();
            viewMesh.name = "View Mesh";
            coneViewMeshFilter.mesh = viewMesh;
        }
        if (coneViewMeshRenderer != null) { coneViewMaterial = coneViewMeshRenderer.material; }
        if (coneViewMaterial != null) { coneViewMaterial.SetFloat(MATERIAL_FLOAT_VISIONLENGTH, visionLength + 1f); }
        if (circleViewMeshRenderer != null) { 
            circleViewMaterial = circleViewMeshRenderer.material;
            circleViewMeshRenderer.transform.localScale = new Vector3(visionProximity * 2f, 0.01f, visionProximity * 2f);
        }
        if (circleViewMaterial != null) { circleViewMaterial.SetFloat(MATERIAL_FLOAT_VISIONLENGTH, visionProximity * 2f * 0.55f); }
        previousCanMove = canMove;
        if (audioSource != null) audioSource.enabled = false;
        if (lineRenderer != null) {
            lineRenderer.startWidth = lineRenderer.endWidth = lineWidth;
        }
        currentDistance = float.PositiveInfinity;
    }

    void TryFillNull() {
        if (player == null) { player = GameObject.FindGameObjectWithTag("Player")?.transform; }
        if (guardRigidbody == null) { guardRigidbody = guardTransform.GetComponent<Rigidbody>(); }
        //if (coneViewMeshFilter == null) { coneViewMeshFilter = GetComponent<MeshFilter>(); }
        if (coneViewMeshRenderer == null && coneViewMeshFilter != null) { coneViewMeshRenderer = coneViewMeshFilter.GetComponent<MeshRenderer>(); }
        if (lineRenderer == null) { lineRenderer = GetComponentInChildren<LineRenderer>(); }
        if (audioSource == null) { audioSource = GetComponentInChildren<AudioSource>(); }
    }

    void FixedUpdate() {
        if (updateWaypoints) {
            PopulateWaypoints();
        }

        if (guardTransform == null) { return; }

        if (canSee) {
            VisionCheck();
        }
        if (coneViewMeshRenderer != null) { coneViewMeshRenderer.enabled = canSee; }
        if (lineRenderer != null) {lineRenderer.enabled = canSee; }

        if (currentTimeAfterSpot > 0f) {
            GuardPuzzled();
        }

        if (canMove) {
            MoveGuard();
        }
    }

    void LateUpdate() {
        if (guardTransform == null) { return; }
        DrawFieldOfView();
    }

    public Vector3 GetGuardPosition() {
        return guardTransform.position;
    }

    public void SetGuardLayer(int layer) {
        if (guardTransform == null || guardTransform.GetChild(0) == null) { return; }
        foreach (Transform child in guardTransform.GetChild(0)) {
            child.gameObject.layer = layer;
        }
    }

    public void SetGuardOffsetVision(float newVisionOffset) {
        visionOffset = newVisionOffset;
    }

    void PopulateWaypoints() {
        if (waypointsHolder == null || waypointsHolder.childCount <= 0) return;

        if (waypointsHolder.childCount != waypoints.Length) {
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
            if (currentDetectionTime <= 0f) {
                // Former Catch
            }
        }
        if (coneViewMaterial != null) coneViewMaterial.SetFloat(MATERIAL_FLOAT_FILL, 1f - Mathf.Clamp01(currentDetectionTime / detectionTime));
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
        offsetGuardPosition = guardTransform.position + Vector3.up * visionOffset;
        offsetPlayerPosition = player.position + Vector3.up * visionOffset;
        if (Vector3.Distance(offsetGuardPosition, offsetPlayerPosition) <= visionLength) {
            if (Vector3.Distance(offsetGuardPosition, offsetPlayerPosition) <= visionProximity || Vector3.Angle(guardTransform.forward, (offsetPlayerPosition - offsetGuardPosition).normalized) <= (visionAngle / 2f)) {
                if (!Physics.Linecast(offsetGuardPosition, offsetPlayerPosition, viewMask)) {
                    Debug.DrawLine(offsetGuardPosition, offsetPlayerPosition, Color.red);
                    return true;
                }
            }
        }
        return false;
    }

    void MoveGuard() {
        if (guardTransform == null || guardRigidbody == null) return;
        currentDistance = Vector3.Distance(offsetGuardPosition, offsetPlayerPosition);
        if (playerSpotted) {
            if (DestinyManager.Instance != null) { DestinyManager.Instance.DestinyTimeScale(this, 1f - Mathf.Clamp01(currentDistance / (visionLength - detectionDistance))); }
            if (currentDistance <= detectionDistance) {
                if (GroupManager.Instance != null && !GroupManager.Instance.LoseRandomMember()) {
                    if (DestinyManager.Instance != null) { DestinyManager.Instance.DestinyPointLose(this); }
                } else {
                    if (DestinyManager.Instance != null) { DestinyManager.Instance.LostTrack(this); }
                }
                gameObject.SetActive(false);
                return;
            }
        }
        target = playerSpotted ? player.position : waypoints[currentWaypoint];
        if (!playerSpotted || currentDistance >= detectionDistance) {
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
        if (viewMesh == null) { return; }
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
        vertices[0] = Vector3.zero; //+ Vector3.up * visionOffset;
        if (lineRenderer != null) {
            lineRenderer.positionCount = vertexCount + 1;
            lineRenderer.SetPosition(0, vertices[0]);
            lineRenderer.SetPosition(vertexCount, vertices[0]);
        }
        for (int i = 0; i < vertexCount - 1; i++) {
            vertices[i + 1] = guardTransform.InverseTransformPoint(new Vector3(viewPoints[i].x, 0f, viewPoints[i].z)/*viewPoints[i]*/);
            if (i < vertexCount - 2) {
                triangles[i * 3] = i + 2;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = 0;
            }
            if (lineRenderer != null) {
                lineRenderer.SetPosition(i + 1, vertices[i + 1]);
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
        var targetPos = offsetGuardPosition + GetCartesianFromPolar(visionLength, angle);
        if (Physics.Linecast(offsetGuardPosition, targetPos, out hit, viewMask)) {
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
            var guardPos = Application.isPlaying ? offsetGuardPosition : guardTransform.position + Vector3.up * visionOffset;
            Gizmos.color = activeWaypointSphereColor;
            Gizmos.DrawWireSphere(guardPos, detectionDistance);
            Gizmos.color = viewPointsSphereColor;
            Gizmos.DrawWireSphere(guardPos, visionProximity);
            foreach (var viewPoint in viewPoints) {
                Gizmos.DrawSphere(viewPoint, gizmosSphereSize);
            }            
            Gizmos.color = targetLineColor;
            if (rayNumber == 0 || rayNumber == 1) Gizmos.DrawLine(guardPos, guardPos + GetCartesianFromPolar(visionLength, 0f));
            else {
                for (int i = 0; i < rayNumber; i++) {
                    var angle = (-visionAngle / 2f) + i * (visionAngle / (rayNumber - 1));
                    Gizmos.DrawLine(guardPos, guardPos + GetCartesianFromPolar(visionLength, angle));
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
