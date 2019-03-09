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
        return staticCells.FirstOrDefault(c => c.Coordinates == cell.GetNeighboorCoordinates(cellDirection));
    }
}
