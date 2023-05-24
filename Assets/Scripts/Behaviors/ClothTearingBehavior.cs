using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class ClothTearingBehavior : MonoBehaviour {
    public static ClothTearingBehavior Instance { get; private set; }

    [System.Serializable]
    class Point {
        public Vector2 Position, LastPosition;//, StartPosition;
        public bool Locked;
        public bool IsActive = true;
        public Point(Vector2 position, bool locked = false, bool isActive = true) {
            Position = position;
            LastPosition = Position;
            //StartPosition = Position;
            Locked = locked;
            IsActive = isActive;
        }
        public Point(float x, float y, bool locked = false, bool isActive = true) : this(new Vector2(x, y), locked, isActive) { }
    }
    [System.Serializable]
    class Stick {
        public Point PointA, PointB;
        public float Length;
        public bool IsActive = true;
        public Stick(Point pointA, Point pointB, float length = -1f, bool isActive = true) {
            PointA = pointA;
            PointB = pointB;
            if (length >= 0f) {
                Length = length;
            } else {
                Length = Vector3.Distance(PointA.Position, PointB.Position);
            }
            IsActive = isActive;
        }
    }

    [SerializeField] float gravity = 9.81f;//-0.24f;
    //[SerializeField] float friction = 0.99f;
    [SerializeField, Range(0.5f, 10f)] float distanceToTear = 1.5f;
    [SerializeField, Range(1, 10)] int maxIterations = 3;
    [SerializeField] float minPointHeight = -75f;
    [SerializeField, HideInInspector] Point[] points;
    [SerializeField, Range(0, 50)] int colNumber = 31;
    [SerializeField, Range(0, 50)] int rowNumber = 15;
    [SerializeField] int[] lockPointIndexes;
    //[SerializeField] Transform pointToInstantiate;
    //[SerializeField] Transform parentInstantiated;
    [SerializeField, Range(0.1f, 2f)] float distanceToCut = 0.5f;
    [SerializeField, HideInInspector] Stick[] sticks;
    [SerializeField] MeshFilter clothMeshFilter;
    [SerializeField] MeshCollider clothMeshCollider;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Transform parentLineRenderers;
    [SerializeField] Vector2 startEndWidth = new Vector2(0.1f, 0f);
    [SerializeField] UnityEvent onInstanceRegistered;
    [ReadOnly] public DoorBehavior doorBehavior;
    [SerializeField] UnityEvent onClothTearingSuccess;
    [Header("Gizmos")]
    [SerializeField] float pointSphereRadius = 0.1f;
    [SerializeField] Color pointSphereColor = Color.blue;
    [SerializeField] Color pointLockedSphereColor = Color.red;
    [SerializeField] Color stickLineColor = Color.green;
    [SerializeField] Color mouseSphereColor = Color.white;

    //Vector2 offset = Vector2.zero;
    Vector3[] vertices;
    //int[] triangles;
    Mesh clothMesh;
    System.Random random;
    Vector3 currentMousePosition;
    int[][] sticksPerPoint;
    LineRenderer[] lineRenderers;

    void OnValidate() {
        if (clothMeshFilter == null) { clothMeshFilter = GetComponentInChildren<MeshFilter>(); }
        if (clothMeshCollider == null) { clothMeshCollider = GetComponentInChildren<MeshCollider>(); }
        if (lineRenderer == null) { lineRenderer = GetComponentInChildren<LineRenderer>(true); }
        if (lineRenderer != null) { lineRenderer.gameObject.SetActive(false); }
        if (lockPointIndexes == null || Application.isPlaying) { return; }
        var maxIndex = colNumber * rowNumber;
        for (int i = 0; i < lockPointIndexes.Length; i++) {
            var lockPointIndex = lockPointIndexes[i];
            if (lockPointIndex < 0 || lockPointIndex >= maxIndex) {
                lockPointIndex = Mathf.Clamp(lockPointIndex, 0, maxIndex - 1);
                lockPointIndexes[i] = lockPointIndex;
            }
        }
        /*for (int i = 0; i < lockPoints.Length; i++) {
            var lockPoint = lockPoints[i];
            bool hasChanged = false;
            if (lockPoint.x < 0 || lockPoint.x >= colNumber) { 
                lockPoint.x = Mathf.Clamp(lockPoint.x, 0, colNumber - 1);
                hasChanged = true;
            }
            if (lockPoint.y < 0 || lockPoint.y >= rowNumber) { 
                lockPoint.y = Mathf.Clamp(lockPoint.y, 0, rowNumber - 1);
                hasChanged = true;
            }
            if (hasChanged) { lockPoints[i] = lockPoint; }
        }*/
    }

    void Awake() {
        if (Instance == null || Instance != this) { Instance = this; onInstanceRegistered?.Invoke(); }
    }

    void Start() {
        if (clothMeshFilter != null) {
            clothMesh = new Mesh();
            clothMesh.name = "Cloth Mesh";
            clothMeshFilter.mesh = clothMesh;
        }
        if (clothMeshCollider != null) {
            clothMeshCollider.sharedMesh = clothMesh;
        }
    }

    void OnDestroy() {
        Debug.LogWarning("Destroying instanciated cloth mesh");
        if (clothMesh != null) { Destroy(clothMesh); }
    }

    void OnEnable() {      
        if (lineRenderer != null && parentLineRenderers != null) {
            lineRenderer.startWidth = startEndWidth.x;
            lineRenderer.endWidth = startEndWidth.y;
            lineRenderer.gameObject.SetActive(false);            
        }
        GenerateMesh();
        vertices = new Vector3[points.Length];
        random = new System.Random();
    }

    public void ClothTearFinish() {
        onClothTearingSuccess?.Invoke();
        if (doorBehavior != null) {
            doorBehavior.InteractSuccess();
            doorBehavior = null;
        }
    }

    public void ClothTearAbort() {
        if (doorBehavior != null) { 
            doorBehavior.InteractAbort();
            doorBehavior = null; }
    }

    [ContextMenu("Generate Mesh")]
    void GenerateMesh() {
        //offset = new Vector2(transform.position.x, transform.position.y);
        DestroyInstantiated();
        GeneratePoints();
        GenerateSticks();
    }

    [ContextMenu("Destroy Instantiated")]
    void DestroyInstantiated() {
        /*if (parentInstantiated != null) {
           for (int i = parentInstantiated.childCount; i > 0; --i) {
               var gameObjectToDestroy = parentInstantiated.GetChild(0).gameObject;
               if (Application.isEditor && !Application.isPlaying) { DestroyImmediate(gameObjectToDestroy); } 
               else { Destroy(gameObjectToDestroy); }
           }
        }*/
        lineRenderers = null;
        if (parentLineRenderers != null) {
            for (int i = parentLineRenderers.childCount; i > 0; --i) {
                var gameObjectToDestroy = parentLineRenderers.GetChild(0).gameObject;
                //if (Application.isEditor && !Application.isPlaying) {
                    //Debug.Log("Destroy immediate");
                    DestroyImmediate(gameObjectToDestroy); 
                /*} else {
                    Debug.Log("Detroy " + gameObjectToDestroy.name);
                    Destroy(gameObjectToDestroy);
                }*/
            }
        }
    }

    void GeneratePoints() {
        var newPoints = new List<Point>();
        var pointIndex = 0;
        for (int j = 0; j < rowNumber; j++) {
            for (int i = 0; i < colNumber; i++) {
                var pointPosition = new Vector2(/*offset.x + */i, /*offset.y*/ - j);
                var isLocked = lockPointIndexes.Contains(pointIndex);// Where(p => Vector2.Distance(p, pointPosition) <= 0.1f).Count() > 0;
                var newPoint = new Point(pointPosition, isLocked);
                newPoints.Add(newPoint);
                //if (pointToInstantiate != null && parentInstantiated != null) {
                    //var point = Instantiate(pointToInstantiate, new Vector3(/*offset.x + */i, /*offset.y*/ - j, 0f), Quaternion.identity, parentInstantiated);
                    //point.name = "Point " + pointIndex + " [" + i + "," + -j + "]";
                //}
                pointIndex++;
            }
        }
        points = newPoints.ToArray();
    }

    void GenerateSticks() {
        var newSticks = new List<Stick>();
        //var pointIndex = 0;
        var stickIndex = 0;
        sticksPerPoint = new int[points.Length][];
        var newLineRenderers = new List<LineRenderer>();
        for (int j = 0; j < rowNumber; j++) {
            for (int i = 0; i < colNumber; i++) {
                var pointIndex = i + j * colNumber;
                Stick newStick;
                var sticksIndex = new int[2] { -1, -1 };
                if (i < colNumber - 1 && pointIndex + 1 < points.Length) {
                    newStick = new Stick(points[pointIndex], points[pointIndex + 1]);
                    newSticks.Add(newStick);
                    if (Application.isPlaying && lineRenderer != null && parentLineRenderers != null) {
                        var newLineRenderer = Instantiate(lineRenderer, points[pointIndex].Position, Quaternion.identity, parentLineRenderers);
                        newLineRenderer.name = "Line " + stickIndex + " [" + pointIndex + "," + (pointIndex + 1) + "]";
                        newLineRenderers.Add(newLineRenderer);
                    }
                    sticksIndex[0] = stickIndex++;
                }
                if (pointIndex + colNumber < points.Length) {
                    newStick = new Stick(points[pointIndex], points[pointIndex + colNumber]);
                    newSticks.Add(newStick);
                    if (Application.isPlaying && lineRenderer != null && parentLineRenderers != null) {
                        var newLineRenderer = Instantiate(lineRenderer, points[pointIndex].Position, Quaternion.identity, parentLineRenderers);
                        newLineRenderer.name = "Line " + stickIndex + " [" + pointIndex + "," + (pointIndex + colNumber) + "]";
                        newLineRenderers.Add(newLineRenderer);
                    }
                    sticksIndex[1] = stickIndex++;
                }
                sticksPerPoint[pointIndex] = sticksIndex;
                //pointIndex++;
            }
        }
        lineRenderers = newLineRenderers.ToArray();
        sticks = newSticks.ToArray();
    }

    void FixedUpdate() {
#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.Quote)) {
            Debug.LogWarning("[²] - Cloth tearing cheat code");
            ClothTearFinish();
        }
#endif
        Simulate();
        DrawCloth();
    }
    
    public void HandleMouseCut(Vector3 mousePosition) {
        currentMousePosition = mousePosition;
        foreach (var stick in sticks) {
            var stickCenter = (stick.PointA.Position + stick.PointB.Position) / 2f;
            float dist = Vector3.Distance(mousePosition, transform.TransformPoint(stickCenter));
            if (dist <= distanceToCut) {
                stick.IsActive = false;
            }
        }
    }

    void Simulate() {
        var pointsToEvaluate = points.OrderBy(o => random.Next()).ToArray();
        //points.Shuffle();
        Vector2 positionBeforeUpdate;
        //Vector2 velocity;
        foreach (var point in pointsToEvaluate) {
            if (point.IsActive) {
                if (point.Locked) {
                    //point.Position = point.StartPosition; 
                    //point.LastPosition = point.StartPosition;
                } else {
                    positionBeforeUpdate = point.Position;
                    point.Position += point.Position - point.LastPosition;
                    //velocity = (point.Position - point.LastPosition) * friction;
                    point.LastPosition = positionBeforeUpdate;
                    //point.Position += velocity;
                    point.Position += Vector2.down * gravity * Time.deltaTime * Time.deltaTime;
                    if (point.Position.y <= minPointHeight) {
                        point.IsActive = false;
                    }
                }
            }
        }
        var sticksToEvaluate = sticks.OrderBy(o => random.Next()).ToArray();
        Vector2 stickCenter, stickDirection;
        for (int i = 0; i < maxIterations; i++) {
            /*var startDistance = 1f;
            var changeDirection = Vector2.zero;*/
            foreach (var stick in sticksToEvaluate) {
                if (stick.IsActive && !(stick.PointA.IsActive & stick.PointB.IsActive)) { stick.IsActive = false; }
                if (stick.IsActive && Vector3.Distance(stick.PointA.Position, stick.PointB.Position) >= distanceToTear) { stick.IsActive = false; }
                if (!stick.IsActive) { continue; }
                stickCenter = (stick.PointA.Position + stick.PointB.Position) / 2f;
                stickDirection = (stick.PointA.Position - stick.PointB.Position).normalized;
                if (!stick.PointA.Locked) { stick.PointA.Position = stickCenter + stickDirection * stick.Length / 2f; }
                if (!stick.PointB.Locked) { stick.PointB.Position = stickCenter - stickDirection * stick.Length / 2f; }
                /*var dist = (stick.PointA.Position - stick.PointB.Position).magnitude;
                var error = Mathf.Abs(dist - startDistance);
                if (dist > startDistance) {
                    changeDirection = (stick.PointA.Position - stick.PointB.Position).normalized;
                } else if (dist < startDistance) {
                    changeDirection = (stick.PointB.Position - stick.PointA.Position).normalized;
                }
                var changeAmount = changeDirection * error;*/
                //if (true/*!stick.PointA.Locked*/) { stick.PointA.Position -= changeAmount * 0.5f; }
                //if (true/*!stick.PointB.Locked*/) { stick.PointB.Position += changeAmount * 0.5f; }
            }
        }
    }

    void DrawCloth() {
        if (clothMesh == null) { return; }
        for (int i = 0; i < points.Length; i++) {
            vertices[i] = points[i].Position;
        }
        //var triangleNumber = (colNumber - 1) * (rowNumber - 1) * 2 * 3;
        //triangles = new int[triangleNumber];
        var trianglesList = new List<int>();
        for (int j = 0; j < rowNumber - 1; j++) {
            for (int i = 0; i < colNumber - 1; i++) {
                var pointIndex = i + j * colNumber;
                bool isQuad = true;
                var firstStickIndex = sticksPerPoint[pointIndex][0];
                if (CheckStickIndex(firstStickIndex)) { isQuad = false; } // check stick right of pointIndex
                var secondStickIndex = sticksPerPoint[pointIndex][1];
                if (CheckStickIndex(secondStickIndex)) { isQuad = false; } // check stick bottom of pointIndex
                var thirdStickIndex = sticksPerPoint[pointIndex + 1][1]; 
                if (CheckStickIndex(thirdStickIndex)) { isQuad = false; } // check stick bottom of point right of pointIndex
                var fourthStickIndex = sticksPerPoint[pointIndex + colNumber][0];
                if (CheckStickIndex(fourthStickIndex)) {  isQuad = false; } // check stick right of point bottom of pointIndex
                if(!points[pointIndex].IsActive || !points[pointIndex + 1].IsActive || !points[pointIndex + 1 + colNumber].IsActive || !points[pointIndex + colNumber].IsActive) { isQuad = false; }
                if (isQuad) {
                    // Upper triangle
                    trianglesList.Add(pointIndex);
                    trianglesList.Add(pointIndex + 1);
                    trianglesList.Add(pointIndex + 1 + colNumber);
                    // Lower triangle
                    trianglesList.Add(pointIndex + 1 + colNumber);
                    trianglesList.Add(pointIndex + colNumber);
                    trianglesList.Add(pointIndex);
                    /*triangles[++triangleIndex]*/
                } else {
                    SetLineRenderPosition(firstStickIndex);
                    SetLineRenderPosition(secondStickIndex);
                    SetLineRenderPosition(thirdStickIndex);
                    SetLineRenderPosition(fourthStickIndex);
                }
                //pointIndex++;
            }
        }
        clothMesh.Clear();
        clothMesh.vertices = vertices;
        clothMesh.triangles = trianglesList.ToArray();
        clothMesh.RecalculateNormals();
        clothMeshCollider.sharedMesh = null;
        clothMeshCollider.sharedMesh = clothMesh;
    }

    void SetLineRenderPosition(int stickIndex) {
        if (lineRenderers.Length > 0) {
            var currentLineRender = lineRenderers[stickIndex];
            if (sticks[stickIndex].IsActive) {
                var posA = sticks[stickIndex].PointA.Position;
                currentLineRender.SetPosition(0, new Vector3(posA.x, posA.y, 0.1f));
                var posB = sticks[stickIndex].PointB.Position;
                currentLineRender.SetPosition(1, new Vector3(posB.x, posB.y, 0.1f));
            }
            currentLineRender.gameObject.SetActive(sticks[stickIndex].IsActive);
        }
    }

    bool CheckStickIndex(int stickIndex) {
        return stickIndex < 0 || !sticks[stickIndex].IsActive;
    }

    void OnDrawGizmos() {
        Gizmos.matrix = transform.localToWorldMatrix;
        if (points != null) {
            foreach (var point in points) {
                if (point.IsActive) {
                    Gizmos.color = point.Locked ? pointLockedSphereColor : pointSphereColor;
                    Gizmos.DrawSphere(point.Position, pointSphereRadius);
                }
            }
        }
        if (sticks != null) {
            Gizmos.color = stickLineColor;
            foreach (var stick in sticks) {
                if (stick.IsActive) { Gizmos.DrawLine(stick.PointA.Position, stick.PointB.Position); }
            }
        }
        Gizmos.color = mouseSphereColor;
        Gizmos.DrawSphere(currentMousePosition, distanceToCut);
    }
}
