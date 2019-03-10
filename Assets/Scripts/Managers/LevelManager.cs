using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] GameObject panelVictory;
    [SerializeField] CellManager objectiveCell;
    [SerializeField] PlayerManager playerManager;

    public CellManager ObjectiveCell { get { return objectiveCell; } set { objectiveCell = value; } }
    public PlayerManager PlayerManager { get { return playerManager; } set { playerManager = value; } }

    void Start() {
        if (panelVictory) {
            panelVictory.SetActive(false);
        }
    }

    public void OnExitReached() {
        if (panelVictory) {
            panelVictory.SetActive(true);
            playerManager.CanMove = false;
        }
    }
}
