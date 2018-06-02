using LeGrandPotAuFeu.Grid;
using LeGrandPotAuFeu.Unit;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LeGrandPotAuFeu.UI {
	public class HexGameUI : MonoBehaviour {
		public HexGrid grid;
		public Text turnText;
		public Text maxEnduranceText;
		public Text currentEnduranceText;
		public Slider enduranceSlider;

		HexCell currentCell;
		HexPlayer player;
		bool canPlayerMove = false;
		int turnCount = 0;

		void OnEnable() {			
			if (!grid.player) {
				Debug.Log("No player found: adding default player...");
				grid.AddUnit(grid.GetCell((grid.cellCountX * grid.cellCountZ) / 2), 0, -1);
			}
			player = grid.player;
			var endurance = player.unitStats.endurance;
			maxEnduranceText.text = endurance.ToString();
			enduranceSlider.maxValue = endurance;

			OnTurnPlayerTurnBegin();			
		}

		void Update() {
			if (!EventSystem.current.IsPointerOverGameObject() && canPlayerMove) {
				if (Input.GetMouseButtonDown(1)) {
					StopAllCoroutines();
					StartCoroutine(DoMove());
				} else {
					DoPathfinding();
				}				
			}
		}

		public void OnTurnPlayerTurnBegin() {
			player.ResetEnduranceLeft();
			UpdateEnduranceLeft();
			turnCount++;
			UpdateTurn();
			canPlayerMove = true;
		}

		void UpdateTurn() {
			turnText.text = turnCount.ToString();			
		}

		void UpdateEnduranceLeft() {
			enduranceSlider.value = player.EnduranceLeft;
			currentEnduranceText.text = player.EnduranceLeft.ToString();
		}

		public void SetGameMode(bool isInactive) {
			enabled = !isInactive;
			gameObject.SetActive(!isInactive);

			if (isInactive) {
				grid.ClearPath();
			}				
		}

		bool UpdateCurrentCell() {
			HexCell cell = grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
			if (cell != currentCell) {
				currentCell = cell;
				return true;
			}
			return false;
		}

		void DoPathfinding() {
			if (UpdateCurrentCell()) {
				if (currentCell && player.IsValidDestination(currentCell)) {
					grid.FindPath(player, currentCell);
				} else {
					grid.ClearPath();
				}
			}
		}

		IEnumerator DoMove() {
			if (grid.HasPath) {
				canPlayerMove = false;
				yield return player.Travel(grid.GetPath());
				UpdateEnduranceLeft();
				grid.ClearPath();
				if (player.EnduranceLeft > 0) {
					canPlayerMove = true;
				}
			}
		}
	}
}