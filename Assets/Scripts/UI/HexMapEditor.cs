using LeGrandPotAuFeu.Grid;
using LeGrandPotAuFeu.Unit;
using LeGrandPotAuFeu.Utility;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LeGrandPotAuFeu.UI {
	enum OptionalToggle {
		Ignore, Yes, No
	}
	public class HexMapEditor : MonoBehaviour {
		public HexGrid grid;
		public Material terrainMaterial;

		bool isDrag;
		HexDirection dragDirection;
		HexCell previousCell;

		int activeTerrainTypeIndex = -1;
		int activeElevation = 0;
		int activeWaterLevel = 0;
		int activeUrbanLevel = 0;
		int activeFarmLevel = 0;
		int activePlantLevel = 0;
		int activeSpecialIndex = 0;
		int brushSize = 0;
		int unitType = 1;

		bool applyElevation = false;
		bool applyWaterLevel = false;
		bool applyUrbanLevel = false;
		bool applyFarmLevel = false;
		bool applyPlantLevel = false;
		bool applySpecialIndex = false;

		OptionalToggle roadMode, walledMode = OptionalToggle.Ignore;

		void Awake() {
			terrainMaterial.EnableKeyword("GRID_ON");
			Shader.EnableKeyword("HEX_MAP_EDIT_MODE");
		}

		void Update() {
			if (!EventSystem.current.IsPointerOverGameObject()) {
				if (Input.GetMouseButton(0)) {
					HandleInput();
					return;
				}
				if (Input.GetKey(KeyCode.LeftShift)) {
					DestroyUnit();
					return;
				} else if (Input.GetKeyDown(KeyCode.U)) {
					CreateUnit();
					return;
				}
			}
			previousCell = null;
		}

		HexCell GetCellUnderCursor() {			
			return grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition)); ;
		}

		void HandleInput() {
			HexCell currentCell = GetCellUnderCursor();
			if (currentCell) {
				if (previousCell && previousCell != currentCell) {
					ValidateDrag(currentCell);
				} else {
					isDrag = false;
				}
				EditCells(currentCell);
				previousCell = currentCell;
			} else {
				previousCell = null;
			}
		}

		void ValidateDrag(HexCell currentCell) {
			for (
				dragDirection = HexDirection.NE;
				dragDirection <= HexDirection.NW;
				dragDirection++
			) {
				if (previousCell.GetNeighbor(dragDirection) == currentCell) {
					isDrag = true;
					return;
				}
			}
			isDrag = false;
		}

		void EditCells(HexCell center) {
			int centerX = center.coordinates.X;
			int centerZ = center.coordinates.Z;

			for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++) {
				for (int x = centerX - r; x <= centerX + brushSize; x++) {
					EditCell(grid.GetCell(new HexCoordinates(x, z)));
				}
			}
			for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++) {
				for (int x = centerX - brushSize; x <= centerX + r; x++) {
					EditCell(grid.GetCell(new HexCoordinates(x, z)));
				}
			}
		}

		void EditCell(HexCell cell) {
			if (cell) {
				if (activeTerrainTypeIndex >= 0) {
					cell.TerrainTypeIndex = activeTerrainTypeIndex;
				}
				if (applyElevation) {
					cell.Elevation = activeElevation;
				}
				if (applyWaterLevel) {
					cell.WaterLevel = activeWaterLevel;
				}
				if (applySpecialIndex) {
					cell.SpecialIndex = activeSpecialIndex;
				}
				if (roadMode == OptionalToggle.No) {
					cell.RemoveRoads();
				}
				if (applyUrbanLevel) {
					cell.UrbanLevel = activeUrbanLevel;
				}
				if (applyFarmLevel) {
					cell.FarmLevel = activeFarmLevel;
				}
				if (applyPlantLevel) {
					cell.PlantLevel = activePlantLevel;
				}
				if (walledMode != OptionalToggle.Ignore) {
					cell.Walled = walledMode == OptionalToggle.Yes;
				}

				if (isDrag) {
					HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
					if (otherCell) {
						if (roadMode == OptionalToggle.Yes) {
							otherCell.AddRoad(dragDirection);
						}
					}
				}
			}
		}

		void CreateUnit() {
			HexCell cell = GetCellUnderCursor();
			if (cell && !cell.Unit && cell.Explorable) {
				var orientation = HexDirectionExtensions.GetRandomDirection();
				grid.AddUnit(cell, orientation, unitType);
			}
		}

		void DestroyUnit() {
			HexCell cell = GetCellUnderCursor();
			if (cell && cell.Unit) {
				grid.RemoveUnit(cell.Unit);
			}
		}

		public void SetRoadMode(int mode) {
			roadMode = (OptionalToggle)mode;
		}

		public void SetWalledMode(int mode) {
			walledMode = (OptionalToggle)mode;
		}

		public void SetTerrainTypeIndex(int index) {
			activeTerrainTypeIndex = index;
		}

		public void SetElevation(float elevation) {
			activeElevation = (int)elevation;
		}

		public void SetApplyElevation(bool toggle) {
			applyElevation = toggle;
		}

		public void SetApplyWaterLevel(bool toggle) {
			applyWaterLevel = toggle;
		}

		public void SetWaterLevel(float level) {
			activeWaterLevel = (int)level;
		}

		public void SetApplyUrbanLevel(bool toggle) {
			applyUrbanLevel = toggle;
		}

		public void SetUrbanLevel(float level) {
			activeUrbanLevel = (int)level;
		}

		public void SetApplyFarmLevel(bool toggle) {
			applyFarmLevel = toggle;
		}

		public void SetFarmLevel(float level) {
			activeFarmLevel = (int)level;
		}

		public void SetApplyPlantLevel(bool toggle) {
			applyPlantLevel = toggle;
		}

		public void SetPlantLevel(float level) {
			activePlantLevel = (int)level;
		}

		public void SetApplySpecialIndex(bool toggle) {
			applySpecialIndex = toggle;
		}

		public void SetSpecialIndex(float index) {
			activeSpecialIndex = (int)index;
		}

		public void SetBrushSize(float size) {
			brushSize = (int)size;
		}

		public void SetUnitType(float type) {
			unitType = (int)type;
		}

		public void ShowGrid(bool visible) {
			if (visible) {
				terrainMaterial.EnableKeyword("GRID_ON");
			} else {
				terrainMaterial.DisableKeyword("GRID_ON");
			}
		}

		public void SetEditMode(bool isActive) {
			enabled = isActive;
			gameObject.SetActive(isActive);

			grid.ShowUI(!isActive);
			if (isActive) {
				Shader.EnableKeyword("HEX_MAP_EDIT_MODE");
			} else {
				Shader.DisableKeyword("HEX_MAP_EDIT_MODE");
			}
		}
	}
}