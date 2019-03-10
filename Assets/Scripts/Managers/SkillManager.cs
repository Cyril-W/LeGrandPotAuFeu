using UnityEngine;
using UnityEngine.UI;

public class SkillManager : MonoBehaviour
{
    [SerializeField] LevelManager levelManager;
    [Header("Range")]
    [SerializeField] GameObject compassGameObject;
    [SerializeField] Transform compassArrowTransform;
    [SerializeField] Button arrowBtn;
    [SerializeField] Text arrowTxt;
    [SerializeField] int arrowRecharge = 2;

    int turnBeforeNextArrow;

    void Start() {
        OnPlayerMove();
    }

    public void OnPlayerMove() {
        if (turnBeforeNextArrow > 0) {
            arrowTxt.text = turnBeforeNextArrow.ToString();
            turnBeforeNextArrow--;
        } else {
            arrowBtn.interactable = true;
            arrowTxt.text = "Arrow";
        }
        compassGameObject.SetActive(false);
    }

    public void OnRangerSkillUsed() {
        if (turnBeforeNextArrow <= 0) {
            var coordinatesDifference = levelManager.PlayerManager.CurrentCell.Coordinates - levelManager.ObjectiveCell.Coordinates;
            var lookRotation = Quaternion.LookRotation(coordinatesDifference).eulerAngles;
            lookRotation.y -= levelManager.PlayerManager.transform.rotation.eulerAngles.y;
            compassArrowTransform.localRotation = Quaternion.Euler(lookRotation);
            turnBeforeNextArrow = arrowRecharge;
            arrowBtn.interactable = false;
            arrowTxt.text = turnBeforeNextArrow.ToString();
            compassGameObject.SetActive(true);
        } else {
            Debug.LogWarning("Cannot use arrow, skill recharching");
        }
    }
}
