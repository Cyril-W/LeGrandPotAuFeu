using UnityEngine;

public class CellManager : MonoBehaviour {
    [Header("Drag'n'drop")]
    [SerializeField] Transform cellTransform;
    [SerializeField] Renderer cellRenderer;
    [Header("Coordinates")]
    [SerializeField] int x;
    [SerializeField] int y;
    [SerializeField] int z;
    [Header("Clickable Shader")]
    [SerializeField] string shaderParameterName;
    [SerializeField] float notClickable;
    [SerializeField] float clickable;

    public int X { get { return x; } set { x = value; } }
    public int Y { get { return y; } set { y = value; } }
    public int Z { get { return z; } set { z = value; } }
    public Vector3 Coordinates { get { return new Vector3(X, Y, Z); } }

    public bool IsClickable {
        get { return isClickable; }
        set {
            isClickable = value;
            cellRenderer.material.SetFloat(shaderParameterName, isClickable ? clickable : notClickable);
        }
    }
    public CellManagerEvent OnClicked;

    CellManager cellManagerN, cellManagerS, cellManagerE, cellManagerW;
    bool isClickable;

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

    public CellManager GetOrSetCellManager(CellDirection cellDirection) {
        var cellManager = GetCellManager(cellDirection);
        if (cellManager == null) {
            cellManager = CellsManager.GetNeighbor(this, cellDirection);
            if (cellManager != null) {
                SetCellManager(cellManager, cellDirection);
            }
        }
        return cellManager;
    }

    CellManager GetCellManager(CellDirection cellDirection) {
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

    void SetCellManager(CellManager cellManager, CellDirection cellDirection) {
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

    public Vector3 GetNeighboorCoordinates(CellDirection cellDirection) {
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
