using UnityEngine;

public class CellManager : MonoBehaviour {
    [Header("Drag'n'drop")]
    [SerializeField] Transform cellTransform;
    [Header("Coordinates")]
    [SerializeField] int x;
    [SerializeField] int y;
    [SerializeField] int z;
    [Header("Animation")]
    [SerializeField] Animator cellAnimator;
    [SerializeField] BoolName boolIsVisible;
    [SerializeField] BoolName boolIsClickable;

    public int X { get { return x; } set { x = value; } }
    public int Y { get { return y; } set { y = value; } }
    public int Z { get { return z; } set { z = value; } }
    public Vector3 Coordinates { get { return new Vector3(X, Y, Z); } }

    public bool IsVisible {
        get { return isVisible; }
        set {
            isVisible = value;
            cellAnimator.SetBool(boolIsVisible.name, isVisible);
        }
    }
    public bool IsClickable {
        get { return isClickable; }
        set {
            isClickable = value;
            cellAnimator.SetBool(boolIsClickable.name, isClickable);
        }
    }
    public CellManagerEvent OnClicked;

    CellManager cellManagerN, cellManagerS, cellManagerE, cellManagerW;
    bool isClickable, isVisible;

    void OnValidate() {
        if (cellTransform != null) {
            cellTransform.position = new Vector3(X, Y, Z);
        }
    }

    public void OnClick() {
        if (OnClicked != null && IsClickable) {
            OnClicked.Invoke(this);
        }
    }

    public CellManager GetOrSetNeighbor(CellDirection cellDirection) {
        var cellManager = GetNeighbor(cellDirection);
        if (cellManager == null) {
            cellManager = CellsManager.GetNeighbor(this, cellDirection);
            if (cellManager != null) {
                SetNeighbor(cellManager, cellDirection);
            }
        }
        return cellManager;
    }

    CellManager GetNeighbor(CellDirection cellDirection) {
        switch (cellDirection) {
            case CellDirection.N:
                return cellManagerN;
            case CellDirection.S:
                return cellManagerS;
            case CellDirection.E:
                return cellManagerE;
            case CellDirection.W:
                return cellManagerW;
            default:
                return null;
        }
    }

    void SetNeighbor(CellManager cellManager, CellDirection cellDirection) {
        switch (cellDirection) {
            case CellDirection.N:
                cellManagerN = cellManager;
                break;
            case CellDirection.S:
                cellManagerS = cellManager;
                break;
            case CellDirection.E:
                cellManagerE = cellManager;
                break;
            case CellDirection.W:
                cellManagerW = cellManager;
                break;
            default:
                break;
        }
    }

    public Vector3 GetNeighborCoordinates(CellDirection cellDirection) {
        switch (cellDirection) {
            case CellDirection.N:
                return new Vector3(X, Y, Z + 1);
            case CellDirection.S:
                return new Vector3(X, Y, Z - 1);
            case CellDirection.E:
                return new Vector3(X + 1, Y, Z);
            case CellDirection.W:
                return new Vector3(X - 1, Y, Z);
            default:
                return Vector3.zero;
        }
    }
}
