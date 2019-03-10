using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CellsManager : MonoBehaviour
{
    [SerializeField] List<CellManager> cells;

    public static List<CellManager> staticCells;

    void OnEnable() {
        staticCells = cells;
    }

    public static CellManager GetNeighbor(CellManager cell, CellDirection cellDirection) {
        return staticCells.FirstOrDefault(c => c.Coordinates == cell.GetNeighborCoordinates(cellDirection));
    }

    public static void UpdateVisibility(CellManager centerCell, int visibilityRange) {
        foreach (var cell in staticCells) {
            if (!cell.IsVisible) {
                if (cell == centerCell) {
                    cell.IsVisible = true;
                } else {
                    cell.IsVisible = Mathf.Abs(centerCell.X - cell.X) + Mathf.Abs(centerCell.Z - cell.Z) <= visibilityRange;
                }
            }
        }
    }
}
