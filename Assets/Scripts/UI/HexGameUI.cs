﻿using LeGrandPotAuFeu.Grid;
using LeGrandPotAuFeu.Unit;
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
		public Button turnFinishButton;

		HexCell currentCell;
		HexUnit selectedEnemy;
		bool canPlayerMove = false;
		int turnCount = 0;

		void OnEnable() {
			HexUnit.OnFinished += OnUnitMoveFinish;

			if (!grid.Player) {
				Debug.LogWarning("No player found ; adding default player...");
				var cell = grid.GetCell(0).GetNeighbor(Utility.HexDirection.NE);
				grid.AddUnit(cell, 0, 0);
			}
			var endurance = grid.Player.EnduranceLeft;
			maxEnduranceText.text = endurance.ToString();
			enduranceSlider.maxValue = endurance;
			grid.ResetVisibility();

			OnPlayerTurnBegin();	
		}

		void OnDisable() {
			HexUnit.OnFinished -= OnUnitMoveFinish;
		}

		void Update() {
			if (!EventSystem.current.IsPointerOverGameObject()) { 
				if (Input.GetMouseButtonDown(0)) {
					DoSelection();
				}
				if (canPlayerMove) {
					if (Input.GetMouseButtonDown(1)) {
						DoMove();
					} else {
						DoPathfinding();
					}
				}		
			}
		}

		public void SetGameMode(bool isInactive) {
			enabled = !isInactive;
			gameObject.SetActive(!isInactive);

			if (isInactive) {
				if (selectedEnemy) {
					selectedEnemy.IsSelected = false;
					selectedEnemy = null;
				}
				grid.ClearPath();
			}				
		}

		public void OnClickTurnFinish() {
			turnFinishButton.interactable = false;
			canPlayerMove = false;
			
			//
		}

		void OnPlayerTurnBegin() {
			grid.Player.ResetEnduranceLeft();
			UpdateEnduranceLeft();
			turnCount++;
			UpdateTurn();
			turnFinishButton.interactable = true;
			canPlayerMove = true;
		}

		void OnUnitMoveFinish(HexUnit unit) {
			if (unit.Type == 0) {
				UpdateEnduranceLeft();
				grid.ClearPath();
				if (grid.Player.EnduranceLeft > 0) {
					canPlayerMove = true;
				}
			} else {
				foreach (var enemy in grid.Enemies) {
					if (enemy.EnduranceLeft == 0) {
						return;
					}
				}
				OnPlayerTurnBegin();
			}
		}

		void UpdateTurn() {
			turnText.text = turnCount.ToString();			
		}

		void UpdateEnduranceLeft() {
			enduranceSlider.value = grid.Player.EnduranceLeft;
			currentEnduranceText.text = grid.Player.EnduranceLeft.ToString();
		}

		void DoSelection() {
			if (selectedEnemy) {
				selectedEnemy.IsSelected = false;
			}

			if (currentCell) {
				var newEnemy = currentCell.Unit;
				if (newEnemy && newEnemy.Type > 0) {
					if (!selectedEnemy || selectedEnemy != newEnemy) {
						selectedEnemy = newEnemy;
						selectedEnemy.IsSelected = true;
						return;
					}			
				}
			}

			if (selectedEnemy) {
				selectedEnemy = null;
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
				if (currentCell && grid.Player.IsValidDestination(currentCell)) {
					grid.FindPath(grid.Player, currentCell);
				} else {
					grid.ClearPath();
				}
			}
		}

		void DoMove() {
			if (grid.HasPath) {
				grid.Player.Travel(grid.GetPath());
				grid.ClearPath();
			}
		}
	}
}