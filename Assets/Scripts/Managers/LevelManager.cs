using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] GameObject panelVictory;

    void Start() {
        if (panelVictory) {
            panelVictory.SetActive(false);
        }
    }

    public void OnExitReached() {
        if (panelVictory) {
            panelVictory.SetActive(true);
        }
    }
}
