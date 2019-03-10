using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerManager : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] Transform playerTransform;
    [SerializeField] CellManager currentCell;
    [SerializeField] int visibilityRange;
    [SerializeField] float secondsToMove;
    [Header("Animation")]
    [SerializeField] Animator playerAnimator;
    [SerializeField] BoolName boolIsMoving;

    [SerializeField] UnityEvent onPlayerMove;

    public CellManager CurrentCell { get { return currentCell; } }
    public bool CanMove { get; set; }

    void Start() {
        RequestForMovement(CellDirection.N);
        RequestForMovement(CellDirection.S);
        RequestForMovement(CellDirection.E);
        RequestForMovement(CellDirection.W);
        CellsManager.UpdateVisibility(currentCell, visibilityRange);
        CanMove = true;
    }

    bool RequestForMovement(CellDirection cellDirection) {
        var neighbor = currentCell.GetOrSetNeighbor(cellDirection);
        if (neighbor != null) {
            neighbor.IsClickable = true;
            neighbor.OnClicked.AddListener(MoveToCellManager);
        }
        return neighbor != null;
    }

    void LeaveCurrentCell() {
        for (int i = 0; i < Enum.GetValues(typeof(CellDirection)).Length; i++) {
            var neighbor = currentCell.GetOrSetNeighbor((CellDirection)i);
            if (neighbor != null) {
                neighbor.IsClickable = false;
                neighbor.OnClicked.RemoveListener(MoveToCellManager);
            }
        }
    }

    void MoveToCellManager(CellManager cellManager) {
        if (CanMove) {
            playerAnimator.SetBool(boolIsMoving.name, true);
            CanMove = false;
            LeaveCurrentCell();
            currentCell = cellManager;
            onPlayerMove.Invoke();
            StartCoroutine(MoveToPosition(currentCell.transform));
        }
    }

    IEnumerator MoveToPosition(Transform destination) {
        var currentPos = playerTransform.position;
        var destinationPos = destination.position;
        var currentRot = playerTransform.rotation;
        var destinationRot = Quaternion.LookRotation(playerTransform.position - destination.position);
        var t = 0f;
        while (t < 1) {
            t += Time.deltaTime / secondsToMove;
            playerTransform.position = Vector3.Lerp(currentPos, destinationPos, t);
            playerTransform.rotation = Quaternion.Slerp(currentRot, destinationRot, t);
            yield return null;
        }
        RequestForMovement(CellDirection.N);
        RequestForMovement(CellDirection.S);
        RequestForMovement(CellDirection.E);
        RequestForMovement(CellDirection.W);
        CellsManager.UpdateVisibility(currentCell, visibilityRange);
        playerAnimator.SetBool(boolIsMoving.name, false);
        CanMove = true;
    }
}
