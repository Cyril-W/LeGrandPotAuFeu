using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClothTearingBehavior : MonoBehaviour {
    [System.Serializable]
    class Point {
        public Vector2 Position, LastPosition;
        public bool Locked;        
        public Point(Vector2 position, bool locked = false) {
            Position = position;
            LastPosition = Position;
            Locked = locked;
        }
        public Point(float x, float y, bool locked = false) : this(new Vector2(x, y), locked) { }
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

    [SerializeField] float gravity = 9.81f;
    [SerializeField] int maxIterations = 10;
    [SerializeField] Point[] points;
    [SerializeField, Range(0, 50)] int colNumber = 31;
    [SerializeField, Range(0, 50)] int rowNumber = 15;
    [SerializeField] int[] lockPointIndexes;
    [SerializeField] Transform pointToInstantiate;
    [SerializeField] Transform parentInstantiated;
    [SerializeField, Range(0.1f, 2f)] float distanceToCut = 0.5f;
    [SerializeField] Stick[] sticks;
    [SerializeField] MeshFilter clothMeshFilter;
    [SerializeField] LayerMask layerToHit;
    [Header("Gizmos")]
    [SerializeField] float pointSphereRadius = 0.1f;
    [SerializeField] Color pointSphereColor = Color.blue;
    [SerializeField] Color pointLockedSphereColor = Color.red;
    [SerializeField] Color stickLineColor = Color.green;
    [SerializeField] float mouseSphereRadius = 0.1f;
    [SerializeField] Color mouseSphereColor = Color.white;

    //Vector2 offset = Vector2.zero;
    int vertexCount;
    Vector3[] vertices;
    int[] triangles;
    Mesh clothMesh;
    System.Random random;
    Vector3 mousePosition;

    void OnValidate() {
        if (clothMeshFilter == null) { clothMeshFilter = GetComponentInChildren<MeshFilter>(); }
        if (lockPointIndexes == null) { return; }
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

    void Start() {
        if (clothMeshFilter != null) {
            clothMesh = new Mesh();
            clothMesh.name = "Cloth Mesh";
            clothMeshFilter.mesh = clothMesh;
        }
    }

    void OnDestroy() {
        Debug.LogWarning("Destroying instanciated cloth mesh");
        if (clothMesh != null) { Destroy(clothMesh); }
    }

    void OnEnable() {
        GenerateMesh();
        vertexCount = points.Length;
        vertices = new Vector3[vertexCount];
        var triangleNumber = (colNumber - 1) * (rowNumber - 1) * 2 * 3;
        triangles = new int[triangleNumber];
        random = new System.Random();
    }

    [ContextMenu("Generate Mesh")]
    void GenerateMesh() {
        //offset = new Vector2(transform.position.x, transform.position.y);
        if (parentInstantiated != null) {
            for (int i = parentInstantiated.childCount; i > 0; --i) {
                var gameObjectToDestroy = parentInstantiated.GetChild(0).gameObject;
                if (Application.isEditor && !Application.isPlaying) { DestroyImmediate(gameObjectToDestroy); } 
                else { Destroy(gameObjectToDestroy); }
            }
        }
        GeneratePoints();
        GenerateSticks();
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
                if (pointToInstantiate != null && parentInstantiated != null) {
                    var point = Instantiate(pointToInstantiate, new Vector3(/*offset.x + */i, /*offset.y*/ - j, 0f), Quaternion.identity, parentInstantiated);
                    point.name = "Point " + pointIndex + " [" + i + "," + -j + "]";
                }
                pointIndex++;
            }
        }
        points = newPoints.ToArray();
    }

    void GenerateSticks() {
        var newSticks = new List<Stick>();
        var pointIndex = 0;
        for (int j = 0; j < rowNumber; j++) {
            for (int i = 0; i < colNumber; i++) {
                Stick newStick;
                if (i < colNumber - 1 && pointIndex + 1 < points.Length) {
                    newStick = new Stick(points[pointIndex], points[pointIndex + 1]);
                    newSticks.Add(newStick);
                }
                if (pointIndex + colNumber < points.Length) {
                    newStick = new Stick(points[pointIndex], points[pointIndex + colNumber]);
                    newSticks.Add(newStick);
                }
                pointIndex++;
            }
        }
        sticks = newSticks.ToArray();
    }

    void FixedUpdate() {
        HandleMouse();
        Simulate();
        DrawCloth();
    }
    
    void HandleMouse() {        
        if (Input.GetMouseButton(0)) {
            var screenPosition = Input.mousePosition;
            var ray = Camera.main.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out var hitData)) {
                mousePosition = hitData.point;
                foreach (var stick in sticks) {
                    var stickCenter = (stick.PointA.Position + stick.PointB.Position) / 2f;
                    float dist = Vector3.Distance(mousePosition, stickCenter);
                    if (dist <= distanceToCut) {
                        stick.IsActive = false;
                    }
                }
            }                        
        }
    }

    void Simulate() {
        var pointsToEvaluate = points.OrderBy(o => random.Next()).ToArray();
        //points.Shuffle();
        Vector2 positionBeforeUpdate;
        foreach (var point in pointsToEvaluate) {
            if (!point.Locked) {
                positionBeforeUpdate = point.Position;
                point.Position += point.Position - point.LastPosition;
                point.Position += Vector2.down * gravity * Time.deltaTime * Time.deltaTime;
                point.LastPosition = positionBeforeUpdate;
            }
        }
        Vector2 stickCenter, stickDirection;
        for (int i = 0; i < maxIterations; i++) {       
            foreach (var stick in sticks) {
                if (!stick.IsActive) { continue; }
                stickCenter = (stick.PointA.Position + stick.PointB.Position) / 2f;
                stickDirection = (stick.PointA.Position - stick.PointB.Position).normalized;
                if (!stick.PointA.Locked) {
                    stick.PointA.Position = stickCenter + stickDirection * stick.Length / 2f;
                }
                if (!stick.PointB.Locked) {
                    stick.PointB.Position = stickCenter - stickDirection * stick.Length / 2f;
                }
            }
        }
    }

    void DrawCloth() {
        if (clothMesh == null) { return; }
        for (int i = 0; i < vertexCount; i++) {
            vertices[i] = points[i].Position;
        }
        var triangleIndex = -1;
        //var pointIndex = 0;
        for (int j = 0; j < rowNumber - 1; j++) {
            for (int i = 0; i < colNumber - 1; i++) {
                var pointIndex = i + j * colNumber;
                triangles[++triangleIndex] = pointIndex;
                triangles[++triangleIndex] = pointIndex + 1;
                triangles[++triangleIndex] = pointIndex + 1 + colNumber;
                triangles[++triangleIndex] = pointIndex + 1 + colNumber;
                triangles[++triangleIndex] = pointIndex + colNumber;
                triangles[++triangleIndex] = pointIndex;
                //pointIndex++;
            }
        }
        clothMesh.Clear();
        clothMesh.vertices = vertices;
        clothMesh.triangles = triangles;
        clothMesh.RecalculateNormals();
    }

    void OnDrawGizmos() {
        Gizmos.matrix = transform.localToWorldMatrix;
        if (points != null) {
            foreach (var point in points) {
                Gizmos.color = point.Locked ? pointLockedSphereColor : pointSphereColor;
                Gizmos.DrawSphere(point.Position, pointSphereRadius);
            }
        }
        if (sticks != null) {
            Gizmos.color = stickLineColor;
            foreach (var stick in sticks) {
                if (stick.IsActive) { Gizmos.DrawLine(stick.PointA.Position, stick.PointB.Position); }
            }
        }
        Gizmos.color = mouseSphereColor;
        Gizmos.DrawSphere(mousePosition, mouseSphereRadius);
    }
}
