﻿using LeGrandPotAuFeu.Grid;
using LeGrandPotAuFeu.Unit;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LeGrandPotAuFeu.UI {
	public class HexGameUI : MonoBehaviour {
		public HexGrid grid;

		HexCell currentCell;
		HexUnit selectedUnit;

		void Awake() {
			Shader.EnableKeyword("HEX_MAP_EDIT_MODE");
		}

		void Update() {
			if (!EventSystem.current.IsPointerOverGameObject()) {
				if (Input.GetMouseButtonDown(0)) {
					DoSelection();
				} else if (selectedUnit) {
					if (Input.GetMouseButtonDown(1)) {
						DoMove();
					} else {
						DoPathfinding();
					}
				}
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

		public void SetEditMode(bool toggle) {
			enabled = !toggle;
			grid.ShowUI(!toggle);
			grid.ClearPath();
			if (toggle) {
				Shader.EnableKeyword("HEX_MAP_EDIT_MODE");
			} else {
				Shader.DisableKeyword("HEX_MAP_EDIT_MODE");
			}
		}

		void DoSelection() {
			grid.ClearPath();
			UpdateCurrentCell();
			if (currentCell && currentCell.Unit is HexPlayer) {
				selectedUnit = currentCell.Unit;
			}
		}

		void DoPathfinding() {
			if (UpdateCurrentCell()) {
				if (currentCell && selectedUnit.IsValidDestination(currentCell)) {
					grid.FindPath(selectedUnit.Location, currentCell, selectedUnit);
				} else {
					grid.ClearPath();
				}
			}
		}

		void DoMove() {
			if (grid.HasPath) {
				selectedUnit.Travel(grid.GetPath());
				grid.ClearPath();
			}
		}
	}
}