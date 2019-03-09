using System;
using System.Collections;
using UnityEngine;

public class MovementHandler : MonoBehaviour
{
    [SerializeField] Transform transformToMove;
    [SerializeField] CellManager currentCell;
    [SerializeField] bool isPlayer;
    [SerializeField] float timeToMove;
    [SerializeField] string shaderParameterName;
    [SerializeField] Renderer rendererToInterpolate;
    [SerializeField] float timeToInterpolate;
    [SerializeField] Vector2 shaderParameterRange;

    bool canMove;

    void Start() {
        StartCoroutine(InterpolateShaderParameter());
        RequestForMovement(CellDirection.N);
        RequestForMovement(CellDirection.S);
        RequestForMovement(CellDirection.E);
        RequestForMovement(CellDirection.W);
        canMove = true;
    }

    IEnumerator InterpolateShaderParameter() {
        var t = 0f;
        var step = shaderParameterRange.x;
        while (t < 1f) {
            t += Time.deltaTime / timeToInterpolate;
            step = Mathf.SmoothStep(shaderParameterRange.x, shaderParameterRange.y, t);
            rendererToInterpolate.material.SetFloat(shaderParameterName, step);
            yield return null;
        }
    }

    bool RequestForMovement(CellDirection cellDirection) {
        var newCellManager = currentCell.GetOrSetCellManager(cellDirection);
        if (isPlayer && newCellManager != null) {
            newCellManager.IsClickable = true;
            newCellManager.OnClicked.AddListener(MoveToCellManager);
        }
        return newCellManager != null;
    }

    void LeaveCurrentCell() {
        if (!isPlayer) {
            return;
        }

        for (int i = 0; i < Enum.GetValues(typeof(CellDirection)).Length; i++) {
            var newCellManager = currentCell.GetOrSetCellManager((CellDirection)i);
            if (newCellManager != null) {
                newCellManager.IsClickable = false;
                newCellManager.OnClicked.RemoveListener(MoveToCellManager);
            }
        }
    }

    void MoveToCellManager(CellManager cellManager) {
        if (canMove) {
            canMove = false;
            LeaveCurrentCell();
            currentCell = cellManager;
            StartCoroutine(MoveToPosition(currentCell.Coordinates));
        }
    }

    IEnumerator MoveToPosition(Vector3 destination) {
        var currentPos = transformToMove.position;
        var t = 0f;
        while (t < 1) {
            t += Time.deltaTime / timeToMove;
            transformToMove.position = Vector3.Lerp(currentPos, destination, t);
            yield return null;
        }
        RequestForMovement(CellDirection.N);
        RequestForMovement(CellDirection.S);
        RequestForMovement(CellDirection.E);
        RequestForMovement(CellDirection.W);
        canMove = true;
    }
}
